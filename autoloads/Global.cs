using Godot;
using Godot.Collections;

public partial class Global : Node
{
    public static Global Singleton { get; private set; }

    public override void _Ready()
    {
        Singleton = this;
    }

    [Signal]
    public delegate void WaveDoneEventHandler(Node2D waveScene, Soul soul);
    
    [Signal]
    public delegate void AddBulletEventHandler(Node2D bullet, Transform2D transform);
    
    [Signal]
    public delegate void ChangeMercyEventHandler(int amount);
    
    [Signal]
    public delegate void HealPlayerEventHandler(int amount);
    
    [Signal]
    public delegate void BulletDestroyedEventHandler(Vector2 pos);
    
    [Signal]
    public delegate void MonsterVisibleEventHandler(bool newVal);
    
    [Signal]
    public delegate void PlayShootSoundEventHandler();

    // Boss tracking and progression
    public Array<string> killedBosses { get; set; } = new Array<string>();
    public int playerExp { get; set; } = 0;
    public int playerGold { get; set; } = 0;
    public Array<Item> battleInventory { get; set; } = new Array<Item>();
    public bool cutscenePlayed { get; set; } = false;

    // Position tracking for battle return
    public string lastScenePath { get; set; } = "";
    public Vector2 lastPlayerPosition { get; set; } = Vector2.Zero;
    
    // Current boss name for battle (set by BattleZone, used by Battle to mark as killed)
    public string currentBossName { get; set; } = "";

    public static readonly Array<string> BOSS_NAMES = new Array<string> { "Cherry", "Poseur", "Present", "Godot" };

    public void MarkBossKilled(string bossName)
    {
        if (!IsBossKilled(bossName))
        {
            killedBosses.Add(bossName);
        }
    }

    public bool IsBossKilled(string bossName)
    {
        return killedBosses.Contains(bossName);
    }

    public void AddExp(int amount)
    {
        playerExp += amount;
    }

    public bool SpendExp(int amount)
    {
        if (playerExp >= amount)
        {
            playerExp -= amount;
            return true;
        }
        return false;
    }

    public bool SpendGold(int amount)
    {
        if (playerGold >= amount)
        {
            playerGold -= amount;
            return true;
        }
        return false;
    }

    public void AddItemToInventory(Item item)
    {
        battleInventory.Add(item);
    }

    public bool AllBossesKilled()
    {
        foreach (string bossName in BOSS_NAMES)
        {
            if (!IsBossKilled(bossName))
            {
                return false;
            }
        }
        return true;
    }
}
