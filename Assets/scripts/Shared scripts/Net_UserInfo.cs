
[System.Serializable]
public class Net_UserInfo : NetMsg
{
    //data bytes 32 + string
    public Net_UserInfo()
    {
        OP = NetOP.UserInfo;
    }

    public bool client { get; set; } //if true this is a client connecting, if false this is a bulider connecting
   
}
