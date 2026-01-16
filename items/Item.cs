using Godot;

public partial class Item : Resource
{
    [Export]
    public string ItemName { get; set; } = "Item name";
    
    [Export]
    public int Amount { get; set; } = 10;
    
    [Export(PropertyHint.MultilineText)]
    public string Text { get; set; } = "* You took a big bite out of the item...\n  it tasted good !\n";
}
