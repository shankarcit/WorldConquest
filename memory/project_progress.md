# World Conquest — Development Progress

## Session: 2026-03-19

### Implemented
- [x] Map rendering — GeoJSON → Unity country meshes
- [x] Country hover highlight (yellow)
- [x] Country click event (MapEventBus)
- [x] Military data loading (50 countries, JSON)
- [x] Camera controller (pan + orthographic zoom)
- [x] URP material for country meshes
- [x] Git repo initialized, pushed to github.com/shankarcit/WorldConquest
- [x] **GameManager.cs** — Singleton, random country assignment, reroll, StartGame()
- [x] **CountryJustificationMenu.cs** — Left-side panel, alliance/war with name confirmation
- [x] **PlayerHUD.cs** — Top/bottom bar, notification popup
- [x] **MapEventBus** — OnCountryClicked, OnWarDeclared, OnNotification, OnCountryConquered
- [x] **GeoJsonLoader** — Calls GameManager.OnCountriesLoaded()

### Auto-implementation (session 2)
- [x] **CountryData** — Added TerrainType enum, TerritoryPercent, HasCoastline fields
- [x] **CombatSystem.cs** — Full GDD §8.3 formula: AttackPower, terrain modifier, capture ratio, outcome
- [x] **CombatSystem.ApplyResult()** — Mutates attacker/defender assets, handles conquest + asset absorption
- [x] **CombatSystem.RegenerateTick()** — Slow regen for troops/air/navy per GDD §8.4
- [x] **DeploymentScreen.cs** — Slider-based deployment UI, calls CombatSystem, shows battle result panel
- [x] **CountryJustificationMenu** — Closes on war declaration so DeploymentScreen opens cleanly

### Next up (from GDD)
- [ ] Splash screen — country assignment display with reroll button (GDD §5.1)
- [ ] Win condition checker — World Domination / Superpower / Diplomatic Win (GDD §10)
- [ ] Country conquest visual — conquered countries change color on map (GDD §8.5)
- [ ] Turn system — RegenerateTick called each turn (GDD §8.4)
- [ ] Minimap (GDD §5.2)
