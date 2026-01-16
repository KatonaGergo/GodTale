using Godot;

public static class Util
{
    public static string Tornado(string text, float radius = 10.0f, float freq = 3.0f)
    {
        return $"[tornado radius={radius} freq={freq}]{text}[/tornado]";
    }

    public static string Shake(string text, float rate = 20.0f, float level = 5.0f)
    {
        return $"[shake rate={rate} level={level}]{text}[/shake]";
    }

    public static string Wave(string text, float amp = 100.0f, float freq = 10.0f)
    {
        return $"[wave amp={amp} freq={freq}]{text}[/wave]";
    }
}
