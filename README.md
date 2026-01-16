# Undertale Clone i.e GodTale

A fan-made Undertale-inspired game built with Godot Engine 4.4 using **C#**. This project recreates the core gameplay mechanics of Undertale, including turn-based combat with bullet-hell patterns, exploration, boss battles, and a shop system.

> **Note**: This is a C# conversion of the original GDScript project. The original GDScript version can be found at: [https://github.com/JuhaszLaszlo69/dusttale_mytake](https://github.com/JuhaszLaszlo69/dusttale_mytake)

## Features

### Core Gameplay
- **Turn-Based Combat System**: Engage in battles with unique bullet-hell patterns
- **Multiple Soul Modes**: 
  - **RED Soul**: Free movement in all directions
  - **BLUE Soul**: Gravity-based movement with jumping mechanics
  - **YELLOW Soul**: Free movement with shooting capabilities
- **Boss Battles**: Fight against 4 unique bosses:
  - Cherry
  - Poseur
  - Present
  - Godot
- **Boss Progression System**: Track defeated bosses and unlock end-game content
- **EXP & Gold System**: Earn experience points and gold from defeating enemies
- **Inventory System**: Collect and use items during battles
- **Shop System**: Purchase items using EXP and Gold in a tavern-style shop

### Exploration
- **Multiple Maps**: 
  - Main Map: Primary exploration area with boss encounters
  - Overworld Original: Tavern-style area with shop access
- **Player Position Tracking**: Automatically saves and restores player position when entering/exiting battles
- **Scene Transitions**: Smooth fade transitions between areas
- **Inventory Display**: Always-visible inventory UI while exploring

### Shop System
- **Dual Currency**: Items cost both EXP and Gold
- **Menu-Based Navigation**: Cursor-based shop interface with arrow key navigation
- **Item Purchase**: Buy healing items (Apple, Nice Cream, Pie) for use in battles
- **Visual Shop Interface**: Shopkeeper with animated dialogue

### Progression & Completion
- **Boss Tracking**: Defeated bosses disappear from the map
- **End-Game Cutscene**: Special cutscene triggers when all bosses are defeated
- **Persistent State**: Game state persists across scene transitions

## Controls

### General Navigation
- **Arrow Keys**: Move player / Navigate menus
- **Enter / Space**: Confirm selection / Interact
- **C / X / Backspace**: Cancel / Go back

### Battle Controls
- **Arrow Keys**: Move soul (RED mode) / Move horizontally (BLUE mode) / Move freely (YELLOW mode)
- **Space / Up Arrow**: Jump (BLUE mode) / Shoot (YELLOW mode)
- **Enter**: Select battle action (Attack, Act, Item, Mercy)

### Shop Controls
- **Arrow Keys (Up/Down)**: Navigate menu options
- **Enter**: Select item / Confirm purchase
- **C / X / Backspace**: Go back / Cancel

## Project Structure

