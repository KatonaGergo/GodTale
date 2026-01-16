using Godot;

public partial class BattleZone : Area2D
{
    [Export]
    public PackedScene enemy_scene { get; set; }

    [Export]
    public string boss_name { get; set; } = "";

    public override async void _Ready()
    {
        // Wait a frame to ensure properties are loaded from scene file
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        
        // Manually load properties from scene file if they weren't loaded automatically
        // This handles the snake_case to PascalCase mapping issue
        if (enemy_scene == null)
        {
            Variant enemySceneVariant = Get("enemy_scene");
            if (enemySceneVariant.VariantType == Variant.Type.Object && enemySceneVariant.AsGodotObject() is PackedScene scene)
            {
                enemy_scene = scene;
                GD.Print($"BattleZone._Ready: Loaded enemy_scene from Get(): {enemy_scene?.ResourcePath}");
            }
        }
        
        // Always try to load boss_name from scene file, even if it seems empty
        // The property might not be loaded yet due to C# property mapping issues
        Variant bossNameVariant = Get("boss_name");
        if (bossNameVariant.VariantType == Variant.Type.String)
        {
            string loadedBossName = bossNameVariant.AsString();
            if (!string.IsNullOrEmpty(loadedBossName))
            {
                boss_name = loadedBossName;
                GD.Print($"BattleZone._Ready: Loaded boss_name from Get(): '{boss_name}'");
            }
        }
        
        // If still empty, try to infer from node name (e.g., "GodotBattleZone" -> "Godot")
        if (string.IsNullOrEmpty(boss_name))
        {
            string nodeName = Name;
            if (nodeName.Contains("BattleZone"))
            {
                boss_name = nodeName.Replace("BattleZone", "");
                GD.Print($"BattleZone._Ready: Inferred boss_name from node name '{nodeName}': '{boss_name}'");
            }
        }
        
        // Check if this boss is already killed and hide the zone if so
        // Match GDScript: visible = false, set_deferred("monitoring", false), set_deferred("monitorable", false)
        GD.Print($"BattleZone._Ready: Checking boss '{boss_name}' - IsBossKilled: {Global.Singleton.IsBossKilled(boss_name)}");
        if (boss_name != "" && Global.Singleton.IsBossKilled(boss_name))
        {
            Visible = false;
            Monitoring = false;
            Monitorable = false;
            
            // Also disable the CollisionShape2D child to completely disable collision detection
            CollisionShape2D collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
            if (collisionShape != null)
            {
                collisionShape.SetDeferred("disabled", true);
            }
            
            GD.Print($"BattleZone: Boss '{boss_name}' is already defeated. Zone disabled and hidden.");
            return;
        }
        else
        {
            GD.Print($"BattleZone._Ready: Boss '{boss_name}' is NOT killed. Zone will remain active.");
        }

        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.Name == "Jugador" || body.Name == "Player")
        {
            // Check again if boss is killed (in case it was defeated after _Ready was called)
            // Only start battle if boss is NOT killed (allows replay if player lost)
            if (boss_name == "" || !Global.Singleton.IsBossKilled(boss_name))
            {
                StartBattle();
            }
            else
            {
                // Boss is already defeated - disable collision to prevent further detection
                GD.Print($"BattleZone: Boss '{boss_name}' is already defeated. Disabling collision and preventing battle.");
                Monitoring = false;
                Monitorable = false;
                CollisionShape2D collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
                if (collisionShape != null)
                {
                    collisionShape.Disabled = true;
                }
            }
        }
    }

    private async void StartBattle()
    {
        if (enemy_scene == null)
        {
            // Default to cherry if none specified
            GD.PrintErr($"BattleZone: enemy_scene is null! Defaulting to cherry. Boss name: {boss_name}");
            enemy_scene = GD.Load<PackedScene>("res://enemy_data/cherry.tscn");
        }
        else
        {
            GD.Print($"BattleZone: Loading enemy scene: {enemy_scene.ResourcePath}, Boss name: {boss_name}");
        }

        // Save current scene and player position before battle
        string currentScene = GetTree().CurrentScene.SceneFilePath;
        Global.Singleton.lastScenePath = currentScene;

        // Find player and save position
        Node2D player = null;
        Node sceneRoot = GetTree().CurrentScene;

        // Try different ways to find the player
        Node playerNode = sceneRoot.GetNodeOrNull("Player");
        if (playerNode != null && playerNode is Node2D node2d)
        {
            player = node2d;
        }
        else
        {
            playerNode = sceneRoot.GetNodeOrNull("Jugador");
            if (playerNode != null && playerNode is Node2D node2d2)
            {
                player = node2d2;
            }
            else
            {
                playerNode = sceneRoot.GetNodeOrNull("Objetos/Jugador");
                if (playerNode != null && playerNode is Node2D node2d3)
                {
                    player = node2d3;
                }
            }
        }

        if (player == null)
        {
            // Try finding by groups
            Godot.Collections.Array<Node> players = GetTree().GetNodesInGroup("player");
            foreach (Node p in players)
            {
                if (p is Node2D playerNode2d)
                {
                    player = playerNode2d;
                    break;
                }
            }
        }

        if (player != null && player is Node2D player2d)
        {
            Global.Singleton.lastPlayerPosition = player2d.GlobalPosition;
        }
        else
        {
            Global.Singleton.lastPlayerPosition = Vector2.Zero;
        }

        Node enemyInstance = enemy_scene.Instantiate();
        if (enemyInstance != null)
        {
            Battle.enemy = enemyInstance as Enemy;
            // Store the boss_name in Global so Battle.cs can use it to mark the boss as killed
            // This ensures the boss name matches between BattleZone and Battle
            if (!string.IsNullOrEmpty(boss_name))
            {
                Global.Singleton.currentBossName = boss_name;
                GD.Print($"BattleZone: Stored current boss name '{boss_name}' in Global for battle marking.");
            }
            FadeToBlack fade = GetNode<FadeToBlack>("/root/Fade");
            await fade.FadeIntoBlack();
            GetTree().ChangeSceneToFile("uid://45qmet5s5aix");
        }
    }
}
