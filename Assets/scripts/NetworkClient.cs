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

    public bool connected = false;
    //local ip "127.0.0.1"

    public string SERVER_IP = "127.0.0.1";
    ushort PORT = 8999;
    const int BYTE_SIZE = 512;
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

    private void OnData(NetMsg msg)
    {
        //Debug.Log("Received a message of type "  + msg.OP);

        switch (msg.OP)
        {
            case NetOP.None:
                Debug.Log("Unexpected NET_OP");
                break;


        }
    }
}
