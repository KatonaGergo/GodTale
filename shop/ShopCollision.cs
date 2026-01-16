using Godot;
using System.Threading.Tasks;

public partial class ShopCollision : Area2D
{
    private bool _playerNearby = false;
    private Control _shopUi = null;

    public override void _Ready()
    {
        BodyEntered += _OnBodyEntered;
        BodyExited += _OnBodyExited;
        // Find shop UI in the scene
        _shopUi = GetTree().CurrentScene.GetNodeOrNull<Control>("ShopUI");
    }

    private void _OnBodyEntered(Node2D body)
    {
        if (body.Name == "Jugador" || body.Name == "Player")
        {
            _playerNearby = true;
            ShowInteractionPrompt();
        }
    }

    private void _OnBodyExited(Node2D body)
    {
        if (body.Name == "Jugador" || body.Name == "Player")
        {
            _playerNearby = false;
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_accept") && _playerNearby)
        {
            Godot.Collections.Array<Node2D> bodies = GetOverlappingBodies();
            foreach (Node2D body in bodies)
            {
                if (body.Name == "Jugador" || body.Name == "Player")
                {
                    OpenShop();
                    return;
                }
            }
        }
    }

    private void ShowInteractionPrompt()
    {
        // Could add a visual prompt here
    }

    private async void OpenShop()
    {
        // Save current scene and player position before entering shop
        string currentScene = GetTree().CurrentScene.SceneFilePath;
        Global.Singleton.lastScenePath = currentScene;
        
        Node2D player = null;
        Node sceneRoot = GetTree().CurrentScene;
        player = sceneRoot.GetNodeOrNull<Node2D>("Objetos/Jugador");
        
        if (player == null)
        {
            player = sceneRoot.GetNodeOrNull<Node2D>("Jugador");
        }
        
        if (player == null)
        {
            player = sceneRoot.GetNodeOrNull<Node2D>("Player");
        }
        
        if (player == null)
        {
            Godot.Collections.Array<Node> players = GetTree().GetNodesInGroup("player");
            if (players.Count > 0 && players[0] is Node2D)
            {
                player = (Node2D)players[0];
            }
        }
        
        if (player != null)
        {
            Global.Singleton.lastPlayerPosition = player.GlobalPosition;
        }
        else
        {
            Global.Singleton.lastPlayerPosition = Vector2.Zero;
        }
        
        // Change to shop scene with fade
        FadeToBlack fade = GetNode<FadeToBlack>("/root/Fade");
        await fade.FadeIntoBlack();
        GetTree().ChangeSceneToFile("res://shop/shop.tscn");
    }
}