```
GodTale/
├── autoloads/          # Global game state management
│   ├── Global.cs       # Boss tracking, EXP/Gold, inventory
│   └── FadeToBlack.cs  # Scene transition system
├── battle/             # Battle system
│   ├── Battle.cs       # Main battle logic
│   ├── BattleZone.cs   # Battle zone detection
│   ├── TextBox.cs      # Battle text display
│   ├── MonsterDialouge.cs # Monster dialogue system
│   └── battle.tscn     # Battle scene
├── bullets/            # Bullet patterns and mechanics
│   ├── Bullet.cs       # Base bullet class
│   ├── LinearBullet.cs # Linear bullet type
│   ├── FollowerBullet.cs # Follower bullet type
│   └── JumpObstacle.cs # Jump obstacle type
├── cutscenes/          # Story cutscenes
│   ├── AllBossesDefeated.cs # End-game cutscene logic
│   └── all_bosses_defeated.tscn # End-game cutscene
├── enemy_data/         # Enemy/boss definitions
│   ├── Enemy.cs        # Base enemy class
│   ├── Cherry.cs       # Cherry boss
│   ├── Poseur.cs       # Poseur boss
│   ├── Present.cs      # Present boss
│   ├── GodotEnemy.cs   # Godot boss
│   ├── cherry.tscn
│   ├── poseur.tscn
│   ├── present.tscn
│   └── godot.tscn
├── items/              # Item resources
│   ├── Item.cs         # Item class
│   ├── apple.tres
│   ├── nice_cream.tres
│   └── pie.tres
├── maps/               # Game maps
│   ├── MainMap.cs      # Main map logic
│   ├── OverworldOriginal.cs # Overworld logic
│   ├── Door.cs         # Door transition logic
│   ├── Door2.cs        # Alternative door logic
│   ├── main_map.tscn   # Main exploration map
│   └── overworld_original.tscn # Tavern/shop map
├── shop/               # Shop system
│   ├── Shop.cs         # Shop logic
│   ├── ShopEntrance.cs # Shop entrance detection
│   ├── ShopCollision.cs # Shop collision detection
│   ├── ShopUiOverlay.cs # Shop UI overlay
│   └── shop.tscn       # Shop scene
├── soul/               # Soul mechanics
│   ├── Soul.cs         # Soul movement and modes
│   ├── YellowShot.cs   # Yellow soul shooting
│   └── Particle.cs     # Particle effects
├── text_box/           # Dialogue system
│   ├── MonsterTextBox.cs # Monster text box
│   ├── CustomButton.cs # Custom button component
│   └── Util.cs         # Text utility functions
├── waves/              # Bullet wave patterns
│   ├── Wave.cs         # Base wave class
│   ├── Shoot1.cs       # Shooting pattern 1
│   ├── Shoot2.cs       # Shooting pattern 2
│   ├── Shoot3.cs       # Shooting pattern 3
│   ├── Jump.cs         # Jump pattern
│   ├── Jump2.cs        # Jump pattern 2
│   ├── FollowSoul.cs   # Follow soul pattern
│   ├── SpinningStorm.cs # Spinning storm pattern
│   ├── SpinningStorm2.cs # Spinning storm pattern 2
│   └── DiamondsFromBelow.cs # Diamonds pattern
├── player/             # Player character
│   └── Scripts/
│       └── Jugador.cs  # Player movement and controls
├── intermediate_scenes/ # Intermediate scenes
│   ├── TitleScreen.cs  # Title screen logic
│   └── title_screen.tscn # Title screen scene
├── UndertaleBattle.csproj # C# project file
└── UndertaleBattle.sln  # C# solution file
```

## Technical Details

### Global State Management
The `Global` autoload singleton manages:
- **Boss Tracking**: `killed_bosses` array tracks defeated bosses
- **Currency**: `player_exp` and `player_gold` for shop purchases
- **Inventory**: `battle_inventory` array stores collected items
- **Position Tracking**: `last_scene_path` and `last_player_position` for seamless transitions
- **Progression**: `AllBossesKilled()` checks for end-game condition

### Battle System
- Turn-based combat with bullet-hell patterns
- HP system (20 HP max)
- Mercy system (spare enemies at 100% mercy)
- Item usage during battles
- EXP and Gold rewards on victory

### Shop System
- Menu-based navigation with cursor
- Dual currency purchases (EXP + Gold)
- Item info display
- Purchase confirmation prompts
- Shopkeeper animations

### Boss System
- 4 unique bosses with individual battle patterns
- Boss sprites disappear from map after defeat
- End-game cutscene triggers when all bosses are defeated
- Boss names: Cherry, Poseur, Present, Godot

## How to Play

1. **Start the Game**: Launch from the title screen
2. **Explore**: Navigate the main map using arrow keys
3. **Battle Bosses**: Enter battle zones to fight bosses
4. **Earn Rewards**: Gain EXP and Gold from defeating enemies
5. **Visit Shop**: Go to the overworld_original map to access the shop
6. **Buy Items**: Purchase healing items for use in battles
7. **Complete the Game**: Defeat all 4 bosses to see the end-game cutscene

## Debug Features

The title screen includes debug options:
- **[DEBUG] - Boss Name** buttons to directly access boss battles
- **[DEBUG] View Cutscene** button to view the end-game cutscene without completing the game

## Requirements

- **Godot Engine**: Version 4.4 or later
- **.NET SDK**: Required for C# support (Godot 4.4+ includes .NET support)
- **Platform**: Windows, Linux, or macOS
- **C# Support**: Must have C# enabled in Godot project settings

## Credits

This is a fan-made project inspired by Undertale by Toby Fox. All original Undertale assets, music, and concepts belong to their respective owners.

**Original GDScript Version**: [https://github.com/JuhaszLaszlo69/dusttale_mytake](https://github.com/JuhaszLaszlo69/dusttale_mytake)

This C# version is a conversion of the original GDScript implementation.

## License

This project is for educational and personal use only. Please respect the original Undertale copyright and do not distribute without proper attribution.

