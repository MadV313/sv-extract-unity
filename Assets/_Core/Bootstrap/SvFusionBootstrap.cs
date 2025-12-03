using Fusion;
using UnityEngine;

public class SvFusionBootstrap : MonoBehaviour {
  [Header("Prefabs")]
  [SerializeField] private NetworkRunner runnerPrefab;
  [SerializeField] private NetworkObject playerPrefab;

  private async void Start() {
    var runner = Instantiate(runnerPrefab);
    DontDestroyOnLoad(runner.gameObject);

    var args = new StartGameArgs {
      GameMode    = GameMode.Shared,   // swap to ClientServer later
      SessionName = "dev",
      PlayerCount = 8
    };

    var result = await runner.StartGame(args);
    if (!result.Ok) {
      Debug.LogError($"Fusion start failed: {result.ShutdownReason}");
      return;
    }

    runner.ProvideInput = true;

    if (playerPrefab) {
      runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, runner.LocalPlayer);
    } else {
      Debug.LogWarning("Player prefab not assigned on SvFusionBootstrap.");
    }
  }
}
