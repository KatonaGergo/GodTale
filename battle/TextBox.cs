using Godot;
using System.Threading.Tasks;

public partial class TextBox : RichTextLabel
{
    [Signal]
    public delegate void FinishedScrollingEventHandler();

    public float NormalSpeed { get; set; } = 0.05f;
    // Slow down dialouge if '.' or '\n' char is encountered:
    public float SlowSpeed { get; set; } = 0.5f;

    private Timer _timer;
    private AudioStreamPlayer _textSound;

    public override void _Ready()
    {
        _timer = GetNode<Timer>("%Timer");
        _textSound = GetNode<AudioStreamPlayer>("%TextSound");
        
        // Connect timer signal (scene file connection might not work in C#)
        _timer.Timeout += _OnTimerTimeout;
    }

    public async Task Scroll(string newText)
    {
        await ClearText();
        Text = Util.Shake(newText, 5, 5);
        VisibleCharacters = 0;
        _timer.Start();
        _textSound.Play();
    }

    public async Task SetNewText(string newText)
    {
        await ClearText();
        VisibleRatio = 1.0f;
        Text = Util.Shake(newText);
    }

    private void _OnTimerTimeout()
    {
        if (VisibleRatio == 1.0f || GetParsedText().Length == 0)
        {
            _timer.Stop();
            EmitSignal(SignalName.FinishedScrolling);
            return;
        }
        _textSound.Play();
        _timer.WaitTime = NextChar() == "." || NextChar() == "\n" ? SlowSpeed : NormalSpeed;
        VisibleCharacters += 1;
    }

    private string NextChar()
    {
        string t = GetParsedText();
        return VisibleCharacters + 1 == t.Length ? "" : t[VisibleCharacters + 1].ToString();
    }

    public async Task ClearText()
    {
        _timer.Stop();
        VisibleRatio = 0.0f;
    }
}
