using Godot;
using Godot.Collections;
using System.Threading.Tasks;

public partial class AllBossesDefeated : Node2D
{
    private Label _textLabel;
    private CpuParticles2D _particles;

    private Array<string> _messages = new Array<string>
    {
        "Game over you killed every boss!",
        "You have achieved the ultimate power...",
        "But at what cost?",
        "The world is now empty...",
        "Only you remain.",
        "Determined."
    };

    private int _currentMessageIndex = 0;

    public override async void _Ready()
    {
        _textLabel = GetNode<Label>("%TextLabel");
        _particles = GetNode<CpuParticles2D>("%Particles");
        
        // Clear text immediately and hide it
        _textLabel.Text = "";
        _textLabel.Modulate = new Color(_textLabel.Modulate.R, _textLabel.Modulate.G, _textLabel.Modulate.B, 0.0f);
        _textLabel.Visible = true;
        
        // Center particles based on viewport
        Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
        _particles.Position = viewportSize / 2.0f;
        
        // Wait for fade to complete before starting
        FadeToBlack fade = GetNode<FadeToBlack>("/root/Fade");
        await fade.FadeFromBlack();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); // Ensure everything is ready
        StartCutscene();
    }

    private async Task StartCutscene()
    {
        // Start particle effects
        _particles.Emitting = true;
        
        // Show first message
        await ShowMessage(_messages[0]);
        await ToSignal(GetTree().CreateTimer(1.0), Timer.SignalName.Timeout);
        
        // Show remaining messages
        for (int i = 1; i < _messages.Count; i++)
        {
            _textLabel.Text = "";
            await ToSignal(GetTree().CreateTimer(0.5), Timer.SignalName.Timeout);
            await ShowMessage(_messages[i]);
            await ToSignal(GetTree().CreateTimer(1.5), Timer.SignalName.Timeout);
        }
        
        // Final fade out
        await ToSignal(GetTree().CreateTimer(2.0), Timer.SignalName.Timeout);
        
        // Fade out text
        Tween fadeTween = CreateTween();
        fadeTween.TweenProperty(_textLabel, "modulate:a", 0.0, 0.5);
        await ToSignal(fadeTween, Tween.SignalName.Finished);
        
        _textLabel.Text = "";
        _particles.Emitting = false;
        
        // Fade to black and transition
        FadeToBlack fade = GetNode<FadeToBlack>("/root/Fade");
        await fade.FadeIntoBlack();
        if (IsInsideTree() && GetTree() != null)
        {
            GetTree().ChangeSceneToFile("res://maps/main_map.tscn");
        }
    }

    private async Task ShowMessage(string message)
    {
        // Clear and hide text first
        _textLabel.Text = "";
        _textLabel.Modulate = new Color(_textLabel.Modulate.R, _textLabel.Modulate.G, _textLabel.Modulate.B, 0.0f);
        _textLabel.Visible = true;
        
        // Fade in text label first (quick fade)
        Tween fadeTween = CreateTween();
        fadeTween.TweenProperty(_textLabel, "modulate:a", 1.0, 0.2);
        await ToSignal(fadeTween, Tween.SignalName.Finished);
        
        // Now type out message letter by letter (while visible)
        string fullText = "";
        for (int i = 0; i < message.Length; i++)
        {
            fullText += message[i];
            _textLabel.Text = fullText;
            await ToSignal(GetTree().CreateTimer(0.05), Timer.SignalName.Timeout);
        }
        
        // Wait a bit after typing completes
        await ToSignal(GetTree().CreateTimer(0.5), Timer.SignalName.Timeout);
    }
}
