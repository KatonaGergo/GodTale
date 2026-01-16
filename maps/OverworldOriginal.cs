using Godot;

public partial class OverworldOriginal : Node2D
{
    [Export]
    public PackedScene DefaultEnemy { get; set; }

    private Label _expLabel;
    private Label _goldLabel;

    public override async void _Ready()
    {
        _expLabel = GetNode<Label>("%ExpLabel");
        _goldLabel = GetNode<Label>("%GoldLabel");
        
        // Fade in from black
        FadeToBlack fade = GetNode<FadeToBlack>("/root/Fade");
        fade.FadeFromBlack();
        UpdateExpDisplay();
        UpdateGoldDisplay();
        
        // Restore player position if returning from battle
        if (Global.Singleton.lastPlayerPosition != Vector2.Zero)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            RestorePlayerPosition();
        }
    }

    private void UpdateExpDisplay()
    {
        _expLabel.Text = $"EXP: {Global.Singleton.playerExp}";
    }

    private void UpdateGoldDisplay()
    {
        _goldLabel.Text = $"Gold: {Global.Singleton.playerGold}";
    }

    private void RestorePlayerPosition()
    {
        Node2D player = null;
        Node playerNode = GetNodeOrNull("Objetos/Jugador");
        if (playerNode != null && playerNode is Node2D)
        {
            player = (Node2D)playerNode;
        }
        
        if (player != null && player is Node2D && Global.Singleton.lastPlayerPosition != Vector2.Zero)
        {
            player.GlobalPosition = Global.Singleton.lastPlayerPosition;
            Global.Singleton.lastPlayerPosition = Vector2.Zero;
            Global.Singleton.lastScenePath = "";
        }
    }
}
