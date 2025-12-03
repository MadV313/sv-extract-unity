using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class TopDownCameraController : MonoBehaviour
{
    [Header("Follow Target")]
    public Transform target; // Player's CameraTarget child

    [Header("Player Link (for FP drag)")]
    [Tooltip("Root transform to rotate when dragging in near/FP view. If left empty, we'll try to find it from the target's CharacterController parent.")]
    public Transform playerRoot;

    [Header("Orbit & Zoom")]
    public bool holdLeftMouseToOrbit = false; // false = Right Mouse Button
    public float orbitSpeed  = 180f;   // match player turn speed
    public float zoomSpeed   = 2.5f;
    public Vector2 heightLimits = new Vector2(1.0f, 12f);
    public Vector2 radiusLimits = new Vector2(0.00f, 12f);

    [Header("Starting Values")]
    public float height = 11f;
    public float radius = 10f;

    [Header("Close-zoom tweak (no real FP mode)")]
    public float minBackRadius   = 0.30f;
    public float forwardNudgeNear = 0.60f;
    public float nudgeFadeStart   = 0.90f;
    public float nearClipClose    = 0.03f;

    [Header("Stability (anti-kick)")]
    public float safeBackMargin = 0.10f;
    public float nearAimEnter   = 0.50f;
    public float nearAimExit    = 0.70f;

    [Header("Near view orientation (slight downlook)")]
    public float nearEyeHeight = 0.5f;
    public float nearPitchDeg  = 9f;
    public float proxyLookAhead = 0.90f;   // 0.9–1.1 for more FP feel

    [Header("Quality of life")]
    [Tooltip("Auto recentre the camera behind the player when pressing W (3rd-person only).")]
    public bool snapBehindOnMove = true;
    [Tooltip("Time (seconds) for smooth snap-behind in 3rd-person.")]
    public float snapTime = 0.60f; // locked per your note
    [Tooltip("Don't snap-behind immediately after a mouse-rotate. Wait this long (seconds).")]
    public float snapAfterDragDelay = 0.75f; // locked per your note

    // Expose whether we're in near/FP view for the movement script.
    public bool IsNearView { get; private set; }

    // PUBLIC yaw for other scripts. Writes ignored while near to avoid feedback loops.
    public float yaw
    {
        get => _yaw;
        set
        {
            if (usingProxy || target == null) return;         // ignore writes while near
            _yaw = NormalizeAngle(value);                      // world yaw
            float targetYaw = NormalizeAngle(target.eulerAngles.y);
            orbitAngle = NormalizeAngle(_yaw - targetYaw);     // world -> local
            ApplyOffset(false);
        }
    }

    private float _yaw = 0f;               // world yaw we expose
    private float orbitAngle = 0f;         // LOCAL orbit angle around target (0 = behind)
    private float snapVel;                 // for SmoothDampAngle

    private float recentDragTimer = 0f;    // time since last RMB drag

    private CinemachineVirtualCamera vcam;
    private CinemachineTransposer    transposer;
    private CinemachineComposer      composer;
    private Camera                   mainCam;
    private float                    originalNearClip = 0.1f;

    // LookAt proxy (parented to target)
    private Transform lookAtProxy;
    private bool usingProxy = false;

    void Awake()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();

        // Ensure Body is Transposer
        transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();
        if (!transposer)
        {
            var framing = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (framing) DestroyImmediate(framing);
            vcam.AddCinemachineComponent<CinemachineTransposer>();
            transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();
        }
        transposer.m_BindingMode = CinemachineTransposer.BindingMode.LockToTargetWithWorldUp;
        transposer.m_XDamping = 0f;
        transposer.m_YDamping = 0f;
        transposer.m_ZDamping = 0f;

        // Ensure Composer exists (Aim)
        composer = vcam.GetCinemachineComponent<CinemachineComposer>();
        if (!composer) composer = vcam.AddCinemachineComponent<CinemachineComposer>();
        composer.m_TrackedObjectOffset = Vector3.zero;
        composer.m_SoftZoneWidth  = 0.8f;
        composer.m_SoftZoneHeight = 0.8f;
        composer.m_DeadZoneWidth  = 0.0f;
        composer.m_DeadZoneHeight = 0.0f;

        mainCam = Camera.main;
        if (mainCam) originalNearClip = mainCam.nearClipPlane;

        // Create proxy
        var go = new GameObject("VCam_LookAtProxy (runtime)");
        go.hideFlags = HideFlags.DontSave;
        lookAtProxy = go.transform;

        // Defensive: remove recomposer/noise that can fight our aim
        foreach (var ext in GetComponents<CinemachineExtension>())
        {
            if (ext is CinemachineRecomposer) Destroy(ext);
        }
    }

    void OnDestroy()
    {
        if (lookAtProxy) Destroy(lookAtProxy.gameObject);
    }

    void Start()
    {
        if (!target) target = (Transform)vcam.Follow;

        // Try to auto-detect player root if not set: prefer a CharacterController parent
        if (!playerRoot && target)
        {
            var cc = target.GetComponentInParent<CharacterController>();
            playerRoot = cc ? cc.transform : (target.parent ? target.parent : target);
        }

        if (lookAtProxy && target)
        {
            lookAtProxy.SetParent(target, worldPositionStays: false);
            lookAtProxy.localPosition = Vector3.zero;
        }

        if (vcam && target) vcam.LookAt = target;

        // 0° = behind target for our local offset math
        orbitAngle = 0f;
        _yaw = NormalizeAngle(target ? target.eulerAngles.y + orbitAngle : 0f);

        ApplyOffset(true);
        UpdateYawForMovement();
    }

    void Update()
    {
        if (!target) return;

        // timers
        if (recentDragTimer > 0f) recentDragTimer -= Time.deltaTime;

        // Zoom
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            height = Mathf.Clamp(height - scroll * zoomSpeed, heightLimits.x, heightLimits.y);
            radius = Mathf.Clamp(radius - scroll * (zoomSpeed * 0.9f), radiusLimits.x, radiusLimits.y);
            ApplyOffset(false);
            UpdateYawForMovement();
        }

        // Orbit (RMB or LMB depending on setting)
        bool dragBtn = holdLeftMouseToOrbit ? Input.GetMouseButton(0) : Input.GetMouseButton(1);
        if (dragBtn)
        {
            float dx = Input.GetAxis("Mouse X");
            float dynamicOrbit = orbitSpeed * (1f + Mathf.Abs(dx) * 0.8f);

            if (usingProxy)
            {
                // ---------- FP/near: rotate PLAYER + camera together ----------
                if (playerRoot)
                {
                    float newYaw = NormalizeAngle(playerRoot.eulerAngles.y + dx * dynamicOrbit * Time.deltaTime);
                    playerRoot.rotation = Quaternion.Euler(0f, newYaw, 0f);
                    _yaw = newYaw;                 // keep world yaw in sync
                }
                // Keep orbitAngle neutral in FP so 3P alignment stays clean
                orbitAngle = 0f;
            }
            else
            {
                // ---------- 3P/far: orbit camera around player (no player rotation) ----------
                orbitAngle = NormalizeAngle(orbitAngle + dx * dynamicOrbit * Time.deltaTime);
                _yaw = NormalizeAngle(target.eulerAngles.y + orbitAngle); // outward world yaw while far
            }

            recentDragTimer = snapAfterDragDelay; // mark recent drag so we don't snap immediately
            ApplyOffset(false);
        }

        // Smooth snap-behind ONLY in 3rd-person when pressing forward,
        // not dragging, and only if there wasn't a recent drag.
        if (snapBehindOnMove && !usingProxy && !dragBtn && recentDragTimer <= 0f && IsPressingForward())
        {
            float before = orbitAngle;
            orbitAngle = Mathf.SmoothDampAngle(
                orbitAngle, 0f, ref snapVel, Mathf.Max(0.01f, snapTime));
            if (Mathf.Abs(Mathf.DeltaAngle(before, orbitAngle)) > 0.001f)
            {
                _yaw = NormalizeAngle(target.eulerAngles.y + orbitAngle);
                ApplyOffset(false);
            }
        }
    }

    void LateUpdate()
    {
        // Keep movement-facing stable per state
        UpdateYawForMovement();
    }

    public void ApplyOffset(bool instant = false)
    {
        if (!target) return;

        // Clamp zoom values
        height = Mathf.Clamp(height, heightLimits.x, heightLimits.y);
        radius = Mathf.Clamp(radius, radiusLimits.x, radiusLimits.y);

        // Always stay behind; never cross
        float effectiveRadius = Mathf.Max(minBackRadius, radius);

        // Forward nudge fades in when close
        float nudgeT = Mathf.InverseLerp(nudgeFadeStart, minBackRadius, effectiveRadius); // 0 far -> 1 very close
        float nudge  = Mathf.Lerp(0f, forwardNudgeNear, Mathf.Clamp01(nudgeT));
        nudge = Mathf.Min(nudge, Mathf.Max(0f, effectiveRadius - 0.01f));

        float backDist = Mathf.Max(minBackRadius, effectiveRadius - nudge);
        backDist = Mathf.Max(backDist, safeBackMargin);

        // Near-state hysteresis
        bool wantProxy = backDist <= nearAimEnter;
        bool wantExit  = backDist >= nearAimExit;

        if (wantProxy && !usingProxy)
        {
            usingProxy = true;
            if (vcam) vcam.LookAt = lookAtProxy;
        }
        else if (wantExit && usingProxy)
        {
            // --- JUST EXITED NEAR/FP → FAR ---
            usingProxy = false;
            if (vcam && target) vcam.LookAt = target;

            // ANTI-KICK: if we were recently dragging AND are pressing forward,
            // align instantly behind the player to avoid any heading bump.
            if (recentDragTimer > 0f && IsPressingForward())
            {
                orbitAngle = 0f;
                _yaw = NormalizeAngle(target.eulerAngles.y);
            }
        }

        // Reflect near-view state for movement script consumption.
        IsNearView = usingProxy;

        // --- LOCAL follow offset (relative to target) ---
        Quaternion localRot = Quaternion.Euler(0f, orbitAngle, 0f);

        Vector3 localOffset;
        if (usingProxy)
        {
            // Near-FP: allow rotation while near by rotating the offset around Y
            localOffset = localRot * new Vector3(0f, nearEyeHeight, -backDist);
        }
        else
        {
            // Normal orbit around target (LOCAL space). 0° is behind.
            localOffset = localRot * new Vector3(0f, height, -backDist);
        }

        // Apply position (hard jump if far off, else smooth)
        if ((localOffset - transposer.m_FollowOffset).sqrMagnitude > 1f || instant)
            transposer.m_FollowOffset = localOffset;
        else
            transposer.m_FollowOffset = Vector3.Lerp(transposer.m_FollowOffset, localOffset, Time.deltaTime * 12f);

        // Near clip tweak
        if (mainCam) mainCam.nearClipPlane = (backDist <= nudgeFadeStart) ? nearClipClose : originalNearClip;

        // --------- Proxy placement (LOCAL to target) ----------
        if (usingProxy)
        {
            // Force slight down-tilt while near; rotate the proxy by the same local orbit
            float ahead = Mathf.Max(0.01f, proxyLookAhead);
            float drop  = Mathf.Tan(nearPitchDeg * Mathf.Deg2Rad) * ahead;

            Vector3 proxyLocal = new Vector3(0f, nearEyeHeight - drop, ahead);
            lookAtProxy.localPosition = localRot * proxyLocal;

            // Tighten composer a touch while near
            composer.m_SoftZoneWidth  = 0.7f;
            composer.m_SoftZoneHeight = 0.7f;
            composer.m_DeadZoneWidth  = 0.0f;
            composer.m_DeadZoneHeight = 0.0f;
        }
        else
        {
            composer.m_SoftZoneWidth  = 0.8f;
            composer.m_SoftZoneHeight = 0.8f;
            composer.m_DeadZoneWidth  = 0.0f;
            composer.m_DeadZoneHeight = 0.0f;
        }
    }

    // ————— helpers —————

    private void UpdateYawForMovement()
    {
        // In near-FP: keep movement yaw locked to the player (we rotate the player in FP),
        // so dragging the view doesn’t send movement into a spiral.
        if (usingProxy)
            _yaw = NormalizeAngle(target.eulerAngles.y);
        else
            _yaw = NormalizeAngle(vcam.transform.eulerAngles.y);
    }

    private static float NormalizeAngle(float a)
    {
        a %= 360f;
        if (a < 0f) a += 360f;
        return a;
    }

    private static bool IsPressingForward()
    {
        // Works with default Input Manager and WASD
        if (Input.GetKey(KeyCode.W)) return true;
        float v = Input.GetAxisRaw("Vertical");
        return v > 0.25f;
        // If you use a custom input system, replace this with your "move forward" flag.
    }
}
