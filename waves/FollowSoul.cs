using Godot;

public partial class FollowSoul : Wave
{
    private Timer _timer;

    public override async void _Ready()
    {
        base._Ready();
        
        // Wait for soul to be created (Battle creates it after adding the wave and waiting 2 frames)
        // We need to wait more frames to ensure the soul is created
        for (int i = 0; i < 5; i++)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Node soulNode = GetTree().GetFirstNodeInGroup("soul");
            if (soulNode != null)
            {
                // Soul found! Spawn the bullet
                Node bullet = BulletScene.Instantiate();
                if (bullet == null)
                {
                    GD.PrintErr("FollowSoul: Failed to instantiate bullet!");
                    break;
                }
                
                if (bullet is Node2D node2D && soulNode is Node2D soul)
                {
                    // Match GDScript: soul.global_position + Vector2(0, 100)
                    // Spawn bullet 100 pixels below the soul (in the battle box)
                    Vector2 position = soul.GlobalPosition + new Vector2(0, 100);
                    Transform2D transform = new Transform2D(0, position);
                    GD.Print($"FollowSoul: Spawning bullet at position {position} (soul at {soul.GlobalPosition}, offset: 100 down)");
                    Global.Singleton.EmitSignal(Global.SignalName.AddBullet, node2D, transform);
                }
                else
                {
                    GD.PrintErr($"FollowSoul: Bullet is not Node2D or soul is not Node2D. Bullet type: {bullet.GetType()}, Soul type: {soulNode.GetType()}");
                }
                break;
            }
        }
        
        // Final check if soul still not found
        Node finalSoulCheck = GetTree().GetFirstNodeInGroup("soul");
        if (finalSoulCheck == null)
        {
            GD.PrintErr("FollowSoul: Soul not found in scene tree after waiting 5 frames!");
        }
        
        _timer = GetNode<Timer>("EndTimer");
        _timer.Timeout += _OnTimerTimeout;
    }

    private void _OnTimerTimeout()
    {
        Node soulNode = GetTree().GetFirstNodeInGroup("soul");
        if (soulNode is Soul soul)
        {
            Global.Singleton.EmitSignal(Global.SignalName.WaveDone, this, soul);
        }
    }
}
