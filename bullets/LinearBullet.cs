using Godot;

public partial class LinearBullet : Bullet
{
    public override void _PhysicsProcess(double delta)
    {
        Position += (float)delta * Speed * GlobalTransform.Y;
    }

    public LinearBullet New(float rotateBy)
    {
        Rotate(rotateBy);
        return this;
    }
}
