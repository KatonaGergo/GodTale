using Godot;

public partial class Shoot2 : Wave
{
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
            GD.PrintErr("Shoot2: Failed to load player bullet scene!");
            return;
        }
        
        Node2D bullet = playerBulletScene.Instantiate<Node2D>();
        if (bullet == null)
        {
            GD.PrintErr("Shoot2: Failed to instantiate player bullet!");
            return;
        }
        
        // Shoot from soul position upward
        Transform2D transform = new Transform2D(0, soul.GlobalPosition);
        Global.Singleton.EmitSignal(Global.SignalName.AddBullet, bullet, transform);
        Global.Singleton.EmitSignal(Global.SignalName.PlayShootSound);
    }

    private void _OnSpawnTimerTimeout()
    {
        Bullet instance = BulletScene.Instantiate() as Bullet;
        Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
        instance.GlobalPosition = new Vector2(viewportSize.X / 2, 0);
        instance.Rotate(Mathf.Pi);
        Global.Singleton.EmitSignal(Global.SignalName.AddBullet, instance, instance.GlobalTransform);
    }
}
