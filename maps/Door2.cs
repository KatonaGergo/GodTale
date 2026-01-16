using Godot;
using System.Threading.Tasks;

public partial class Door2 : Area2D
{
    [Export]
    public string TargetScene { get; set; } = "";

    [Export]
    public Vector2 TargetPosition { get; set; } = Vector2.Zero;

    private bool _playerNearby = false;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.Name == "Jugador" || body.Name == "Player")
        {
            _playerNearby = true;
            ShowInteractionPrompt();
        }
    }

    private void OnBodyExited(Node2D body)
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
            // Double check player is still overlapping
            Godot.Collections.Array<Node2D> bodies = GetOverlappingBodies();
            foreach (Node2D body in bodies)
            {
                if (body.Name == "Jugador" || body.Name == "Player")
                {
                    EnterDoor();
                    return;
                }
            }
        }
    }

    private void ShowInteractionPrompt()
    {
        // Could add a visual prompt here
    }

    private async void EnterDoor()
    {
        FadeToBlack fade = GetNode<FadeToBlack>("/root/Fade");
        await fade.FadeIntoBlack();
        if (IsInsideTree() && GetTree() != null)
        {
            string scenePath = TargetScene != "" ? TargetScene : "res://maps/overworld_original.tscn";
            GetTree().ChangeSceneToFile(scenePath);
        }
    }
}
