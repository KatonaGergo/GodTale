using Godot;

public partial class Shoot3 : Wave
{
    private PackedScene _whiteBulletScene = GD.Load<PackedScene>("uid://vy7eelm7fbgd");
    
    private int _counter = 0;
    private Timer _spawnTimer;
    private Timer _shootCooldownTimer;

    public override void _Ready()
    {
        base._Ready();
        _spawnTimer = GetNodeOrNull<Timer>("SpawnTimer");
        if (_spawnTimer != null)
        {
            _spawnTimer.Timeout += _OnSpawnTimerTimeout;
            // Ensure timer is not one-shot and starts (autostart might not work in C#)
            _spawnTimer.OneShot = false;
            _spawnTimer.Start();
        }
        
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
            GD.PrintErr("Shoot3: Failed to load player bullet scene!");
            return;
        }
        
        Node2D bullet = playerBulletScene.Instantiate<Node2D>();
        if (bullet == null)
        {
            GD.PrintErr("Shoot3: Failed to instantiate player bullet!");
            return;
        }
        
        // Shoot from soul position upward
        Transform2D transform = new Transform2D(0, soul.GlobalPosition);
        Global.Singleton.EmitSignal(Global.SignalName.AddBullet, bullet, transform);
        Global.Singleton.EmitSignal(Global.SignalName.PlayShootSound);
    }
    
    private void _OnSpawnTimerTimeout()
    {
        float center = GetViewport().GetVisibleRect().Size.X / 2;
        float centerOffset = 20;
        _counter += 1;
        bool even = _counter % 2 == 0;
        
        LinearBullet blueBullet = ((LinearBullet)BulletScene.Instantiate()).New(Mathf.Pi);
        blueBullet.GlobalPosition = new Vector2(center + (even ? centerOffset : -centerOffset), 0);
        Global.Singleton.EmitSignal(Global.SignalName.AddBullet, blueBullet, blueBullet.GlobalTransform);
        
        LinearBullet whiteBullet = ((LinearBullet)_whiteBulletScene.Instantiate()).New(0);
        whiteBullet.GlobalPosition = new Vector2(center + (even ? -centerOffset : centerOffset), 0);
        Global.Singleton.EmitSignal(Global.SignalName.AddBullet, whiteBullet, whiteBullet.GlobalTransform);
    }
}
