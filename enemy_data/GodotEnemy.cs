using Godot;

public partial class GodotEnemy : Enemy
{
    private int _chatCounter = 0;
    private bool _justInsulted = false;

    public override string DoActGetText(string act)
    {
        if (act == "Chat")
        {
            if (_chatCounter == 0)
            {
                _chatCounter += 1;
                Global.Singleton.EmitSignal(Global.SignalName.ChangeMercy, 50);
                return "* You talked to Godot about GDscript...\n* It seemed interested!";
            }
            else if (_chatCounter == 1)
            {
                _chatCounter += 1;
                Global.Singleton.EmitSignal(Global.SignalName.ChangeMercy, 50);
                return "* You listed the data types of GDscript!\n* int, float, String...\n* Godot was very proud of you!";
            }
            else
            {
                return "* That's enough talking!";
            }
        }
        else if (act == "Insult")
        {
            _justInsulted = true;
            return "* You told Godot that GDscript is slow...\n* Godot got angry!";
        }
        else if (act == "Check")
        {
            return "* Godot - ATK 10 DEF 5\n* A Robot programmed in C++\n* He really likes talking";
        }
        else
        {
            return "There was an error!!!";
        }
    }

    public override string GetIdleText()
    {
        if (_chatCounter == 0)
        {
            return "* Godot is _processing() what just happened";
        }
        if (_chatCounter == 1)
        {
            return "* Godot is eager to hear more about GDscript";
        }
        return "* Godot is very happy!";
    }

    public override string GetMonsterText()
    {
        if (_justInsulted)
        {
            _justInsulted = false;
            return Util.Shake("How dare you insult the best language!");
        }
        if (_chatCounter == 1)
        {
            return "Not bad!";
        }
        else if (_chatCounter == 2)
        {
            return Util.Wave("You are a true Godot enthusiast!");
        }
        return Util.Shake("I will crush you to BITs!");
    }
}
