using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CameraRelativeMove : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public TopDownCameraController cameraController;
    private CharacterController controller;

    [Header("Movement")]
    public float moveSpeed = 7f;
    public float sprintSpeed = 11f;
    public float turnSpeed = 130f;              // set 180 in Inspector if you prefer
    public float inputDeadzone = 0.08f;

    [Header("Gravity / Jump")]
    public float gravity = -35f;
    public float jumpHeight = 1f;

    [Header("Camera Follow")]
    public float camSnapSpeed = 8f;
    public float camFollowSpeed = 3.5f;
    public float camDeadzone = 3f;

    [Header("Grounding (quality of life)")]
    public bool snapToGroundOnStart = true;
    public LayerMask groundMask = ~0;
    public float snapCastAbove = 3f;     // how far above we start the ground check
    public float snapCastDepth = 6f;     // how far below we search for ground
    [Tooltip("Extra clearance to keep the capsule slightly above the ground on spawn-only snap.")]
    public float extraGroundClearance = 0.12f;

    // Snap-back (only when you resume moving after orbit release)
    [Header("Orbit Release Snap")]
    [Tooltip("Time (seconds) for camera to glide back behind the player after releasing RMB, but only once you move.")]
    public float orbitReleaseSnapTime = 0.6f;
    [Tooltip("Stop snapping when within this many degrees of the player's yaw.")]
    public float orbitReleaseDeadzone = 0.5f;

    private Vector3 velocity;
    private bool isGrounded;
    private bool wasOrbiting = false;

    // NEW: snap state
    private bool pendingSnap = false;    // armed on RMB release; consumed when you start moving
    private bool snappingBack = false;   // true while SmoothDampAngle is running
    private float snapVelDeg = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        if (!cameraTransform && Camera.main)
            cameraTransform = Camera.main.transform;

        EnsureControllerConfigured();

        if (snapToGroundOnStart)
        {
            // snap immediately and again next frame (after physics settles)
            SnapToGroundNow();
            StartCoroutine(SnapNextFrame());
        }
    }

    System.Collections.IEnumerator SnapNextFrame()
    {
        yield return null;
        SnapToGroundNow();
    }

    void Update()
    {
        HandleMovement();
        // IMPORTANT: no per-frame “maintain clearance” or “drift correction”
        // to avoid fighting CharacterController grounding.
    }

    void HandleMovement()
    {
        // --- Ground & gravity ---
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // --- Input ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(h, v);

        if (input.sqrMagnitude < inputDeadzone * inputDeadzone) input = Vector2.zero;
        else if (input.sqrMagnitude > 1f) input.Normalize();

        bool rmbOrbiting = Input.GetMouseButton(1);
        bool moving = input.sqrMagnitude > 0.001f;

        // NEW: arm pending snap on RMB release (don’t snap yet)
        if (!rmbOrbiting && wasOrbiting)
        {
            pendingSnap = true;      // you can keep free-looking while idle; snap will wait
            snappingBack = false;    // reset any in-flight snap
            snapVelDeg = 0f;
        }
        // Cancel any running snap while dragging
        if (rmbOrbiting)
        {
            snappingBack = false;
            snapVelDeg = 0f;
        }

        // --- Player rotation while NOT orbiting (A/D) ---
        if (!rmbOrbiting && Mathf.Abs(input.x) > 0f)
        {
            float yawDelta = input.x * turnSpeed * Time.deltaTime;
            float newYaw = transform.eulerAngles.y + yawDelta;
            transform.rotation = Quaternion.Euler(0f, newYaw, 0f);

            if (cameraController)
            {
                cameraController.yaw = newYaw;
                cameraController.ApplyOffset();
            }
        }

        // --- Camera-relative movement ---
        Vector3 moveDir = Vector3.zero;
        if (cameraTransform)
        {
            Vector3 forward = cameraTransform.forward; forward.y = 0f; forward.Normalize();
            Vector3 right   = cameraTransform.right;   right.y   = 0f; right.Normalize();
            moveDir = (forward * input.y + right * input.x).normalized;
        }

        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        Vector3 horizontal = moving ? moveDir * speed : Vector3.zero;

        // --- Keep player facing camera when moving (unless orbiting) ---
        if (moving && !rmbOrbiting)
        {
            float targetYaw = cameraController ? cameraController.yaw : cameraTransform.eulerAngles.y;
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(0f, targetYaw, 0f),
                Time.deltaTime * 6f
            );

            if (cameraController)
            {
                float diff = Mathf.DeltaAngle(cameraController.yaw, targetYaw);
                if (Mathf.Abs(diff) > camDeadzone)
                {
                    float followSpeed = Mathf.Abs(diff) > 20f ? camSnapSpeed : camFollowSpeed;
                    cameraController.yaw = Mathf.LerpAngle(cameraController.yaw, targetYaw, Time.deltaTime * followSpeed);
                    cameraController.ApplyOffset();
                }
            }
        }

        // NEW: start snap ONLY when you actually move after RMB release
        if (pendingSnap && moving && !rmbOrbiting)
        {
            snappingBack = true;
            pendingSnap = false;       // consume the pending snap
            snapVelDeg = 0f;
        }

        // NEW: run smooth snap (camera → behind player) while moving
        if (snappingBack && cameraController && moving && !rmbOrbiting)
        {
            float playerYaw = transform.eulerAngles.y;
            float current    = cameraController.yaw;
            float newYaw     = Mathf.SmoothDampAngle(current, playerYaw, ref snapVelDeg,
                                                     Mathf.Max(0.01f, orbitReleaseSnapTime));
            cameraController.yaw = newYaw;
            cameraController.ApplyOffset();

            if (Mathf.Abs(Mathf.DeltaAngle(newYaw, playerYaw)) <= orbitReleaseDeadzone)
            {
                cameraController.yaw = playerYaw;
                cameraController.ApplyOffset();
                snappingBack = false;
            }
        }

        wasOrbiting = rmbOrbiting;

        // --- Apply movement ---
        velocity.y += gravity * Time.deltaTime;
        Vector3 final = horizontal; final.y = velocity.y;
        controller.Move(final * Time.deltaTime);

        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    // ---------- Grounding helpers ----------

    // Keep CC values reasonable; enforce skinWidth=0.8f. Do not change center at runtime.
    void EnsureControllerConfigured()
    {
        if (!controller) return;
        controller.skinWidth       = 0.8f; // enforce the stable value you want
        controller.stepOffset      = Mathf.Clamp(controller.stepOffset, 0.15f, 0.6f);
        controller.minMoveDistance = Mathf.Clamp(controller.minMoveDistance, 0.0005f, 0.01f);
        // DO NOT change controller.center here; use the Inspector’s value.
    }

    // One-time placement so the controller’s bottom rests slightly above the first collider below.
    public void SnapToGroundNow()
    {
        if (!controller) return;

        float half = controller.height * 0.5f;
        float up   = Mathf.Max(snapCastAbove, half);

        Vector3 start = transform.position + Vector3.up * up;
        float castDist = up + snapCastDepth;

        // SphereCast with CC radius gives stable results on uneven ground
        if (Physics.SphereCast(start, controller.radius, Vector3.down, out var hit, castDist, groundMask, QueryTriggerInteraction.Ignore))
        {
            float desiredY = hit.point.y + half + extraGroundClearance;  // small lift
            float deltaY = desiredY - transform.position.y;

            // Move via CC to avoid overlap/grounding state toggles
            if (Mathf.Abs(deltaY) > 0.001f)
                controller.Move(new Vector3(0f, deltaY, 0f));
        }
    }
}
