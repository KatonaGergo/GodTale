using Godot;

public partial class JumpObstacle : Bullet
{
    [Export]
    public Vector2 Dir { get; set; } = Vector2.Left;

    public static int BattleBoxBottom { get; } = 751;

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += (float)delta * Speed * Dir;
    }
}
