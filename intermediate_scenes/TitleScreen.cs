using Godot;
using Godot.Collections;

public partial class TitleScreen : CanvasLayer
{
    [Export]
    public Array<PackedScene> Enemies { get; set; } = new Array<PackedScene>();

    private RichTextLabel _title;
    private VBoxContainer _battlesContainer;
    private AudioStreamPlayer _song;
    private AudioStreamPlayer _moveSound;
    private AudioStreamPlayer _encounter1;
    private AudioStreamPlayer _encounter2;

    public override void _Ready()
    {
        _title = GetNode<RichTextLabel>("%Title");
        _battlesContainer = GetNode<VBoxContainer>("%BattlesContainer");
        _song = GetNode<AudioStreamPlayer>("%Song");
        _moveSound = GetNode<AudioStreamPlayer>("%MoveSound");
        _encounter1 = GetNode<AudioStreamPlayer>("%Encounter1");
        _encounter2 = GetNode<AudioStreamPlayer>("%Encounter2");
        
        // Fallback: Try to load enemies array manually if it's empty (property name mismatch issue)
        if (Enemies == null || Enemies.Count == 0)
        {
            GD.Print("Enemies array is empty, loading enemy scenes directly...");
            Enemies = new Array<PackedScene>();
            
            // Load enemy scenes directly from their paths (matching the scene file)
            string[] enemyPaths = {
                "res://enemy_data/godot.tscn",
                "res://enemy_data/poseur.tscn",
                "res://enemy_data/cherry.tscn",
                "res://enemy_data/present.tscn"
            };
            
            foreach (string path in enemyPaths)
            {
                PackedScene scene = GD.Load<PackedScene>(path);
                if (scene != null)
                {
                    Enemies.Add(scene);
                    GD.Print($"Loaded enemy scene: {path}");
                }
                else
                {
                    GD.PrintErr($"Failed to load enemy scene: {path}");
                }
            }
            
            GD.Print($"Loaded {Enemies.Count} enemies from scene paths");
        }
        
        GD.Print($"Enemies array count: {Enemies.Count}");
        
        FadeToBlack fade = GetNode<FadeToBlack>("/root/Fade");
        fade.FadeFromBlack();
        _title.Text = Util.Shake(_title.Text);
        
        // Add overworld button first
        Godot.Button overworldButton = new Godot.Button();
        overworldButton.AddThemeFontSizeOverride("font_size", 50);
        overworldButton.Text = "Go to Overworld";
        overworldButton.CustomMinimumSize = new Vector2(400, 60);
        overworldButton.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        _battlesContainer.AddChild(overworldButton);
        overworldButton.FocusEntered += () => _OnFocusEntered(overworldButton);
        overworldButton.FocusExited += () => _OnFocusExited(overworldButton);
        overworldButton.Pressed += GoToOverworld;
        
        // Add debug button to view cutscene
        Godot.Button debugButton = new Godot.Button();
        debugButton.AddThemeFontSizeOverride("font_size", 50);
        debugButton.Text = "[DEBUG] View Cutscene";
        debugButton.CustomMinimumSize = new Vector2(400, 60);
        debugButton.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        _battlesContainer.AddChild(debugButton);
        debugButton.FocusEntered += () => _OnFocusEntered(debugButton);
        debugButton.FocusExited += () => _OnFocusExited(debugButton);
        debugButton.Pressed += GoToCutscene;
        
        foreach (PackedScene scene in Enemies)
        {
            if (scene == null)
            {
                GD.PrintErr("Null scene in Enemies array!");
                continue;
            }
            
            Node instance = scene.Instantiate();
            if (instance == null)
            {
                GD.PrintErr($"Failed to instantiate scene: {scene.ResourcePath}");
                continue;
            }
            
            if (!(instance is Enemy))
            {
                GD.PrintErr($"Scene {scene.ResourcePath} is not an Enemy!");
                instance.QueueFree();
                continue;
            }
            
            Enemy enemy = instance as Enemy;
            if (enemy == null)
            {
                GD.PrintErr("Failed to cast to Enemy!");
                instance.QueueFree();
                continue;
            }
            
            // Use node name directly (like Battle.cs line 200 does) - this is available immediately
            // The node name from the scene file (e.g., "Godot", "Cherry", "Poseur", "Present") 
            // is set in the scene file and available right after instantiation
            string enemyName = enemy.Name;
            
            // Also set EnemyName property for use in battle system
            // Add to scene tree temporarily to load properties
            AddChild(enemy);
            GetTree().ProcessFrame += LoadEnemyProperty;
            
            void LoadEnemyProperty()
            {
                GetTree().ProcessFrame -= LoadEnemyProperty;
                
                // Wait one more frame like BattleZone.cs does
                GetTree().ProcessFrame += LoadEnemyPropertyFinal;
            }
            
            void LoadEnemyPropertyFinal()
            {
                GetTree().ProcessFrame -= LoadEnemyPropertyFinal;
                
                // Try to load enemy_name property from scene file
                if (string.IsNullOrEmpty(enemy.EnemyName) || enemy.EnemyName == "Enemy name here")
                {
                    Variant enemyNameVariant = enemy.Get("enemy_name");
                    string loadedName = enemyNameVariant.AsString();
                    if (!string.IsNullOrEmpty(loadedName))
                    {
                        enemy.EnemyName = loadedName;
                    }
                    else
                    {
                        // Fallback to node name if property not loaded
                        enemy.EnemyName = enemyName;
                    }
                }
                
                // Remove from scene tree
                RemoveChild(enemy);
                
                // Create button with the enemy name (use node name which is reliable)
                CreateEnemyButton(enemy, enemyName);
            }
        }
        
        // Set focus after a short delay to allow buttons to be created
        GetTree().ProcessFrame += SetInitialFocus;
    }
    
