using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Unity.Collections;
using System;

public class NetworkClient : MonoBehaviour
{
    public bool client =  true; //is this a client (true) or builder (false)
    public BlockPlacer blockPlacer;//reference to the blockplacer/scenemanager when in the tool scene
    public SceneMemBuilder builderscene;//reference for popups and stuff in builder main menu

    public bool connected = false;
    //local ip "127.0.0.1"

    public string SERVER_IP = "127.0.0.1";
    ushort PORT = 8999;
    const int BYTE_SIZE = 1024;
    //new networking code
    string SessionToken = "";

    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    NetworkPipeline m_PipelineFast;
    NetworkPipeline m_PipelineSlow;
    NetworkPipeline m_PipelineBig;
    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (connected)
        {
            UpdateMessagesNew();
        }
    }

    public void Init()
    {
        Debug.Log("Trying to connect to : " + SERVER_IP + ", Port : " + PORT);
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);
        _ = new NetworkEndPoint();
        NetworkEndPoint endpoint;
        NetworkEndPoint.TryParse(SERVER_IP, PORT, out endpoint, NetworkFamily.Ipv4);
        endpoint.Port = PORT;
        m_PipelineBig = m_Driver.CreatePipeline(typeof(FragmentationPipelineStage), typeof(ReliableSequencedPipelineStage));
        m_PipelineSlow = m_Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
        m_Connection = m_Driver.Connect(endpoint);
        connected = true;
    }

    public void UpdateMessagesNew()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {

            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = m_Connection.PopEvent(m_Driver, out stream, out _)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("We are now connected to the server");


               


            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                NativeArray<Byte> buffer = new NativeArray<byte>(stream.Length, Allocator.Temp);

                stream.ReadBytes(buffer);

                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(buffer.ToArray());
                buffer.Dispose();
                NetMsg msg = (NetMsg)formatter.Deserialize(ms);
                OnData(msg);


            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                m_Connection = default(NetworkConnection);
            }
        }
    }


    #region SendData
    public void SendServerFast( NetMsg msg)
    {
        //Ser_fastSends++;
        byte[] buffer = new byte[BYTE_SIZE];

        //this is where we crush data into byte array
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        try
        {
            formatter.Serialize(ms, msg);
        }
        catch
        {
            Debug.Log("message of type: " + msg.OP + "is too big");
        }

        NativeArray<Byte> package = new NativeArray<byte>(buffer, Allocator.Temp);
        m_Driver.BeginSend(NetworkPipeline.Null, m_Connection, out var writer);
        writer.WriteBytes(package);
        m_Driver.EndSend(writer);
        package.Dispose();
    }

    public void SendServerFast(NetworkConnection connection, NetMsg msg)
    {
        //Ser_fastSends++;
        byte[] buffer = new byte[BYTE_SIZE];

        //this is where we crush data into byte array
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, msg);
        NativeArray<Byte> package = new NativeArray<byte>(buffer, Allocator.Temp);
        m_Driver.BeginSend(NetworkPipeline.Null, connection, out var writer);
        writer.WriteBytes(package);
        m_Driver.EndSend(writer);
        package.Dispose();
    }

    public void SendServer(NetworkConnection connection, NetMsg msg)
    {

        byte[] buffer = new byte[BYTE_SIZE];

        //this is where we crush data into byte array
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, msg);
        NativeArray<Byte> package = new NativeArray<byte>(buffer, Allocator.Temp);
        m_Driver.BeginSend(m_PipelineSlow, connection, out var writer);
        writer.WriteBytes(package);
        m_Driver.EndSend(writer);
        package.Dispose();
    }
    public void SendServer( NetMsg msg)
    {
        //Ser_slowsends++;


        byte[] buffer = new byte[BYTE_SIZE];

        //this is where we crush data into byte array
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        try
        {
            formatter.Serialize(ms, msg);
        }
        catch
        {
            Debug.Log("message of type: " + msg.OP + "is too big");
        }
        var shortbuffer = new byte[ms.Position + 1];
        for (int i = 0; i < shortbuffer.Length; i++)
        {
            shortbuffer[i] = buffer[i];
        }
        NativeArray<Byte> package = new NativeArray<byte>(shortbuffer, Allocator.Temp);
        m_Driver.BeginSend(m_PipelineSlow, m_Connection, out var writer);
        writer.WriteBytes(package);
        m_Driver.EndSend(writer);
        package.Dispose();
        //Debug.Log("finished slow send to user " + connectionID);
    }


    public void SendServerBig(NetworkConnection connection, NetMsg msg)
    {

        byte[] buffer = new byte[BYTE_SIZE * 32];

        //this is where we crush data into byte array
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, msg);
        NativeArray<Byte> package = new NativeArray<byte>(buffer, Allocator.Temp);
        m_Driver.BeginSend(m_PipelineBig, connection, out var writer);
        writer.WriteBytes(package);
        m_Driver.EndSend(writer);
        package.Dispose();
    }
    public void SendServerBig(NetMsg msg)
    {
        byte[] buffer = new byte[BYTE_SIZE * 64];

        //big send caps out at like 65 kilobytes - might not be big enough
        //this is where we crush data into byte array
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, msg);
        var shortbuffer = new byte[ms.Position + 1];
        for (int i = 0; i < shortbuffer.Length; i++)
        {
            shortbuffer[i] = buffer[i];
        }
        NativeArray<Byte> package = new NativeArray<byte>(shortbuffer, Allocator.Temp);
        m_Driver.BeginSend(m_PipelineBig, m_Connection, out var writer);
        writer.WriteBytes(package);
        m_Driver.EndSend(writer);
        package.Dispose();
        
    }

    #endregion
    private void OnData(NetMsg msg)
    {
        //Debug.Log("Received a message of type "  + msg.OP);

        switch (msg.OP)
        {
            case NetOP.None:
                Debug.Log("Unexpected NET_OP");
                break;

            case NetOP.UserInfo:
                //we were connected to the server, they are requesting our user info
                SendUserInfo();
                break;

            case NetOP.GenericMessage:
                GenericMessage((Net_GenerigMessage)msg);
                break;

        }
    }

    void SendUserInfo()
    {
        Net_UserInfo msg = new Net_UserInfo();

        msg.client = client;

        SendServer(msg);
    }

    void GenericMessage(Net_GenerigMessage msg)
    {
        switch (msg.MessageId)
        {
            case (0):
                //this is sent to the builder because they are trying to make an account with an email that is already in use
                if (client)
                {
                    //something went wrong this is only a message for builders
                    Debug.Log("client received message designated for builder - something has gone horibly wrong");
                }
                else {
                    if (builderscene != null)
                    {
                        builderscene.EmailInUse();
                    }
                }
                break;

            case (1):
                //this is sent to the builder telling them that a verification code has been re-sent to their email
                if (client)
                {
                    //something went wrong this is only a message for builders
                    Debug.Log("client received message designated for builder - something has gone horibly wrong");
                }
                else
                {
                    if (builderscene != null)
                    {
                        builderscene.GoToVerifyEmailScreen();
                        builderscene.showPopup("Email verification code was re-sent to " + builderscene.StoreEmail);
                    }
                }
                break;
            case (2):
                //this is sent to the builder telling them that a verification code has been sent to their email
                if (client)
                {
                    //something went wrong this is only a message for builders
                    Debug.Log("client received message designated for builder - something has gone horibly wrong");
                }
                else
                {
                    if (builderscene != null)
                    {
                        builderscene.GoToVerifyEmailScreen();
                    }
                }
                break;
            case (4):
                //this is sent to the builder telling them that the email they put in is incorrect
                if (client)
                {
                    //something went wrong this is only a message for builders
                    Debug.Log("client received message designated for builder - something has gone horibly wrong");
                }
                else
                {
                    if (builderscene != null)
                    {
                        builderscene.LoginFail("No accounts with that email");
                    }
                }
                break;
            case (5):
                //this is sent to the builder telling them that the email is correct but wrong password
                if (client)
                {
                    //something went wrong this is only a message for builders
                    Debug.Log("client received message designated for builder - something has gone horibly wrong");
                }
                else
                {
                    if (builderscene != null)
                    {
                        builderscene.LoginFail("Incorrect password");
                    }
                }
                break;
            case (7):
                //this is sent to the builder telling them that login was a success and to go to account info set up
                if (client)
                {
                    //something went wrong this is only a message for builders
                    Debug.Log("client received message designated for builder - something has gone horibly wrong");
                }
                else
                {
                    if (builderscene != null)
                    {
                        builderscene.GoToAccountInfoSettup();
                    }
                }
                break;

            case (10):
                //this is reply to client after they uploaded a project and confirmation email was sent (successfuly)
                if (blockPlacer != null)
                {
                    blockPlacer.UploadResponce(true);
                }
            break;

            case (11):
                //this is reply to client after they uploaded a project and confirmation email was sent (unsuccessfuly)
                if (blockPlacer != null)
                {
                    blockPlacer.UploadResponce(false);
                }
                break;
        }
    }
}
