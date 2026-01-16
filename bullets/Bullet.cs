using Godot;

public partial class Bullet : Area2D
{
    [Export]
    public float Speed { get; set; } = 500.0f;
    
    [Export]
    public int DamageAmount { get; set; } = 5;
    
    [Export]
    public bool FreedOnHit { get; set; } = true;
    
    [Export]
    public bool Shootable { get; set; } = false;

    public override void _Ready()
    {
        VisibleOnScreenNotifier2D notifier = GetNodeOrNull<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
        if (notifier != null)
        {
            notifier.ScreenExited += OnScreenExited;
        }
    }

    private void OnScreenExited()
    {
        QueueFree();
    }
}
