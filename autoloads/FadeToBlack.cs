using Godot;
using System.Threading.Tasks;

public partial class FadeToBlack : CanvasLayer
{
    [Signal]
    public delegate void FadeFinishedEventHandler();

    private AnimationPlayer _anim;

    public override void _Ready()
    {
        _anim = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public async Task FadeIntoBlack(float duration = 1.0f)
    {
        _anim.Play("to_black");
        _anim.SpeedScale = (float)(_anim.CurrentAnimationLength / duration);
        await ToSignal(_anim, AnimationPlayer.SignalName.AnimationFinished);
    }

    public async Task FadeFromBlack()
    {
        _anim.SpeedScale = 1.0f;
        _anim.Play("from_black");
        await ToSignal(_anim, AnimationPlayer.SignalName.AnimationFinished);
        EmitSignal(SignalName.FadeFinished);
    }
}
