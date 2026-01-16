using Godot;
using Godot.Collections;

public partial class MainMap : Node2D
{
    private AudioStreamPlayer _musicPlayer;
    private VBoxContainer _inventoryContainer;
    private Label _inventoryLabel;
    private Label _expLabel;
    private Label _goldLabel;

    public override async void _Ready()
    {
        _musicPlayer = GetNode<AudioStreamPlayer>("MusicPlayer");
        _inventoryContainer = GetNode<VBoxContainer>("%InventoryContainer");
        _inventoryLabel = GetNode<Label>("%InventoryLabel");
        _expLabel = GetNode<Label>("%ExpLabel");
        _goldLabel = GetNode<Label>("%GoldLabel");
        
        FadeToBlack fade = GetNode<FadeToBlack>("/root/Fade");
        fade.FadeFinished += _OnFadeFinished;
        fade.FadeFromBlack();
        SetupInventoryUi();
        UpdateInventoryDisplay();
        UpdateExpDisplay();
        UpdateGoldDisplay();
        
        // Hide boss sprites if they are already defeated
        HideDefeatedBosses();
        
        // Restore player position if returning from battle
        if (Global.Singleton.lastPlayerPosition != Vector2.Zero)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            RestorePlayerPosition();
        }
        
        // Check if all bosses are killed and trigger cutscene
        if (Global.Singleton.AllBossesKilled() && !Global.Singleton.cutscenePlayed)
        {
            await ToSignal(GetTree().CreateTimer(0.5), Timer.SignalName.Timeout);
            TriggerCutscene();
        }
    }

    private void _OnFadeFinished()
    {
        if (_musicPlayer == null || !IsInstanceValid(_musicPlayer))
        {
            return;
        }
        _musicPlayer.Stream = GD.Load<AudioStream>("res://songs/005. Ruins (UNDERTALE Soundtrack) - Toby Fox.mp3");
        _musicPlayer.Play();
    }

    public override void _ExitTree()
    {
        FadeToBlack fade = GetNodeOrNull<FadeToBlack>("/root/Fade");
        if (fade != null)
        {
            fade.FadeFinished -= _OnFadeFinished;
        }
        base._ExitTree();
    }

    private void SetupInventoryUi()
    {
        // Inventory UI is set up in the scene, just update display
    }

    private void UpdateInventoryDisplay()
    {
        if (Global.Singleton.battleInventory.Count == 0)
        {
            _inventoryLabel.Text = "Inventory:\n(Empty)";
        }
        else
        {
            Array<string> itemNames = new Array<string>();
            foreach (Item item in Global.Singleton.battleInventory)
            {
                itemNames.Add(item.ItemName);
            }
            _inventoryLabel.Text = "Inventory:\n" + string.Join("\n", itemNames);
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
        Node playerNode = GetNodeOrNull("Player");
        if (playerNode != null && playerNode is Node2D)
        {
            player = (Node2D)playerNode;
        }
        else
        {
            playerNode = GetNodeOrNull("Jugador");
            if (playerNode != null && playerNode is Node2D)
            {
                player = (Node2D)playerNode;
            }
        }
        
        if (player != null && player is Node2D && Global.Singleton.lastPlayerPosition != Vector2.Zero)
        {
            player.GlobalPosition = Global.Singleton.lastPlayerPosition;
            Global.Singleton.lastPlayerPosition = Vector2.Zero;
            Global.Singleton.lastScenePath = "";
        }
    }

    private void HideDefeatedBosses()
    {
        // Hide boss sprites if they are already defeated
        Node2D cherrySprite = GetNodeOrNull<Node2D>("%CherrySprite");
        if (cherrySprite != null && Global.Singleton.IsBossKilled("Cherry"))
        {
            cherrySprite.Visible = false;
        }
        
        Node2D poseurSprite = GetNodeOrNull<Node2D>("%PoseurSprite");
        if (poseurSprite != null && Global.Singleton.IsBossKilled("Poseur"))
        {
            poseurSprite.Visible = false;
        }
        
        Node2D presentSprite = GetNodeOrNull<Node2D>("%PresentSprite");
        if (presentSprite != null && Global.Singleton.IsBossKilled("Present"))
        {
            presentSprite.Visible = false;
        }
        
        Node2D godotSprite = GetNodeOrNull<Node2D>("%GodotSprite");
        if (godotSprite != null && Global.Singleton.IsBossKilled("Godot"))
        {
            godotSprite.Visible = false;
        }
    }

    private async void TriggerCutscene()
    {
        Global.Singleton.cutscenePlayed = true;
        FadeToBlack fade = GetNode<FadeToBlack>("/root/Fade");
        await fade.FadeIntoBlack();
        GetTree().ChangeSceneToFile("res://cutscenes/all_bosses_defeated.tscn");
    }
}
