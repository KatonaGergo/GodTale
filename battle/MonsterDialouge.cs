using Godot;

public partial class MonsterDialouge : RichTextLabel
{
    [Signal]
    public delegate void PlayMonsterSpeakAnimEventHandler();

    // Undertale text speed is 30 characters per second:
    private float _textSpeed = 1.0f / 30.0f;

    private Timer _textTimer;
    private AudioStreamPlayer _speakSound;

    public override void _Ready()
    {
        _textTimer = GetNode<Timer>("TextTimer");
        _speakSound = GetNode<AudioStreamPlayer>("%SpeakSound");
    }

    public void Display(string newText)
    {
        Text = newText;
        VisibleCharacters = 0;
        _textTimer.Start();
        EmitSignal(SignalName.PlayMonsterSpeakAnim);
    }

    private void _OnTextTimerTimeout()
    {
        _speakSound.Play();
        VisibleCharacters += 1;
        if (VisibleRatio < 1.0f)
        {
            _textTimer.Start(_textSpeed);
        }
    }

    public void StopTalking()
    {
        Text = "";
        VisibleRatio = 1.0f;
        _textTimer.Stop();
    }
}
