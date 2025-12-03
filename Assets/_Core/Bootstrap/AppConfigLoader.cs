using UnityEngine;
using SVExtract.Net;

[CreateAssetMenu(menuName = "SV13/AppConfig", fileName = "AppConfig")]
public class AppConfig : ScriptableObject
{
    public string ApiBase = "https://sv-extract-api-production.up.railway.app";
    public string CdnBase = "https://sv-extract-cdn-production.up.railway.app";
    public string FusionAppId = "<your_photon_app_id>";
}

public class AppConfigLoader : MonoBehaviour
{
    [SerializeField] private AppConfig config;
    private void Awake() {
        if (config != null) {
            ApiClient.Configure(config.ApiBase);
            // TODO: set Fusion app id into Photon config if needed (via Fusion setup)
        }
    }
}
