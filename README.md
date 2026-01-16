# Undertale Clone i.e GodTale

A fan-made Undertale-inspired game built with Godot Engine 4.4. This project recreates the core gameplay mechanics of Undertale, including turn-based combat with bullet-hell patterns, exploration, boss battles, and a shop system.

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
dusttale_mytake/
├── autoloads/          # Global game state management
│   ├── global.gd       # Boss tracking, EXP/Gold, inventory
│   └── fade_to_black.gd # Scene transition system
├── battle/             # Battle system
│   ├── battle.gd       # Main battle logic
│   └── battle.tscn     # Battle scene
├── bullets/            # Bullet patterns and mechanics
├── cutscenes/          # Story cutscenes
│   └── all_bosses_defeated/ # End-game cutscene
├── enemy_data/         # Enemy/boss definitions
│   ├── cherry.tscn
│   ├── poseur.tscn
│   ├── present.tscn
│   └── godot.tscn
├── items/              # Item resources
│   ├── apple.tres
│   ├── nice_cream.tres
│   └── pie.tres
├── maps/               # Game maps
│   ├── main_map.tscn   # Main exploration map
│   └── overworld_original.tscn # Tavern/shop map
├── shop/               # Shop system
│   ├── shop.gd         # Shop logic
│   └── shop.tscn       # Shop scene
├── soul/               # Soul mechanics
├── text_box/           # Dialogue system
└── waves/              # Bullet wave patterns
```

## Technical Details

### Global State Management
The `Global` autoload singleton manages:
- **Boss Tracking**: `killed_bosses` array tracks defeated bosses
- **Currency**: `player_exp` and `player_gold` for shop purchases
- **Inventory**: `battle_inventory` array stores collected items
- **Position Tracking**: `last_scene_path` and `last_player_position` for seamless transitions
- **Progression**: `all_bosses_killed()` checks for end-game condition

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
- **Platform**: Windows, Linux, or macOS

## Credits

This is a fan-made project inspired by Undertale by Toby Fox. All original Undertale assets, music, and concepts belong to their respective owners.

## License

This project is for educational and personal use only. Please respect the original Undertale copyright and do not distribute without proper attribution.

