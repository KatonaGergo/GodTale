using Godot;
using Godot.Collections;

public partial class Soul : CharacterBody2D
{
    [Signal]
    public delegate void TookDamageEventHandler(int amount, Soul soul);
    
    public Color Color { get; set; }
    
    public enum Mode
    {
        Red,
        Blue,
        Yellow,
        Green,
        Purple
    }
    
    public Mode SoulMode { get; set; }
    
    private static PackedScene SOUL = GD.Load<PackedScene>("uid://tly4ac72poq7");

    public static Soul NewSoul(Mode mode)
    {
        Soul soul = SOUL.Instantiate<Soul>();
        soul.SoulMode = mode;
        return soul;
    }

    private Sprite2D _sprite2D;
    private Area2D _hurtbox;
    private Timer _shootTimer;
    private Timer _invincibilityTimer;
    private AnimationPlayer _soulAnim;

    public override void _Ready()
    {
        _sprite2D = GetNode<Sprite2D>("%Sprite2D");
        _hurtbox = GetNode<Area2D>("%Hurtbox");
        _invincibilityTimer = GetNode<Timer>("%invincibilityTimer");
        _soulAnim = GetNode<AnimationPlayer>("%SoulAnim");
        _invincibilityTimer.Timeout += _OnInvincibilityTimerTimeout;
        
        switch (SoulMode)
        {
            case Mode.Red:
                Color = Colors.Red;
                _sprite2D.Texture = GD.Load<Texture2D>("uid://ck5iwib4fr77x");
                break;
            case Mode.Blue:
                Color = Colors.Blue;
                _sprite2D.Texture = GD.Load<Texture2D>("uid://bgfotjyiv112h");
                break;
            case Mode.Yellow:
                Color = Colors.Yellow;
                _sprite2D.Texture = GD.Load<Texture2D>("uid://bgiewq6am86ut");
                _hurtbox.Rotate(Mathf.Pi);
                _shootTimer = GetNode<Timer>("%ShootTimer");
                break;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        switch (SoulMode)
        {
            case Mode.Red:
                Vector2 dir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
                Velocity = dir * 200;
                MoveAndSlide();
                break;
            case Mode.Blue:
                float jumpForce = -500.0f;
                float dirX = Input.GetAxis("ui_left", "ui_right");
                Velocity = new Vector2(dirX * 200, Velocity.Y);
                if (Input.IsActionJustPressed("ui_up") && IsOnFloor())
                {
                    Velocity = new Vector2(Velocity.X, jumpForce);
                }
                if (Input.IsActionJustReleased("ui_up") && Velocity.Y < jumpForce / 2)
                {
                    Velocity = new Vector2(Velocity.X, jumpForce / 2);
                }
                Velocity = new Vector2(Velocity.X, Velocity.Y + 600.0f * (float)delta); // Gravity
                MoveAndSlide();
                break;
            case Mode.Yellow:
                Vector2 yellowDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
                Velocity = yellowDir * 200;
                if (Input.IsActionJustPressed("ui_accept") && _shootTimer != null && _shootTimer.IsStopped())
                {
                    _shootTimer.Start();
                    Global.Singleton.EmitSignal(Global.SignalName.PlayShootSound);
                    PackedScene yellowBullet = GD.Load<PackedScene>("uid://c53touampkpns");
                    Node2D bullet = yellowBullet.Instantiate<Node2D>();
                    Global.Singleton.EmitSignal(Global.SignalName.AddBullet, bullet, GlobalTransform);
                }
                MoveAndSlide();
                break;
        }
        
        // Check for damage:
        Array<Area2D> allAreas = _hurtbox.GetOverlappingAreas();
        foreach (Area2D area in allAreas)
        {
            if (!(area is Bullet) || _invincibilityTimer.TimeLeft > 0) continue;
            
            Bullet bullet = area as Bullet;
            if (bullet != null)
            {
                EmitSignal(SignalName.TookDamage, bullet.DamageAmount, this);
                if (bullet.FreedOnHit)
                {
                    bullet.QueueFree();
                }
                _soulAnim.Play("hurt");
                _invincibilityTimer.Start();
            }
        }
    }

    private void _OnInvincibilityTimerTimeout()
    {
        _soulAnim.Stop();
        // Reset modulate alpha to 1 (fully visible) - match GDScript: %Sprite2D.modulate.a = 1
        _sprite2D.Modulate = new Color(_sprite2D.Modulate.R, _sprite2D.Modulate.G, _sprite2D.Modulate.B, 1.0f);
    }
}
