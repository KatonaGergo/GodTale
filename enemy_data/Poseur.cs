using Godot;

public partial class Poseur : Enemy
{
    private int _poseCounter = 0;

    public override string DoActGetText(string act)
    {
        if (act == "Pose")
        {
            if (_poseCounter == 0)
            {
                _poseCounter += 1;
                Global.Singleton.EmitSignal(Global.SignalName.ChangeMercy, 35);
                return "* You did a clumsy little pose...\n* Poseur seemed interested!";
            }
            else if (_poseCounter == 1)
            {
                _poseCounter += 1;
                Global.Singleton.EmitSignal(Global.SignalName.ChangeMercy, 35);
                return "* You did a decent pose...\n* it was better than the last one!\n* Poseur got more interested!";
            }
            else if (_poseCounter == 2)
            {
                _poseCounter += 1;
                Global.Singleton.EmitSignal(Global.SignalName.ChangeMercy, 35);
                return "* You posed really hard...\n* Poseur is in tears!";
            }
            else
            {
                return "* That's enough posing!";
            }
        }
        else if (act == "Check")
        {
            return "* Poseur - ATK 12 DEF 7\n* A featureless doll\n* Lives to pose";
        }
        else
        {
            return "There was an error !!!";
        }
    }

    public override string GetIdleText()
    {
        if (_poseCounter == 0)
        {
            return "* Poseur is posing really hard!";
        }
        else if (_poseCounter == 1)
        {
            return "* Poseur wants to see you pose more!";
        }
        else if (_poseCounter == 2)
        {
            return "* Poseur wants to see one last pose!";
        }
        return "* Poseur is satsfied!";
    }

    public override string GetMonsterText()
    {
        if (_poseCounter == 1)
        {
            return Util.Tornado("Nice pose!");
        }
        else if (_poseCounter == 2)
        {
            return Util.Tornado("Fabulous!");
        }
        else if (_poseCounter == 3)
        {
            return Util.Tornado("absolutely beautiful!");
        }
        return Util.Tornado("Let's dance darling!");
    }
}
