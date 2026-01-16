using Godot;

public partial class Jump2 : Wave
{
    private Timer _spawnTimer;

    public override void _Ready()
    {
        base._Ready();
        // Connect and start the spawn timer for repeated spawning
        _spawnTimer = GetNode<Timer>("SpawnTimer");
        _spawnTimer.Timeout += _OnSpawnTimerTimeout;
        _spawnTimer.OneShot = false;
        _spawnTimer.Start();
        
        // Spawn obstacles immediately (match GDScript: _on_spawn_timer_timeout() called in _ready)
        _OnSpawnTimerTimeout();
    }

    private void _OnSpawnTimerTimeout()
    {
        if (BulletScene == null)
        {
            GD.PrintErr("Jump2: BulletScene is null!");
            return;
        }
        
        JumpObstacle bullet1 = BulletScene.Instantiate() as JumpObstacle;
        if (bullet1 != null)
        {
            bullet1.Dir = Vector2.Right;
            Vector2 position1 = new Vector2(0, JumpObstacle.BattleBoxBottom);
            Transform2D transform1 = new Transform2D(0, position1);
            GD.Print($"Jump2: Spawning obstacle 1 - scale before: {bullet1.Scale}, position: {position1}, dir: {bullet1.Dir}");
            Global.Singleton.EmitSignal(Global.SignalName.AddBullet, bullet1, transform1);
        }
        
        JumpObstacle bullet2 = BulletScene.Instantiate() as JumpObstacle;
        if (bullet2 != null)
        {
            bullet2.Dir = Vector2.Left;
            Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
            Vector2 position2 = new Vector2(viewportSize.X, JumpObstacle.BattleBoxBottom);
            Transform2D transform2 = new Transform2D(0, position2);
            GD.Print($"Jump2: Spawning obstacle 2 - scale before: {bullet2.Scale}, position: {position2}, dir: {bullet2.Dir}");
            Global.Singleton.EmitSignal(Global.SignalName.AddBullet, bullet2, transform2);
        }
    }

}
