# World Conquest — Development Progress

## Implemented (all sessions)

### Map & Core
- [x] Map rendering — GeoJSON → Unity country meshes
- [x] Country hover highlight (yellow), click event
- [x] Military data loading (50 countries, JSON)
- [x] Camera controller (pan + orthographic zoom)
- [x] URP material, UInt32 mesh index format

### Game Logic
- [x] GameManager — random country assignment, reroll, StartGame(), SetPlayerCountry()
- [x] CombatSystem — AttackPower formula, terrain modifier, capture ratio, attrition, conquest + asset absorption, RegenerateTick
- [x] TurnManager — End Turn button, calls RegenerateTick on all countries, advances turn counter, SetTurn()
- [x] WinConditionChecker — World Domination / Superpower / Diplomatic Win, win panel, game pause + restart
- [x] AITurnManager — each turn AI countries independently attack (30% chance) or form alliances (20% chance)
- [x] SaveSystem — JSON serialization to persistentDataPath/savegame.json; Save() / Load() / SaveExists(); F5/F9 shortcuts; patches live CountryData in-place without reloading meshes; re-fires OnCountryConquered for visuals (GDD §13)
- [x] GameSaveData / CountrySaveData — serializable POCOs, excludes Polygons, matched by Name on load

### UI
- [x] SplashScreen — assigned country stats, 1x reroll, Start button
- [x] CountryJustificationMenu — left-side panel, alliance/war with name confirmation
- [x] DeploymentScreen — slider-based force selection, battle result panel
- [x] PlayerHUD — top/bottom bar, notification popup
- [x] MapEventBus — OnCountryClicked, OnWarDeclared, OnNotification, OnCountryConquered, OnTurnEnded
- [x] MinimapController — secondary orthographic camera → RenderTexture → RawImage (bottom-right); viewport indicator; cyan player marker (GDD §5.2)
- [x] SaveLoadUI — Save/Load buttons + F5/F9 keyboard shortcuts; optional save path label

### Visuals
- [x] CountryMesh conquest visual — conquered countries turn dark grey, collider disabled (GDD §8.5)

### Assets
- [x] MinimapRT.renderTexture — 512×256 RenderTexture in Assets/RenderTextures/

## Next up (from GDD)
- [ ] Economy system — GDP / resource income per turn (GDD §12 future)
- [ ] Scene wiring: SaveLoadUI component needs Save/Load buttons wired in Inspector
- [ ] Scene wiring: MinimapController needs wiring in Inspector