    private void SetInitialFocus()
    {
        GetTree().ProcessFrame -= SetInitialFocus;
        if (_battlesContainer.GetChild(0) is Godot.Button firstButton)
        {
            firstButton.GrabFocus();
        }
    }
    
    private void CreateEnemyButton(Enemy enemy, string enemyName)
    {
        Godot.Button button = new Godot.Button();
        button.AddThemeFontSizeOverride("font_size", 50);
        button.Text = $"[DEBUG] - {enemyName}";
        button.CustomMinimumSize = new Vector2(400, 60);
        button.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        _battlesContainer.AddChild(button);
        button.FocusEntered += () => _OnFocusEntered(button);
        button.FocusExited += () => _OnFocusExited(button);
        button.Pressed += () => GoToBattle(enemy);
        GD.Print($"Created button for enemy: {enemyName}");
    }

    private async void GoToOverworld()
    {
        _song.Stop();
        FadeToBlack fade = GetNode<FadeToBlack>("/root/Fade");
        await fade.FadeIntoBlack();
        
        // Try loading the scene first to check if it exists
        PackedScene scene = GD.Load<PackedScene>("res://maps/overworld_original.tscn");
        if (scene == null)
        {
            GD.Print("ERROR: Could not load overworld_original.tscn");
            // Try alternative
            scene = GD.Load<PackedScene>("res://overworld.tscn");
            if (scene == null)
            {
                GD.Print("ERROR: Could not load overworld.tscn either!");
                return;
            }
        }
        
        Error error = GetTree().ChangeSceneToFile("res://maps/overworld_original.tscn");
        if (error != Error.Ok)
        {
            GD.Print("Error changing scene: ", error);
            GD.Print("Error code: ", error);
        }
    }

    private void _OnFocusEntered(Godot.Button button)
    {
        button.Modulate = new Color(button.Modulate.R, button.Modulate.G, button.Modulate.B, 1.0f);
        _moveSound.Play();
        button.PivotOffset = button.Size / 2;
        Tween tween = GetTree().CreateTween();
        tween.SetEase(Tween.EaseType.InOut);
        tween.SetTrans(Tween.TransitionType.Bounce);
        tween.TweenProperty(button, "scale", new Vector2(1.5f, 1.5f), 0.2);
        tween.TweenProperty(button, "scale", new Vector2(1.0f, 1.0f), 0.1);
    }

    private void _OnFocusExited(Godot.Button button)
    {
        button.Modulate = new Color(button.Modulate.R, button.Modulate.G, button.Modulate.B, 0.5f);
    }

    private async void GoToCutscene()
    {
        _song.Stop();
        FadeToBlack fade = GetNode<FadeToBlack>("/root/Fade");
        await fade.FadeIntoBlack();
        if (IsInsideTree() && GetTree() != null)
        {
            GetTree().ChangeSceneToFile("res://cutscenes/all_bosses_defeated.tscn");
        }
    }

    private async void GoToBattle(Enemy enemy)
    {
        _song.Stop();
        _encounter1.Play();
        string debugText = $"[DEBUG] - {enemy.EnemyName}";
        foreach (Node child in _battlesContainer.GetChildren())
        {
            if (child is Godot.Button button)
            {
                button.Modulate = new Color(button.Modulate.R, button.Modulate.G, button.Modulate.B, button.Text == debugText ? 1.0f : 0.0f);
                button.ReleaseFocus();
            }
        }
        await ToSignal(GetTree().CreateTimer(0.25), Timer.SignalName.Timeout);
        _encounter2.Play();
        FadeToBlack fade = GetNode<FadeToBlack>("/root/Fade");
        await fade.FadeIntoBlack();
        Battle.enemy = enemy;
        GetTree().ChangeSceneToFile("uid://45qmet5s5aix");
    }
}
