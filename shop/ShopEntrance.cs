using Godot;
using System.Threading.Tasks;

public partial class ShopEntrance : Area2D
{
    private bool _playerNearby = false;

    public override void _Ready()
    {
        BodyEntered += _OnBodyEntered;
        BodyExited += _OnBodyExited;
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
                    EnterShop();
                    return;
                }
            }
        }
    }

    private void ShowInteractionPrompt()
    {
        // Could add a visual prompt here
    }

    private async Task EnterShop()
    {
        FadeToBlack fade = GetNode<FadeToBlack>("/root/Fade");
        await fade.FadeIntoBlack();
        GetTree().ChangeSceneToFile("res://shop/shop.tscn");
    }
}
