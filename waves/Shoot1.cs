using Godot;

public partial class Shoot1 : Wave
{
    private PathFollow2D _pathFollow2D;
    private RichTextLabel _instructions;
    private Timer _spawnTimer;
    private Timer _shootCooldownTimer;

    public override void _Ready()
    {
        base._Ready();
        GlobalPosition = new Vector2(650, 0);
        _pathFollow2D = GetNode<PathFollow2D>("%PathFollow2D");
        _instructions = GetNode<RichTextLabel>("%Instructions");
        _instructions.GlobalPosition = GetViewport().GetVisibleRect().Size / 2 + new Vector2(-_instructions.Size.X, _instructions.Size.Y) / 2;
        _instructions.Text = Util.Shake(_instructions.Text);
        _spawnTimer = GetNode<Timer>("SpawnTimer");
        _spawnTimer.Timeout += _OnSpawnTimerTimeout;
        // Ensure timer is not one-shot and starts (autostart might not work in C#)
        _spawnTimer.OneShot = false;
        _spawnTimer.Start();
        
        // Create cooldown timer for player shooting (spacebar)
        _shootCooldownTimer = new Timer();
        _shootCooldownTimer.WaitTime = 0.2f; // Cooldown between shots
        _shootCooldownTimer.OneShot = true;
        _shootCooldownTimer.Name = "ShootCooldownTimer";
        AddChild(_shootCooldownTimer);
    }
    
    public override void _Input(InputEvent @event)
    {
        // Handle spacebar to shoot player bullets
        if (@event.IsActionPressed("ui_accept") && (_shootCooldownTimer == null || _shootCooldownTimer.IsStopped()))
        {
            ShootPlayerBullet();
            if (_shootCooldownTimer != null)
            {
                _shootCooldownTimer.Start();
            }
        }
    }
    
    private void ShootPlayerBullet()
    {
        // Get the soul position to shoot from
        Node soulNode = GetTree().GetFirstNodeInGroup("soul");
        if (soulNode == null || !(soulNode is Node2D soul))
        {
            return;
        }
        
        // Load player bullet scene (yellow bullet)
        PackedScene playerBulletScene = GD.Load<PackedScene>("uid://c53touampkpns");
        if (playerBulletScene == null)
        {
            GD.PrintErr("Shoot1: Failed to load player bullet scene!");
            return;
        }
        
        Node2D bullet = playerBulletScene.Instantiate<Node2D>();
        if (bullet == null)
        {
            GD.PrintErr("Shoot1: Failed to instantiate player bullet!");
            return;
        }
        
        // Shoot from soul position upward
        Transform2D transform = new Transform2D(0, soul.GlobalPosition);
        Global.Singleton.EmitSignal(Global.SignalName.AddBullet, bullet, transform);
        Global.Singleton.EmitSignal(Global.SignalName.PlayShootSound);
    }

    private void _OnSpawnTimerTimeout()
    {
        if (BulletScene == null)
        {
            GD.PrintErr("Shoot1: BulletScene is null!");
            return;
        }
        Node bullet = BulletScene.Instantiate();
        if (bullet == null)
        {
            GD.PrintErr("Shoot1: Failed to instantiate bullet!");
            return;
        }
        _pathFollow2D.ProgressRatio = GD.Randf();
        Transform2D transform = _pathFollow2D.GlobalTransform;
        if (bullet is Node2D node2D)
        {
            Global.Singleton.EmitSignal(Global.SignalName.AddBullet, node2D, transform);
        }
        else
        {
            // Fallback if not Node2D
            Global.Singleton.EmitSignal(Global.SignalName.AddBullet, bullet as Node2D, Transform2D.Identity);
            GD.PrintErr("Shoot1: Bullet is not Node2D!");
        }
        if (bullet is Bullet bulletInstance)
        {
            bulletInstance.Speed = 500;
        }
    }
}
