# Complete Project Explanation - GodTale

> **Note**: This is a C# conversion of the original GDScript project. The original GDScript version can be found at: [https://github.com/JuhaszLaszlo69/dusttale_mytake](https://github.com/JuhaszLaszlo69/dusttale_mytake)

## 🎮 Core Architecture

### **Autoload Singletons (Global State)**
The game uses two autoload singletons that persist across all scenes:

1. **`Global`** (`autoloads/Global.cs`)
   - **Boss Tracking**: `killed_bosses` array tracks which bosses have been defeated
   - **Currency**: `player_exp` and `player_gold` for shop purchases
   - **Inventory**: `battle_inventory` array stores all collected items
   - **Position Tracking**: `last_scene_path` and `last_player_position` save where the player was before entering battles/shops
   - **Boss List**: `BOSS_NAMES = ["Cherry", "Poseur", "Present", "Godot"]`
   - **Methods**: `MarkBossKilled()`, `IsBossKilled()`, `AllBossesKilled()`, `AddExp()`, `SpendExp()`, `SpendGold()`, `AddItemToInventory()`

2. **`Fade`** (`autoloads/FadeToBlack.cs`)
   - Handles scene transitions with fade-to-black animations
   - Methods: `FadeIntoBlack()`, `FadeFromBlack()`
   - Used whenever transitioning between scenes (battles, shops, maps)

---

## 🗺️ Scene Flow & Navigation

### **Title Screen** (`intermediate_scenes/TitleScreen.cs` / `intermediate_scenes/title_screen.tscn`)
- **Entry Point**: Game starts here (set in `project.godot`)
- **Features**:
  - "Go to Overworld" button → Takes you to `overworld_original.tscn` (tavern/shop area)
  - Debug buttons for each boss → Directly start boss battles
  - "[DEBUG] View Cutscene" → View end-game cutscene without completing game
- **Purpose**: Menu system for navigation and testing

### **Main Map** (`maps/MainMap.cs` / `maps/main_map.tscn`)
- **Primary Exploration Area**: Where players encounter bosses
- **Features**:
  - Displays player inventory, EXP, and Gold in UI
  - Hides boss sprites if they're already defeated
  - Restores player position when returning from battles
  - Automatically triggers end-game cutscene if all bosses are defeated
- **Boss Zones**: `battle/BattleZone.cs` Area2D nodes detect when player enters and start battles

### **Overworld Original** (`maps/OverworldOriginal.cs` / `maps/overworld_original.tscn`)
- **Tavern/Shop Area**: Where players can access the shop
- **Shop Entrance**: `shop/ShopCollision.cs` or `shop/ShopEntrance.cs` detect player and allow shop access
- **Doors**: `maps/Door.cs` and `maps/Door2.cs` handle scene transitions between areas

---

## ⚔️ Battle System

### **Battle Flow** (`battle/Battle.cs`)

**Initialization:**
1. Battle scene loads with fade-in
2. Enemy data is loaded from `Battle.enemy` (set before scene change)
3. Enemy sprite, HP, acts, and bullet waves are configured
4. Player inventory is loaded from `Global.battle_inventory`
5. Battle music plays (alternates between two tracks)

**Turn Structure:**
1. **Player Action Phase**: Player chooses from 4 options:
   - **ATTACK**: Timing-based attack minigame
   - **ACT**: Perform actions to increase mercy (spare enemies)
   - **ITEM**: Use healing items from inventory
   - **MERCY**: Spare enemy (only available when mercy = 100%)

2. **Bullet Hell Phase**: After player action, enemy attacks
   - Battle box resizes to wave's `box_size`
   - Soul appears (RED/BLUE/YELLOW mode based on wave)
   - Bullet wave spawns and attacks
   - Player dodges bullets by moving soul
   - Wave ends when `Global.wave_done` signal is emitted

3. **Monster Response**: Enemy speaks via speech bubble

4. **Repeat**: Cycle continues until:
   - Enemy HP ≤ 0 (battle won by attack)
   - Enemy mercy = 100% and player spares (battle won by mercy)
   - Player HP ≤ 0 (battle lost)

**Victory Conditions:**
- **By Attack**: Enemy HP reaches 0 → Award EXP (if boss) + Gold
- **By Mercy**: Enemy mercy reaches 100% → Player spares → Award EXP (if boss) + Gold
- Bosses are marked as killed in `Global.killed_bosses`
- Player returns to previous scene at saved position

**Defeat:**
- Player HP reaches 0 → Death particles → Return to previous scene

### **Attack System**
- Player presses ATTACK → Attack bar appears
- Player must time pressing Enter when attack line aligns with bar
- Damage = `(575 - distance_from_centre) / 10`
- Visual feedback: Knife animation, damage number, monster hurt animation

