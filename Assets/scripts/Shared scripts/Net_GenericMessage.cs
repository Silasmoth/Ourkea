﻿
[System.Serializable]
public class Net_GenerigMessage : NetMsg
{
    //data bytes 32 + string
    public Net_GenerigMessage()
    {
        OP = NetOP.GenericMessage;
    }

    
    public byte MessageId { get; set; }

    //From Server
    //0 - Account creation, an account with that email already exists
    //2 - Account creation, verification email sent
    //3 - Account creation, verificatoin email failed to send
    //4 - Account login, email does not exist
    //5 - account login, incorrect password
    //6 - Email Verificatoin, wrong code
    //7 - Email Verification, email resent
    //8 - Account Login, someone else logged in to the same account so you have been logged off
    //9 - Server Shutdown
  

}
