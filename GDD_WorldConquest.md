# 🌍 World Conquest — Game Design Document (GDD)

**Engine:** Unity  
**Genre:** Grand Strategy / Turn-Based / Real-Time  
**Platform:** PC  
**Version:** 0.1 (Draft)  
**Last Updated:** 2026-03-15

---

## 1. Game Overview

**World Conquest** is a 3D grand strategy game where the player is assigned a random real-world country and must expand their influence through diplomacy, alliances, and military conquest. Every country starts with military assets proportional to their real-world military ranking. The player must manage troops, missiles, air force, and naval power to wage wars and negotiate alliances on a fully bordered 3D world map.

---

## 2. Core Concept

- The world map renders all real countries with visible borders
- Each country's military strength (troops, missiles, air force, navy) is derived from real-world military rankings
- The player is assigned a **random country** at game start
- Other countries are controlled by **AI** or optionally by **other players** in multiplayer
- Players interact with countries via the **Country Justification Menu**

---

## 3. World Map

### 3.1 Visual Design
- Full 3D globe or flat projection (toggle option)
- Country borders clearly rendered with distinct color coding per country
- Terrain features visible on the map:
  - Mountains (brown/grey elevation)
  - Oceans and seas (blue)
  - Plains (green)
  - Deserts (yellow)
  - Arctic/tundra regions (white)
- Countries highlight on hover
- Clicking a country opens the **Country Justification Menu**

### 3.2 Geography & Terrain Effects
Terrain directly impacts military conquest difficulty:

| Terrain Type     | Conquest Difficulty Modifier |
|------------------|-------------------------------|
| Plains           | ×1.0 (standard)               |
| Desert           | ×1.3 (heat, supply issues)    |
| Forest           | ×1.4 (guerrilla risk)         |
| Mountains        | ×1.8 (extremely hard)         |
| Arctic / Tundra  | ×2.0 (weather attrition)      |
| Island Nation    | ×1.5 (requires naval support) |

Weather events (storms, blizzards, monsoons) can apply temporary additional difficulty multipliers during campaigns.

---

## 4. Military System

### 4.1 Military Ranking & Starting Assets
Countries receive starting military assets based on their real-world global military ranking. Higher-ranked countries begin with significantly more assets.

**Formula:**  
`Base Assets = (Total Countries - Military Rank + 1) × Asset Multiplier`

### 4.2 Asset Categories

| Asset Type   | Description                                              |
|--------------|----------------------------------------------------------|
| **Troops**   | Ground infantry and armored units for land conquest      |
| **Missiles** | Long-range strike capability, reduces enemy defenses     |
| **Air Force**| Fighter jets and bombers, air superiority & ground support|
| **Navy**     | Warships and submarines, required for island/coastal wars|

### 4.3 Example Starting Assets (Top 5 Countries)

| Rank | Country       | Troops   | Missiles | Air Force | Navy |
|------|---------------|----------|----------|-----------|------|
| 1    | USA           | 500,000  | 1,000    | 2,000     | 500  |
| 2    | Russia        | 480,000  | 1,200    | 1,800     | 400  |
| 3    | China         | 460,000  | 1,100    | 1,700     | 450  |
| 4    | India         | 440,000  | 800      | 1,500     | 300  |
| 5    | UK            | 420,000  | 700      | 1,400     | 350  |
| ...  | ...           | ...      | ...      | ...       | ...  |

> Assets scale down proportionally for lower-ranked countries.

---

## 5. Player Setup

### 5.1 Country Assignment
- At game start, the player is assigned a **random real-world country**
- The country's name, flag, military rank, and starting assets are displayed on a splash screen
- Player can optionally reroll once per new game

### 5.2 Player HUD
- **Top bar:** Country name, flag, current territory count, resource indicators
- **Bottom bar:** Asset overview (Troops / Missiles / Air Force / Navy)
- **Left panel:** Country Justification Menu (opens on country click)
- **Minimap:** Bottom-right corner showing global territory control

---

## 6. Country Justification Menu

The **Country Justification Menu** is the primary interaction panel, anchored to the **left side of the screen**. It opens when the player clicks on any country on the map.

### 6.1 Menu Layout

```
┌──────────────────────────────┐
│  🌐 [Selected Country Name]  │
│  Military Rank: #XX          │
│  Status: Neutral / Allied /  │
│           At War             │
├──────────────────────────────┤
│  [ Alliance Request ]        │
│  [ War Declaration ]         │
│  [ View Military Stats ]     │
│  [ Close ]                   │
└──────────────────────────────┘
```

### 6.2 How to Use
1. Click any country on the world map
2. The menu slides in from the left showing that country's info
3. The player types the country name to confirm the target (prevents accidental actions)
4. Choose an action: **Alliance Request** or **War Declaration**

