[System.Serializable]
public class Net_CreateAccount : NetMsg
{
    //data bytes 3 strings
    public Net_CreateAccount()
    {
        OP = NetOP.CreateAccount;
    }
    public string Password { get; set; }
    public string Email { get; set; }
}