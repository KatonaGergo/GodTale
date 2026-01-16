using Godot;
using Godot.Collections;

public partial class ShopUiOverlay : Control
{
    // Shop items configuration: [Item resource, EXP cost]
    private Array<Dictionary> _shopItems = new Array<Dictionary>
    {
        new Dictionary { {"item", GD.Load<Item>("res://items/apple.tres")}, {"cost", 10} },
        new Dictionary { {"item", GD.Load<Item>("res://items/nice_cream.tres")}, {"cost", 15} },
        new Dictionary { {"item", GD.Load<Item>("res://items/pie.tres")}, {"cost", 20} }
    };

    private PackedScene _button = GD.Load<PackedScene>("uid://ptt71q0lsxgx");

    private Label _expLabel;
    private VBoxContainer _itemsContainer;
    private TextBox _textBox;
    private Godot.Button _exitButton;

    private bool _isVisible = false;

    public override void _Ready()
    {
        Visible = false;
        _expLabel = GetNode<Label>("%ExpLabel");
        _itemsContainer = GetNode<VBoxContainer>("%ItemsContainer");
        _textBox = GetNode<TextBox>("%TextBox");
        _exitButton = GetNode<Godot.Button>("%ExitButton");
        UpdateExpDisplay();
        PopulateShop();
    }

    public void ShowShop()
    {
        if (_isVisible)
        {
            return;
        }
        _isVisible = true;
        Visible = true;
        UpdateExpDisplay();
        _exitButton.GrabFocus();
    }

    public void HideShop()
    {
        if (!_isVisible)
        {
            return;
        }
        _isVisible = false;
        Visible = false;
        _textBox.ClearText();
    }

    private void UpdateExpDisplay()
    {
        _expLabel.Text = $"EXP: {Global.Singleton.playerExp}";
    }

    private void PopulateShop()
    {
        // Clear existing items
        foreach (Node child in _itemsContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        // Add shop items
        foreach (Dictionary shopItem in _shopItems)
        {
            Item item = (Item)shopItem["item"];
            int cost = (int)shopItem["cost"];
            
            Godot.Button itemButton = (Godot.Button)_button.Instantiate();
            string itemText = $"{item.ItemName} - {cost} EXP";
            Label label = itemButton.GetNode<Label>("text");
            label.Text = Util.Shake(itemText);
            itemButton.FocusExited += () =>
            {
                itemButton.Modulate = new Color(itemButton.Modulate.R, itemButton.Modulate.G, itemButton.Modulate.B, 0.5f);
            };
            itemButton.Pressed += () => BuyItem(item, cost);
            _itemsContainer.AddChild(itemButton);
        }
    }

    private async void BuyItem(Item item, int cost)
    {
        if (Global.Singleton.SpendExp(cost))
        {
            Global.Singleton.AddItemToInventory(item);
            UpdateExpDisplay();
            _textBox.Scroll($"* You bought {item.ItemName} for {cost} EXP!");
            await ToSignal(_textBox, TextBox.SignalName.FinishedScrolling);
            _textBox.ClearText();
        }
        else
        {
            _textBox.Scroll("* You don't have enough EXP!");
            await ToSignal(_textBox, TextBox.SignalName.FinishedScrolling);
            _textBox.ClearText();
        }
    }

    private void _OnExitButtonPressed()
    {
        HideShop();
    }
}
