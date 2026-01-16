using Godot;

public partial class Jump : Wave
{
    private Timer _spawnTimer;

    public override void _Ready()
    {
        base._Ready();
        // Connect and start the spawn timer for repeated spawning
        _spawnTimer = GetNode<Timer>("SpawnTimer");
        _spawnTimer.Timeout += OnSpawnTimerTimeout;
        _spawnTimer.OneShot = false;
        _spawnTimer.Start();
        
        // Spawn obstacle immediately (match GDScript: _on_spawn_timer_timeout() called in _ready)
        OnSpawnTimerTimeout();
    }

    protected override void OnSpawnTimerTimeout()
    {
        if (BulletScene != null)
        {
            JumpObstacle bullet = BulletScene.Instantiate<JumpObstacle>();
            if (bullet != null)
            {
                // Match GDScript: DisplayServer.screen_get_size().x, JumpObstacle.battle_box_bottom
                // But ensure scale is preserved - check scale after instantiation
                Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
                Vector2 position = new Vector2(viewportSize.X, JumpObstacle.BattleBoxBottom);
                Transform2D transform = new Transform2D(0, position);
                GD.Print($"Jump: Spawning obstacle - scale before: {bullet.Scale}, position: {position}");
                Global.Singleton.EmitSignal(Global.SignalName.AddBullet, bullet, transform);
            }
        }
    }
}
