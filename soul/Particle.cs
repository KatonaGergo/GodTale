using Godot;

public partial class Particle : CpuParticles2D
{
    public override void _Ready()
    {
        // Ensure particles are visible and colored correctly
        // The color should be set by the parent (Battle.cs) before _Ready is called
        // But we can ensure it's visible
        Show();
        Emitting = true;
        // Connect the finished signal manually (scene file connection may not work in C#)
        Finished += _OnFinished;
        GD.Print($"Particle._Ready: Color = {Color}, Modulate = {Modulate}, Emitting = {Emitting}");
    }

    private void _OnFinished()
    {
        QueueFree();
    }
}
