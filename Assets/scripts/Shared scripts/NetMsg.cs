﻿public static class NetOP
{
    public const byte None = 0;
    public const byte UserInfo = 1;
    public const byte GenericMessage = 2;
    public const byte CreateAccount = 3;
    public const byte LoginAtempt = 4;
    public const byte VerifyEmail = 5;
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
