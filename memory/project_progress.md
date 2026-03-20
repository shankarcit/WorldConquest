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
- [x] TurnManager — End Turn button, RegenerateTick + EconomyTick per country, SetTurn()
- [x] WinConditionChecker — World Domination / Superpower / Diplomatic Win, win panel, game pause + restart
- [x] AITurnManager — each turn AI countries independently attack (30% chance) or form alliances (20% chance)
- [x] SaveSystem — JSON serialization to persistentDataPath/savegame.json; F5/F9 shortcuts; patches live CountryData in-place (GDD §13)
- [x] GameSaveData / CountrySaveData — includes GDP + Resources fields
- [x] EconomySystem — GDP derived from military rank (rank 1 → 10,000, rank 50 → 200, unranked → 50); Income = GDP × TerritoryPercent per turn; auto-converts Resources to Troops (÷5) and AirForce (÷200); treasury capped at GDP×3; ProjectedIncome() helper (GDD §12)

### UI
- [x] SplashScreen — assigned country stats, 1x reroll, Start button
- [x] CountryJustificationMenu — left-side panel, alliance/war with name confirmation
- [x] DeploymentScreen — slider-based force selection, battle result panel
- [x] PlayerHUD — top/bottom bar, notification popup; now shows GDP + Treasury + projected income/turn
- [x] MapEventBus — OnCountryClicked, OnWarDeclared, OnNotification, OnCountryConquered, OnTurnEnded
- [x] MinimapController — secondary orthographic camera → RenderTexture → RawImage (bottom-right); viewport indicator; cyan player marker (GDD §5.2)
- [x] SaveLoadUI — Save/Load buttons + F5/F9 keyboard shortcuts

### Visuals
- [x] CountryMesh conquest visual — conquered countries turn dark grey, collider disabled (GDD §8.5)

### Assets
- [x] MinimapRT.renderTexture — 512×256 RenderTexture in Assets/RenderTextures/

## Next up (from GDD §12 future features)
- [ ] Nuclear weapons — global consequence events
- [ ] Espionage / spy missions
- [ ] Historical scenario modes (Cold War, WW2)
- [ ] Scene wiring: PlayerHUD needs GDP Text + Resources Text fields wired in Inspector
