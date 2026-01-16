using Godot;
using Godot.Collections;

public partial class Enemy : Node
{
    [ExportCategory("Enemy data")]
    [Export]
    public string EnemyName { get; set; } = "Enemy name here";
    
    [Export]
    public int HP { get; set; } = 100;
    
    [Export]
    public Texture2D Sprite { get; set; }
    
    [Export]
    public float SpriteScale { get; set; } = 1.0f;
    
    [Export]
    public Array<string> Acts { get; set; } = new Array<string>();
    
    [Export]
    public Array<PackedScene> BulletWaves { get; set; } = new Array<PackedScene>();
    
    [Export(PropertyHint.MultilineText)]
    public string EncounterText { get; set; } = "* name here drew new!";

    public virtual string DoActGetText(string actName)
    {
        return "";
    }

    public virtual string GetIdleText()
    {
        return "";
    }

    public virtual string GetMonsterText()
    {
        return "";
    }
}
