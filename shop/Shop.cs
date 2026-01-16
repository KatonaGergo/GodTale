using Godot;
using Godot.Collections;

public partial class Shop : Node2D
{
    // Shop items configuration: [Item resource, EXP cost, Gold cost]
    private Array<Dictionary> _shopItems = new Array<Dictionary>();

    // Menu navigation variables
    private bool _disableMenu = false;
    private int _choice = 1;
    private int _choiceRtn = 0;
    private int _choiceSize = 2;  // Buy, Exit
    private string _choiceExtend = "";  // Current menu path
    private string _choiceExtendCopy = "";

    // Node references
    private Sprite2D _shopkeeper;
    private AnimationPlayer _shopkeeperAnim;
    private Panel _buyPanel;
    private Label _interPanelText;
    private Label _buyPanelText;
    private Label _infoPanelText;
    private Label _expLabel;
    private Label _goldLabel;
    private VBoxContainer _buyOptions;
    private Label _confirmPrompt;
    private TextureRect _option1Cursor;
    private TextureRect _option2Cursor;

    private TextureRect _currentChoiceCursor = null;
    private int _currentBuyChoice = 0;
    private Dictionary _currentItemData = new Dictionary();

    public override async void _Ready()
    {
        // Initialize shop items (match GDScript: preload at class level, but we load in _Ready for C#)
        // Load items and ensure item_name property is loaded from .tres files
        Item apple = GD.Load<Item>("res://items/apple.tres");
        if (apple != null)
        {
            // Manually load item_name from resource file (snake_case in .tres) - match GDScript: item.item_name
            Variant appleNameVariant = apple.Get("item_name");
            if (appleNameVariant.VariantType == Variant.Type.String)
            {
                apple.ItemName = appleNameVariant.AsString();
            }
            Dictionary appleItem = new Dictionary();
            appleItem["item"] = apple;
            appleItem["exp_cost"] = 10;
            appleItem["gold_cost"] = 5;
            _shopItems.Add(appleItem);
        }
        
        Item niceCream = GD.Load<Item>("res://items/nice_cream.tres");
        if (niceCream != null)
        {
            // Manually load item_name from resource file (snake_case in .tres) - match GDScript: item.item_name
            Variant niceCreamNameVariant = niceCream.Get("item_name");
            if (niceCreamNameVariant.VariantType == Variant.Type.String)
            {
                niceCream.ItemName = niceCreamNameVariant.AsString();
            }
            Dictionary niceCreamItem = new Dictionary();
            niceCreamItem["item"] = niceCream;
            niceCreamItem["exp_cost"] = 15;
            niceCreamItem["gold_cost"] = 8;
            _shopItems.Add(niceCreamItem);
        }
        
        Item pie = GD.Load<Item>("res://items/pie.tres");
        if (pie != null)
        {
            // Manually load item_name from resource file (snake_case in .tres) - match GDScript: item.item_name
            Variant pieNameVariant = pie.Get("item_name");
            if (pieNameVariant.VariantType == Variant.Type.String)
            {
                pie.ItemName = pieNameVariant.AsString();
            }
            Dictionary pieItem = new Dictionary();
            pieItem["item"] = pie;
            pieItem["exp_cost"] = 20;
            pieItem["gold_cost"] = 10;
            _shopItems.Add(pieItem);
        }
        
        // Get node references
        _shopkeeper = GetNode<Sprite2D>("%Shopkeeper");
        _shopkeeperAnim = _shopkeeper.GetNode<AnimationPlayer>("AnimationPlayer");
        _buyPanel = GetNode<Panel>("InterPanel");
        _interPanelText = GetNode<Label>("%InterPanelText");
        _buyPanelText = GetNode<Label>("%BuyPanelText");
        _infoPanelText = GetNode<Label>("%InfoPanelText");
        _expLabel = GetNode<Label>("%ExpLabel");
        _goldLabel = GetNode<Label>("%GoldLabel");
        _buyOptions = GetNode<VBoxContainer>("%buyOptions");
        _confirmPrompt = GetNode<Label>("%ConfirmPrompt");
        _option1Cursor = GetNode<TextureRect>("BuyPanel/Option 1/Cursor");
        _option2Cursor = GetNode<TextureRect>("BuyPanel/Option 2/Cursor");

        // Initialize shopkeeper animation
        _shopkeeperAnim.Play("Idle");

        // Set initial cursor
        _currentChoiceCursor = _option1Cursor;
        _currentChoiceCursor.Visible = true;
        _option2Cursor.Visible = false;

        // Initialize displays
        UpdateExpDisplay();
        UpdateGoldDisplay();

        // Show greeting
        ShowGreeting();

        // Wait for fade
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        FadeToBlack fade = GetNode<FadeToBlack>("/root/Fade");
        fade.FadeFromBlack();
    }

