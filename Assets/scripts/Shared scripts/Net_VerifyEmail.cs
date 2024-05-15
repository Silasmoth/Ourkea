
[System.Serializable]
public class Net_VerifyEmail : NetMsg
{
    //data bytes 32 + string
    public Net_VerifyEmail()
    {
        OP = NetOP.VerifyEmail;
    }
    public int Code { get; set; }
    public string email { get; set; }
    public string HashedPass { get; set; }
    
}
