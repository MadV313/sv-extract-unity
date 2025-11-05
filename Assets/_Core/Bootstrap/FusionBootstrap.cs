using System.Threading.Tasks;
using Fusion;
using UnityEngine;

public class FusionBootstrap : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private NetworkRunner runnerPrefab;
    [SerializeField] private NetworkObject playerPrefab;

    private async void Start() {
        // Spawn a NetworkRunner and start a local shared session for dev
        var runner = Instantiate(runnerPrefab);
        DontDestroyOnLoad(runner.gameObject);

        var args = new StartGameArgs {
            GameMode    = GameMode.Shared,  // switch to ClientServer later
            SessionName = "dev",
            PlayerCount = 8
        };

        var result = await runner.StartGame(args);
        if (result.Ok == false) {
            Debug.LogError($"Fusion start failed: {result.ShutdownReason}");
            return;
        }

        runner.ProvideInput = true;

        // Spawn local player
        if (playerPrefab) {
            runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, runner.LocalPlayer);
        } else {
            Debug.LogWarning("Player prefab not assigned on FusionBootstrap.");
        }
    }
}
