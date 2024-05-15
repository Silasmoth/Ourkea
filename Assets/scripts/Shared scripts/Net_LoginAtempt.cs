
[System.Serializable]
public class Net_LoginAtempt : NetMsg
{
    //data bytes 32 + string
    public Net_LoginAtempt()
    {
        OP = NetOP.LoginAtempt;
    }
    
    public string Email { get; set; }
    public string HashedPass { get; set; }

    public byte Extra { get; set; }
    //0 - Normal login attempt


}
