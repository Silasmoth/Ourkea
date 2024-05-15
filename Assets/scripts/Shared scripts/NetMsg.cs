public static class NetOP
{
    public const byte None = 0;
    public const byte UserInfo = 1;

}

[System.Serializable]
public abstract class NetMsg
{
    public byte OP { set; get; }
    public NetMsg()
    {
        OP = NetOP.None;
    }
}
