
[System.Serializable]
public class Net_Ping : NetMsg
{

    public Net_Ping()
    {
        OP = NetOP.Ping;
    }

    //no need for any data this is just a ping pong to keep the connection from timing out
}
