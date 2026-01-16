using Godot;

public partial class DiamondsFromBelow : Wave
{
    private PathFollow2D _pathFollow2D;
    private Timer _spawnTimer;

    public override void _Ready()
    {
        base._Ready();
        // Right Below the battle box:
        GlobalPosition = new Vector2(656, GetViewport().GetVisibleRect().Size.Y);
        _pathFollow2D = GetNode<PathFollow2D>("%PathFollow2D");
        _spawnTimer = GetNode<Timer>("SpawnTimer");
        _spawnTimer.Timeout += _OnSpawnTimerTimeout;
    }

    private void _OnSpawnTimerTimeout()
    {
        Node bullet = BulletScene.Instantiate();
        _pathFollow2D.ProgressRatio = GD.Randf();
        Transform2D transform = _pathFollow2D.GlobalTransform;
        if (bullet is Node2D node2D)
        {
            Global.Singleton.EmitSignal(Global.SignalName.AddBullet, node2D, transform);
            // Rotate the bullet after it's added to scene tree (matching GDScript: bullet.rotation_degrees += 180)
            // Use CallDeferred to ensure it happens after the transform is set in the signal handler
            node2D.CallDeferred("set", "rotation_degrees", node2D.RotationDegrees + 180);
        }
        else
        {
            // Fallback if not Node2D
            Global.Singleton.EmitSignal(Global.SignalName.AddBullet, bullet as Node2D, transform);
            if (bullet is Node2D node2DFallback)
            {
                node2DFallback.CallDeferred("set", "rotation_degrees", node2DFallback.RotationDegrees + 180);
            }
        }
    }

}