    private void UpdateExpDisplay()
    {
        if (_expLabel != null)
        {
            _expLabel.Text = $"EXP: {Global.Singleton.playerExp}";
        }
    }

    private void UpdateGoldDisplay()
    {
        if (_goldLabel != null)
        {
            _goldLabel.Text = $"Gold: {Global.Singleton.playerGold}";
        }
    }

    private void ShowGreeting()
    {
        _interPanelText.Text = "* Welcome to the shop!";
        _buyPanelText.Text = "";
        _buyPanelText.Visible = false;
    }

    public override void _Input(InputEvent ev)
    {
        if (_disableMenu)
        {
            return;
        }

        if (ev is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Up)
            {
                if (_choiceExtend == "")
                {
                    UpdateChoice("up", _choiceSize);
                }
                else if (_choiceExtend == "buyOptions/")
                {
                    UpdateBuyChoice("up");
                }
                else if (_choiceExtend == "ConfirmPrompt/")
                {
                    UpdateConfirmChoice("up");
                }
            }
            else if (keyEvent.Keycode == Key.Down)
            {
                if (_choiceExtend == "")
                {
                    UpdateChoice("down", _choiceSize);
                }
                else if (_choiceExtend == "buyOptions/")
                {
                    UpdateBuyChoice("down");
                }
                else if (_choiceExtend == "ConfirmPrompt/")
                {
                    UpdateConfirmChoice("down");
                }
            }
            else if (keyEvent.Keycode == Key.Enter || keyEvent.Keycode == Key.KpEnter)
            {
                HandleSelect();
            }
            else if (keyEvent.Keycode == Key.C || keyEvent.Keycode == Key.X || keyEvent.Keycode == Key.Backspace)
            {
                HandleBack();
            }
        }
    }

    private void UpdateChoice(string direction, int maxOptions)
    {
        if (_currentChoiceCursor != null)
        {
            _currentChoiceCursor.Visible = false;
        }

        if (direction == "up")
        {
            if (_choice == 1)
            {
                _choice = maxOptions;
            }
            else
            {
                _choice -= 1;
            }
        }
        else if (direction == "down")
        {
            if (_choice == maxOptions)
            {
                _choice = 1;
            }
            else
            {
                _choice += 1;
            }
        }

        // Update cursor
        if (_choice == 1)
        {
            _currentChoiceCursor = _option1Cursor;
            _option2Cursor.Visible = false;
        }
        else
        {
            _currentChoiceCursor = _option2Cursor;
            _option1Cursor.Visible = false;
        }

        if (_currentChoiceCursor != null)
        {
            _currentChoiceCursor.Visible = true;
        }
    }

    private void UpdateBuyChoice(string direction)
    {
        var options = _buyOptions.GetChildren();
        if (options.Count == 0)
        {
            return;
        }

        // Hide current cursor
        if (_currentBuyChoice >= 0 && _currentBuyChoice < options.Count)
        {
            Node currentOption = _buyOptions.GetChild(_currentBuyChoice);
            TextureRect currentCursor = currentOption.GetNodeOrNull<TextureRect>("Cursor");
            if (currentCursor != null)
            {
                currentCursor.Visible = false;
            }
        }

        if (direction == "up")
        {
            if (_currentBuyChoice == 0)
            {
                _currentBuyChoice = options.Count - 1;
            }
            else
            {
                _currentBuyChoice -= 1;
            }
        }
        else if (direction == "down")
        {
            if (_currentBuyChoice == options.Count - 1)
            {
                _currentBuyChoice = 0;
            }
            else
            {
                _currentBuyChoice += 1;
            }
        }
        else
        {
            // Initial call - just show cursor
            if (_currentBuyChoice >= options.Count)
            {
                _currentBuyChoice = 0;
            }
        }

        // Show new cursor
        if (_currentBuyChoice >= 0 && _currentBuyChoice < options.Count)
        {
            Node newOption = _buyOptions.GetChild(_currentBuyChoice);
            TextureRect newCursor = newOption.GetNodeOrNull<TextureRect>("Cursor");
            if (newCursor != null)
            {
                newCursor.Visible = true;
            }

            // Update info panel
            UpdateInfoPanel();
        }
    }

    private void HandleSelect()
    {
        if (_choiceExtend == "")
        {
            // Main menu
            if (_choice == 1)  // Buy
            {
                EnterBuyMenu();
            }
            else if (_choice == 2)  // Exit
            {
                ExitShop();
            }
        }
        else if (_choiceExtend == "buyOptions/")
        {
            // Buy menu - show confirm prompt
            ShowConfirmPrompt();
        }
        else if (_choiceExtend == "ConfirmPrompt/")
        {
            // Confirm prompt
            HandleConfirm();
        }
    }

    private void HandleBack()
    {
        if (_choiceExtend == "buyOptions/")
        {
            // Exit buy menu
            ExitBuyMenu();
        }
        else if (_choiceExtend == "ConfirmPrompt/")
        {
            // Exit confirm prompt
            ExitConfirmPrompt();
        }
    }

    private void EnterBuyMenu()
    {
        _choiceExtend = "buyOptions/";
        _choiceExtendCopy = "buyOptions/";

        // Hide main menu options
        if (GetNode("BuyPanel/Option 1") is CanvasItem option1)
        {
            option1.Visible = false;
        }
        if (GetNode("BuyPanel/Option 2") is CanvasItem option2)
        {
            option2.Visible = false;
        }

        // Hide InterPanelText to prevent overlap with buy options
        _interPanelText.Visible = false;

        // Show "What would you like?" text when viewing products
        _buyPanelText.Visible = true;
        _buyPanelText.Text = "What would\nyou like?";

        // Show buy options
        _buyOptions.Visible = true;
        PopulateBuyOptions();

        // Show info panel
        Node infoPanel = GetNodeOrNull("BuyPanel/InfoPanel");
        if (infoPanel is CanvasItem infoPanelCanvas)
        {
            infoPanelCanvas.Visible = true;
        }

        // Set first item as selected
        _currentBuyChoice = 0;
        UpdateBuyChoice("");
    }

    private void ExitBuyMenu()
    {
        _choiceExtend = "";
        _choiceExtendCopy = "";

        // Hide buy options
        _buyOptions.Visible = false;
        Node infoPanel = GetNodeOrNull("BuyPanel/InfoPanel");
        if (infoPanel is CanvasItem infoPanelCanvas)
        {
            infoPanelCanvas.Visible = false;
        }

        // Show main menu options
        if (GetNode("BuyPanel/Option 1") is CanvasItem option1)
        {
            option1.Visible = true;
        }
        if (GetNode("BuyPanel/Option 2") is CanvasItem option2)
        {
            option2.Visible = true;
        }

        // Show InterPanelText again
        _interPanelText.Visible = true;

        // Hide "What would you like?" when back at main menu
        _buyPanelText.Visible = false;

        // Reset choice
        _choice = 1;
        UpdateChoice("", _choiceSize);

        // Update text
        _interPanelText.Text = "* Go on";
    }

    private void PopulateBuyOptions()
    {
        // Clear existing options
        foreach (Node child in _buyOptions.GetChildren())
        {
            child.QueueFree();
        }

        // Create menu entries for each shop item
        for (int i = 0; i < _shopItems.Count; i++)
        {
            Dictionary shopItem = _shopItems[i];
            Item item = shopItem["item"].AsGodotObject() as Item;
            int expCost = shopItem["exp_cost"].AsInt32();
            int goldCost = shopItem["gold_cost"].AsInt32();

            // Get item name - match GDScript: item.item_name
            // Since we loaded item_name in _Ready(), ItemName should be set, but try Get() as fallback
            string itemName = item.ItemName;
            if (string.IsNullOrEmpty(itemName) || itemName == "Item name")
            {
                Variant itemNameVariant = item.Get("item_name");
                if (itemNameVariant.VariantType == Variant.Type.String)
                {
                    itemName = itemNameVariant.AsString();
                    item.ItemName = itemName; // Update property for future use
                }
            }

            // Create option label
            Label optionLabel = new Label();
            optionLabel.Name = $"Option {i + 1}";
            optionLabel.Text = $"{expCost} EXP, {goldCost} Gold - {itemName}";
            optionLabel.CustomMinimumSize = new Vector2(0, 60);
            optionLabel.AddThemeFontSizeOverride("font_size", 27);
            optionLabel.VerticalAlignment = VerticalAlignment.Center;

            // Create cursor
            TextureRect cursor = new TextureRect();
            cursor.Name = "Cursor";
            cursor.Texture = GD.Load<Texture2D>("res://sprites/red_soul.svg.png");
            cursor.Scale = new Vector2(0.18868f, 0.18868f);
            cursor.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
            cursor.Visible = (i == 0);  // Only first one visible
            cursor.OffsetLeft = -50.0f;
            cursor.OffsetTop = 20.0f;
            cursor.OffsetRight = 144.0f;
            cursor.OffsetBottom = 214.0f;

            optionLabel.AddChild(cursor);
            _buyOptions.AddChild(optionLabel);
        }
    }

    private void UpdateInfoPanel()
    {
        if (_currentBuyChoice >= 0 && _currentBuyChoice < _shopItems.Count)
        {
            // Clear info panel - all info is already in the buy options list
            _infoPanelText.Text = "";
        }
    }

    private void ShowConfirmPrompt()
    {
        if (_currentBuyChoice >= _shopItems.Count)
        {
            return;
        }

        Dictionary shopItem = _shopItems[_currentBuyChoice];
        Item item = shopItem["item"].AsGodotObject() as Item;
        int expCost = shopItem["exp_cost"].AsInt32();
        int goldCost = shopItem["gold_cost"].AsInt32();

        _currentItemData = shopItem;
        _choiceRtn = _currentBuyChoice + 1;

        // Hide "What would you like?" text
        _buyPanelText.Visible = false;

        // Update confirm prompt text
        _confirmPrompt.Text = $"Buy it for\n{expCost} EXP, {goldCost} Gold?";
        _confirmPrompt.Visible = true;

        // Hide buy options
        _buyOptions.Visible = false;

        // Set choice to confirm menu
        _choiceExtend = "ConfirmPrompt/";
        _choice = 1;

        // Show cursor on Yes
        TextureRect yesCursor = GetNode<TextureRect>("BuyPanel/ConfirmPrompt/Option 1/Cursor");
        TextureRect noCursor = GetNode<TextureRect>("BuyPanel/ConfirmPrompt/Option 2/Cursor");
        yesCursor.Visible = true;
        noCursor.Visible = false;
    }

    private void UpdateConfirmChoice(string direction)
    {
        TextureRect yesCursor = GetNode<TextureRect>("BuyPanel/ConfirmPrompt/Option 1/Cursor");
        TextureRect noCursor = GetNode<TextureRect>("BuyPanel/ConfirmPrompt/Option 2/Cursor");

        if (direction == "up")
        {
            if (_choice == 1)
            {
                _choice = 2;
            }
            else
            {
                _choice = 1;
            }
        }
        else if (direction == "down")
        {
            if (_choice == 2)
            {
                _choice = 1;
            }
            else
            {
                _choice = 2;
            }
        }

        if (_choice == 1)
        {
            yesCursor.Visible = true;
            noCursor.Visible = false;
        }
        else
        {
            yesCursor.Visible = false;
            noCursor.Visible = true;
        }
    }

    private void ExitConfirmPrompt()
    {
        _confirmPrompt.Visible = false;
        _buyOptions.Visible = true;
        // Show "What would you like?" when returning to buy menu
        _buyPanelText.Visible = true;
        _buyPanelText.Text = "What would\nyou like?";
        _choiceExtend = "buyOptions/";
        _currentBuyChoice = _choiceRtn - 1;
        _choiceRtn = 0;
        UpdateBuyChoice("");
    }

    private async void HandleConfirm()
    {
        if (_choice == 1)  // Yes
        {
            // Attempt purchase
            Item item = _currentItemData["item"].AsGodotObject() as Item;
            int expCost = _currentItemData["exp_cost"].AsInt32();
            int goldCost = _currentItemData["gold_cost"].AsInt32();

            bool hasEnoughExp = Global.Singleton.playerExp >= expCost;
            bool hasEnoughGold = Global.Singleton.playerGold >= goldCost;

            if (hasEnoughExp && hasEnoughGold)
            {
                // Purchase successful
                Global.Singleton.SpendExp(expCost);
                Global.Singleton.SpendGold(goldCost);
                Global.Singleton.AddItemToInventory(item);
                UpdateExpDisplay();
                UpdateGoldDisplay();

                // Play shopkeeper animation
                _shopkeeperAnim.Play("Speaking");
                await ToSignal(GetTree().CreateTimer(0.4), Timer.SignalName.Timeout);
                _shopkeeperAnim.Play("Idle");

                // Get item name - match GDScript: item.item_name
                // Since we loaded item_name in _Ready(), ItemName should be set, but try Get() as fallback
                string itemName = item.ItemName;
                if (string.IsNullOrEmpty(itemName) || itemName == "Item name")
                {
                    Variant itemNameVariant = item.Get("item_name");
                    if (itemNameVariant.VariantType == Variant.Type.String)
                    {
                        itemName = itemNameVariant.AsString();
                        item.ItemName = itemName; // Update property for future use
                    }
                }

                // Show success message - hide buy options temporarily to show message
                _buyOptions.Visible = false;
                _interPanelText.Visible = true;
                _interPanelText.Text = $"* You bought {itemName}!";
                _buyPanelText.Text = "Come\nagain!";
            }
            else
            {
                // Not enough currency - hide buy options temporarily to show error
                _shopkeeperAnim.Play("Speaking");
                await ToSignal(GetTree().CreateTimer(0.4), Timer.SignalName.Timeout);
                _shopkeeperAnim.Play("Idle");
                _buyOptions.Visible = false;
                _interPanelText.Visible = true;
                if (!hasEnoughExp && !hasEnoughGold)
                {
                    _interPanelText.Text = "* You don't have enough EXP and Gold!";
                }
                else if (!hasEnoughExp)
                {
                    _interPanelText.Text = "* You don't have enough EXP!";
                }
                else
                {
                    _interPanelText.Text = "* You don't have enough Gold!";
                }
                _buyPanelText.Text = "That's not\nenough\nmoney.";
            }
        }

        // Return to buy menu after showing message
        await ToSignal(GetTree().CreateTimer(1.5), Timer.SignalName.Timeout);
        _interPanelText.Visible = false;
        _buyOptions.Visible = true;
        ExitConfirmPrompt();
    }

    private async void ExitShop()
    {
        FadeToBlack fade = GetNode<FadeToBlack>("/root/Fade");
        await fade.FadeIntoBlack();
        if (IsInsideTree() && GetTree() != null)
        {
            if (Global.Singleton.lastScenePath != "")
            {
                GetTree().ChangeSceneToFile(Global.Singleton.lastScenePath);
            }
            else
            {
                GetTree().ChangeSceneToFile("res://maps/overworld_original.tscn");
            }
        }
    }
}
