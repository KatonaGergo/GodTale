using Godot;

public partial class FollowerBullet : Bullet
{
    public override void _Ready()
    {
        base._Ready();
        
        // Follower bullets should NOT be destroyed on hit - they keep following
        FreedOnHit = false;
        
        // Ensure speed is loaded from scene file (default is 100 in follower_bullet.tscn)
        // If Speed is still the default 500, try loading from scene file
        if (Speed >= 500.0f)
        {
            Variant speedVariant = Get("speed");
            if (speedVariant.VariantType == Variant.Type.Float)
            {
                Speed = speedVariant.AsSingle();
                GD.Print($"FollowerBullet: Loaded speed from scene file: {Speed}");
            }
        }
        // Lower the speed if it's too high (follower bullets should be slower)
        if (Speed > 150.0f)
        {
            Speed = 100.0f; // Match the scene file default
            GD.Print($"FollowerBullet: Speed was too high, set to 100");
        }
        
        // Also ensure FreedOnHit is loaded from scene file if it wasn't set correctly
        Variant freedOnHitVariant = Get("freed_on_hit");
        if (freedOnHitVariant.VariantType == Variant.Type.Bool)
        {
            FreedOnHit = freedOnHitVariant.AsBool();
            GD.Print($"FollowerBullet: Loaded freed_on_hit from scene file: {FreedOnHit}");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Soul soul = GetTree().GetFirstNodeInGroup("soul") as Soul;
        if (soul == null || !IsInstanceValid(soul))
        {
            return;
        }
        Vector2 dir = GlobalPosition.DirectionTo(soul.GlobalPosition);
        GlobalPosition += (float)delta * dir * Speed;
    }
}
