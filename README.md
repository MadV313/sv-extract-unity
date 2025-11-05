# SV Extract — Unity Client (sv-extract-unity)

## Tech
- Unity 2022.3 LTS (URP)
- Photon Fusion 2 (Shared → ClientServer later)
- Addressables (Local & Remote profiles)
- New Input System
- Cinemachine, TextMeshPro
- Helpers: Post-Processing v2 / URP Volumes, Scene Streamer, Object Pool Manager, OmniShade

## First Run
1. Clone repo, open folder in Unity 2022.3 LTS.
2. Unity will import packages from `Packages/manifest.json`.
3. Import **Photon Fusion 2** via Asset Store / Photon Hub.
4. In `Assets/_Core/Bootstrap`, create **AppConfig.asset** and set:
   - ApiBase, CdnBase, FusionAppId.
5. Open `Assets/_Dev/DevTest.unity`.
6. Add a `NetworkRunner` prefab to **FusionBootstrap**, assign `Player.prefab`.
7. Window → Addressables → Create settings. Add **Local** & **Remote** profiles.
8. Play — you should spawn a local **Player** in a shared session.

## Build
- Editor: Shared mode for quick iteration.
- Later: switch Fusion `GameMode` to **ClientServer** for proper networking.
- Set `USE_REMOTE_ADDR` (Scripting Define Symbols) when using remote Addressables CDN.

## Git LFS
Textures, audio, models, scenes, prefabs, and .asset files are tracked via LFS (see `.gitattributes`).
