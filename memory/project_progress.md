# World Conquest — Development Progress

## Implemented (all sessions)

### Map & Core
- [x] Map rendering — GeoJSON → Unity country meshes
- [x] Country hover highlight (yellow), click event
- [x] Military data loading (50 countries, JSON)
- [x] Camera controller (pan + orthographic zoom)
- [x] URP material, UInt32 mesh index format

### Game Logic
- [x] GameManager — random country assignment, reroll, StartGame()
- [x] CombatSystem — AttackPower formula, terrain modifier, capture ratio, attrition, conquest + asset absorption, RegenerateTick
- [x] TurnManager — End Turn button, calls RegenerateTick on all countries, advances turn counter
- [x] WinConditionChecker — World Domination / Superpower / Diplomatic Win, win panel, game pause + restart
- [x] AITurnManager — each turn AI countries independently attack (30% chance) or form alliances (20% chance); uses 50% of assets; respects minimum power ratio; fires notifications (GDD §2)

### UI
- [x] SplashScreen — assigned country stats, 1x reroll, Start button
- [x] CountryJustificationMenu — left-side panel, alliance/war with name confirmation
- [x] DeploymentScreen — slider-based force selection, battle result panel
- [x] PlayerHUD — top/bottom bar, notification popup
- [x] MapEventBus — OnCountryClicked, OnWarDeclared, OnNotification, OnCountryConquered, OnTurnEnded

### Visuals
- [x] CountryMesh conquest visual — conquered countries turn dark grey, collider disabled (GDD §8.5)

## Next up (from GDD)
- [ ] Minimap — bottom-right territory overview (GDD §5.2)
- [ ] Economy system — GDP / resource income per turn (GDD §12 future)
- [ ] Save/load system — JSON game state serialization (GDD §13)
- [ ] Scene wiring — add AITurnManager component to GameManager GameObject in SampleScene.unity
