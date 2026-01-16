using Godot;

public partial class Cherry : Enemy
{
    private bool _cheered = false;
    private bool _talkedFootball = false;
    private bool _justCheered = false;
    private bool _justTalkedFootball = false;
    private int _i = -1;

    public override string DoActGetText(string act)
    {
        if (act == "Cheer" && !_cheered)
        {
            _cheered = true;
            _justCheered = true;
            Global.Singleton.EmitSignal(Global.SignalName.ChangeMercy, 50);
            return "* You started cheering with Cherry...\n* She got more excited!";
        }
        else if (act == "Cheer" && _cheered)
        {
            return "* You cheered again with Cherry...\n* She didn't show much interest";
        }
        else if (act == "Football" && !_talkedFootball)
        {
            _justTalkedFootball = true;
            _talkedFootball = true;
            Global.Singleton.EmitSignal(Global.SignalName.ChangeMercy, 50);
            return "* You talked about human football...\n* Cherry started foaming from the mouth!";
        }
        else if (act == "Football" && _talkedFootball)
        {
            return "* You talked about human football again...\n* Cherry didn't care very much";
        }
        else if (act == "Check")
        {
            return "* Cherry - ATK 15 DEF 11\n* Her helmet is too big for her...\n* She doesn't care";
        }
        else
        {
            return "There was an error !!!";
        }
    }

    public override string GetIdleText()
    {
        if (_talkedFootball && _cheered)
        {
            return "* Cherry considers you her BFF now!";
        }
        else if (_cheered)
        {
            return "* Cherry wants to chat!";
        }
        else if (_talkedFootball)
        {
            return "* Cherry wants to see you cheer!";
        }
        return "* Cherry is jumping up and down!";
    }

    public override string GetMonsterText()
    {
        if (_justCheered)
        {
            _justCheered = false;
            return Util.Wave("I love your moves <3");
        }
        else if (_justTalkedFootball)
        {
            _justTalkedFootball = false;
            return Util.Wave("OMG you know that team too <3");
        }
        _i += 1;
        return _i % 2 == 0 ? Util.Wave("Go Go Hotland lizards <3") : Util.Wave("Cheerleading is a lifestyle <3");
    }
}