### **Soul Modes** (`soul/Soul.cs`)
The soul (player's representation in bullet hell) has 3 modes:

1. **RED Soul**:
   - Free movement in all directions (arrow keys)
   - Speed: 200 pixels/second
   - Standard dodging mode

2. **BLUE Soul**:
   - Horizontal movement only (left/right arrows)
   - Gravity-based: Falls down, can jump with Space/Up
   - Jump force: -500, gravity: 600
   - Platformer-style movement

3. **YELLOW Soul**:
   - Free movement in all directions
   - Can shoot bullets with Enter/Space
   - Bullets can destroy enemy bullets
   - Shooting has cooldown timer

**Damage System:**
- Soul has a hurtbox that detects overlapping bullets
- Bullets have `damage_amount` property
- Invincibility timer prevents rapid damage
- When hit: HP decreases, soul flashes, sound plays

### **Bullet Waves** (`waves/`)
- Each wave is a scene that extends `Wave` class (`waves/Wave.cs`)
- Waves define:
  - `mode`: Which soul mode to use (RED/BLUE/YELLOW)
  - `box_size`: Size of battle box during wave
  - `box_size_change_time`: Animation duration
- Waves spawn bullets using timers
- When wave completes, emits `Global.wave_done` signal
- Examples: `Shoot1.cs`, `Shoot2.cs`, `Shoot3.cs`, `Jump.cs`, `Jump2.cs`, `SpinningStorm.cs`, `SpinningStorm2.cs`, `FollowSoul.cs`, `DiamondsFromBelow.cs`, etc.

### **Bullet Types** (`bullets/`)
- **Linear Bullet**: Moves in straight line at constant speed
- **Follower Bullet**: Follows/tracks the soul
- **Jump Obstacle**: Stationary obstacle (for BLUE soul platforming)
- All bullets have `damage_amount` and `freed_on_hit` properties

---

## 👾 Enemy System

### **Enemy Base Class** (`enemy_data/Enemy.cs`)
- **Properties**:
  - `enemy_name`: Display name
  - `HP`: Health points
  - `sprite`: Texture for enemy appearance
  - `sprite_scale`: Scale multiplier
  - `acts`: Array of action names (e.g., ["Check", "Cheer", "Football"])
  - `bullet_waves`: Array of PackedScene references to wave scenes
  - `encounter_text`: Text shown when battle starts

- **Methods** (overridden by specific enemies):
  - `DoActGetText(act_name)`: Returns text when player performs action
  - `GetIdleText()`: Returns text shown during idle turns
  - `GetMonsterText()`: Returns text when monster speaks

### **Bosses**
1. **Cherry** (`enemy_data/Cherry.cs`)
   - Acts: "Cheer", "Football", "Check"
   - Mercy increases when player cheers or talks about football
   - Unique dialogue based on actions

2. **Poseur** (`enemy_data/Poseur.cs`)
   - Custom battle patterns

3. **Present** (`enemy_data/Present.cs`)
   - Custom battle patterns

4. **Godot** (`enemy_data/GodotEnemy.cs`)
   - Custom battle patterns

### **Battle Zone** (`battle/BattleZone.cs`)
- Area2D that detects when player enters
- Checks if boss is already killed (hides zone if so)
- Saves current scene path and player position
- Instantiates enemy scene and sets `Battle.enemy`
- Transitions to battle scene with fade

---

## 🛒 Shop System

### **Shop Scene** (`shop/shop.tscn`)
- **Menu Navigation**: Cursor-based system with arrow keys
- **Shop Items** (defined in `shop/Shop.cs`):
  - Apple: 10 EXP, 5 Gold
  - Nice Cream: 15 EXP, 8 Gold
  - Pie: 20 EXP, 10 Gold

### **Shop Flow** (`shop/Shop.cs`)
1. **Greeting**: Shopkeeper greets player
2. **Buy Menu**: Player navigates items with arrow keys
3. **Item Info**: Shows item name and costs
4. **Confirm Prompt**: "Buy it for X EXP, Y Gold?"
5. **Purchase**: Checks if player has enough currency
   - Success: Item added to `Global.battle_inventory`, currency deducted
   - Failure: Error message shown
6. **Exit**: Returns to previous scene at saved position

### **Shop Access**
- `shop/ShopCollision.cs` or `shop/ShopEntrance.cs` detect player in overworld
- Saves scene and position before entering shop
- Transitions with fade

---

## 📦 Inventory & Items

### **Item System** (`items/`)
- Items are `.tres` resource files
- Item class defined in `items/Item.cs`
- Properties: `item_name`, `amount` (healing), `text` (description)
- Items: `apple.tres`, `nice_cream.tres`, `pie.tres`

### **Inventory Management**
- Stored in `Global.battle_inventory` (persists across scenes)
- Items can be used during battles to heal
- Items are consumed when used
- Inventory displayed on main map UI

---

## 🎬 Cutscenes

### **End-Game Cutscene** (`cutscenes/AllBossesDefeated.cs` / `cutscenes/all_bosses_defeated.tscn`)
- **Trigger**: Automatically when all 4 bosses are defeated
- **Content**: 
  - Series of messages displayed with typing effect
  - Particle effects
  - Messages about completing the game
- **Flow**: Fade in → Show messages → Fade out → Return to main map

---

## 🎨 UI Systems

### **Text Box** (`text_box/`)
- `battle/TextBox.cs`: Handles scrolling text display in battles
- `text_box/MonsterTextBox.cs`: Handles monster speech bubbles
- `text_box/CustomButton.cs`: Custom button with shake effects
- `text_box/Util.cs`: Utility functions for text effects (shake, wave)

### **Battle UI**
- HP Bar: Shows player HP (max 20)
- Action Buttons: Attack, Act, Item, Mercy
- Options Container: Dynamic buttons for acts/items
- Attack Bar/Line: Timing minigame UI
- Damage Label: Shows damage numbers
- Speech Bubble: Monster dialogue

---

## 🎵 Audio System

### **Music**
- Battle music alternates between two tracks
- Main map plays "Ruins" theme
- Title screen has menu music

### **Sound Effects**
- Encounter sounds
- Attack sounds (knife slash, monster hurt)
- Soul hit sounds
- Item use sounds
- Menu navigation sounds
- Shooting sounds (yellow soul)

---

## 🔄 State Persistence

### **What Persists:**
- Boss kill status (`Global.killed_bosses`)
- Player EXP and Gold
- Inventory items
- Player position (when entering/exiting battles/shops)
- Last scene path (for returning after battles)

### **What Resets:**
- Player HP (resets to 20 at battle start)
- Battle turn counter
- Enemy mercy (per battle)

---

## 🎯 Game Progression

### **Flow:**
1. Start at title screen
2. Go to overworld or main map
3. Explore and encounter bosses
4. Fight bosses in turn-based battles
5. Earn EXP and Gold from victories
6. Visit shop to buy healing items
7. Use items in battles
8. Defeat all 4 bosses
9. End-game cutscene plays
10. Game complete

### **Boss Progression:**
- Each boss can only be defeated once
- Defeated bosses disappear from map
- EXP only awarded on first defeat
- Gold always awarded
- All bosses must be defeated to see ending

---

## 🛠️ Technical Details

### **Scene Transitions:**
- Always use `Fade.FadeIntoBlack()` before changing scenes
- Always use `Fade.FadeFromBlack()` when scene loads
- Player position is saved before transitions
- Position is restored when returning

### **Signal System:**
- `Global.wave_done`: Emitted when bullet wave completes
- `Global.add_bullet`: Emitted to add bullets to battle
- `Global.change_mercy`: Emitted to change enemy mercy
- `Global.heal_player`: Emitted to heal player
- `Global.bullet_destroyed`: Emitted when bullet is destroyed
- `Global.monster_visible`: Emitted to show/hide monster
- `Global.play_shoot_sound`: Emitted when yellow soul shoots

### **Groups:**
- `"soul"`: The player's soul in battle
- `"wave"`: Bullet wave instances
- `"enemy"`: Enemy instances

---

## 🎮 Controls Summary

### **Exploration:**
- Arrow Keys: Move player
- Enter/Space: Interact (doors, shop entrances)

### **Battle:**
- Arrow Keys: Move soul (varies by mode)
- Space/Up: Jump (BLUE) or Shoot (YELLOW)
- Enter: Confirm action/attack timing
- C/X/Backspace: Cancel/go back

### **Shop:**
- Arrow Keys (Up/Down): Navigate menu
- Enter: Select/Confirm purchase
- C/X/Backspace: Cancel/go back

---

## 📝 Key Files Reference

- **Global State**: `autoloads/Global.cs`
- **Battle Logic**: `battle/Battle.cs`
- **Player Movement**: `player/Scripts/Jugador.cs`
- **Soul Mechanics**: `soul/Soul.cs`
- **Enemy Base**: `enemy_data/Enemy.cs`
- **Shop Logic**: `shop/Shop.cs`
- **Battle Zones**: `battle/BattleZone.cs`
- **Fade System**: `autoloads/FadeToBlack.cs`
- **Main Map**: `maps/MainMap.cs`
- **Title Screen**: `intermediate_scenes/TitleScreen.cs`
- **C# Project**: `UndertaleBattle.csproj`
- **C# Solution**: `UndertaleBattle.sln`

---

## 🐛 Debug Features

- Title screen has debug buttons to directly access any boss battle
- "[DEBUG] View Cutscene" button to skip to ending
- Boss battles can be accessed without exploring

---

This should give you everything you need to explain the project while your teammate plays through the game!
