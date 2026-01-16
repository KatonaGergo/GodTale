using Godot;

public partial class Present : Enemy
{
    private bool _justPeeked = false;
    private bool _spared = false;

    public override string DoActGetText(string act)
    {
        if (act == "Check")
        {
            return "* Present - ATK 9 DEF 8\n* A square box\n* Loves surprises";
        }
        else if (act == "Gift")
        {
            Global.Singleton.EmitSignal(Global.SignalName.HealPlayer, 10);
            return "* You begged for an early christmas gift...\n* Present gave you a cookie\n* (10 HP recovered!)";
        }
        else if (act == "Peek")
        {
            _justPeeked = true;
            return "* You tried to look inside Present...\n* It got very angry!";
        }
        else if (act == "Wait")
        {
            Global.Singleton.EmitSignal(Global.SignalName.ChangeMercy, 100);
            _spared = true;
            return "* You waited patiently...\n* Present seemed pleased!";
        }
        return "Error";
    }

    public override string GetIdleText()
    {
        if (_spared)
        {
            return "* Present is very proud of you";
        }
        return "* Present is giving you a funny look";
    }

    public override string GetMonsterText()
    {
        if (_justPeeked)
        {
            _justPeeked = false;
            return Util.Shake("WAIT UNTIL CHRISTMAS!");
        }
        else if (_spared)
        {
            return Util.Wave("You're going on the good list!");
        }
        return Util.Wave("No peeking till christmas!");
    }
}
