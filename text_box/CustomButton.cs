using Godot;

public partial class CustomButton : Godot.Button
{
    private AudioStreamPlayer _moveSound;

    public override void _Ready()
    {
        _moveSound = GetNode<AudioStreamPlayer>("%MoveSound");
        FocusEntered += () =>
        {
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 1.0f);
            _moveSound.Play();
            Tween tween = GetTree().CreateTween();
            tween.SetEase(Tween.EaseType.InOut);
            tween.SetTrans(Tween.TransitionType.Bounce);
            tween.TweenProperty(this, "scale", new Vector2(1.5f, 1.5f), 0.2f);
            tween.TweenProperty(this, "scale", new Vector2(1.0f, 1.0f), 0.1f);
        };
    }
}