---

## 7. Diplomacy System

### 7.1 Alliance Request

- Player clicks **[ Alliance Request ]** inside the Country Justification Menu
- A confirmation dialog appears with the target country name
- **If target is AI-controlled:**
  - 50% chance the alliance is **Accepted**
  - 50% chance the alliance is **Denied**
  - Result is shown via a notification popup
- **If target is player-controlled (multiplayer):**
  - The target player receives an **Alliance Request Notification**
  - They can choose **Accept** or **Deny**
  - No time limit, but the request expires after 60 seconds if unanswered

### 7.2 Alliance Benefits
- Allied countries will not attack each other
- Allies can provide military support during wars (optional)
- Alliances can be broken — breaking an alliance causes a global reputation penalty

### 7.3 Alliance States

| State       | Description                                      |
|-------------|--------------------------------------------------|
| Neutral     | Default — no relationship                        |
| Allied      | Mutual non-aggression + optional military support|
| At War      | Active military conflict                         |
| Broken      | Former allies — reputation penalty applied       |

---

## 8. War System

### 8.1 Declaring War

1. Player clicks **[ War Declaration ]** inside the Country Justification Menu
2. Player confirms the target country name
3. A **Military Deployment Screen** opens

### 8.2 Military Deployment Screen
The player selects which assets to commit to the war:

```
┌─────────────────────────────────────┐
│  ⚔️  Deploy Forces Against [Country] │
├─────────────────────────────────────┤
│  Troops:      [____] / 440,000      │
│  Missiles:    [____] / 800          │
│  Air Force:   [____] / 1,500        │
│  Navy:        [____] / 300          │
│                                     │
│  [ Confirm Deployment ]             │
└─────────────────────────────────────┘
```

### 8.3 Combat Resolution Formula

Land gained is calculated based on:

```
Attack Power = (Troops × 1) + (Missiles × 50) + (Air Force × 30) + (Navy × 20)

Effective Attack = Attack Power / Terrain Difficulty Modifier

Defense Power = Enemy Attack Power (same formula)

Land Captured (%) = (Effective Attack / (Effective Attack + Defense Power)) × 100
```

- A result above **60%** = attacker captures significant territory
- A result between **40–60%** = stalemate, small gains
- A result below **40%** = failed invasion, attacker loses some units

### 8.4 Unit Attrition
- Deployed units are consumed or reduced after each battle
- Missiles are a **one-time use** resource
- Troops and Air Force regenerate slowly over time (based on country size)
- Navy only regenerates if the country has a coastline

### 8.5 Winning a War
- A country is fully conquered when **100% of its territory is captured**
- Conquered country's assets are absorbed by the victor (partial absorption)
- Conquered AI countries are removed from the map; their territory changes color

---

## 9. Multiplayer

- Supports multiple human players, each assigned a different random country
- Players see each other's countries highlighted with a special border color
- Alliance requests and war declarations between players are fully interactive (no RNG — player chooses)
- Chat system available between allied players

---

## 10. Win Conditions

| Condition            | Description                                     |
|----------------------|-------------------------------------------------|
| **World Domination** | Control 100% of all countries                   |
| **Superpower**       | Control 50%+ of the world and have 5+ allies    |
| **Diplomatic Win**   | Form an alliance bloc of 10+ countries          |

---

## 11. UI / UX Summary

| Element                    | Position         | Function                              |
|----------------------------|------------------|---------------------------------------|
| Country Justification Menu | Left side        | Diplomacy and war actions             |
| Asset HUD                  | Bottom bar       | Track military resources              |
| Country Info Panel         | Top bar          | Player country + territory count      |
| Notifications              | Top-right        | Alliance results, war outcomes        |
| Minimap                    | Bottom-right     | Global territory overview             |

---

## 12. Future Features (Planned)

- [ ] Economy & resource system (GDP, oil, food)
- [ ] Nuclear weapons with global consequence events
- [ ] UN Security Council voting events
- [ ] Espionage and spy missions
- [ ] Historical scenario modes (Cold War, WW2, etc.)
- [ ] Mobile port

---

## 13. Technical Notes (Unity)

- **Map Rendering:** Use Unity's terrain system or a custom mesh for the globe
- **Country Data:** Store country borders as polygon data (GeoJSON → Unity mesh)
- **Military Data:** JSON file mapping country name → military rank → asset values
- **UI Framework:** Unity UI Toolkit or uGUI for HUD and menus
- **Multiplayer:** Unity Netcode for GameObjects (NGO) or Photon Fusion
- **Save System:** JSON serialization for game state (territory, alliances, assets)

---

*GDD v0.1 — World Conquest | Built with Unity | 2026*
