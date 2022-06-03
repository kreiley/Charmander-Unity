using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;

public class BaseServer : MonoBehaviour
{
    public string ipAddress = "2605:a601:aa36:3400:c87e:efe3:353b:885e";
    public ushort port = 8000; //do not put on 80 or 443
    public NetworkDriver driver;
    protected NativeList<NetworkConnection> connections;
    public int maxPlayers = 4; //maximum amount of players allowed on server

#if UNITY_EDITOR
    private void Start(){ Init(); }
    private void Update(){ UpdateServer(); }
    private void OnDestroy(){ Shutdown(); }
#endif

    public virtual void Init() {
        //Initialize the Driver
        driver = NetworkDriver.Create();
        NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = port;
        if(driver.Bind(endpoint) != 0)
        {
            Debug.Log("There was error binding to port " + endpoint.Port);
        }
        else
        {
            driver.Listen();
        }

        //Initialize the Connection List
        connections = new NativeList<NetworkConnection>(maxPlayers, Allocator.Persistent);
    }
    public virtual void Shutdown() {
        driver.Dispose();
        connections.Dispose();
    }
    public virtual void UpdateServer() {

        driver.ScheduleUpdate().Complete(); //you have to call a complete on your job so tread don't get locked
        CleanupConnections(); // this is when someone drops without calling the disconnect function
        AcceptNewConnections(); // see if new people are trying to connect
        UpdateMessagePump(); // parse all messages client are sending us

    }
    private void CleanupConnections()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                --i;
            }
        }

    }
    private void AcceptNewConnections()
    {
        NetworkConnection c;
        while((c = driver.Accept()) != default(NetworkConnection))
        {
            connections.Add(c);
            Debug.Log("Accepted a connection");
        }
    }
    protected virtual void UpdateMessagePump()
    {
        DataStreamReader stream; //reads pretty much everything
        for (int i = 0; i < connections.Length; i++)
        {
            NetworkEvent.Type cmd;
            //while you are recieving a command from the user
            while((cmd = driver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if(cmd == NetworkEvent.Type.Data)
                {
                    OnData(stream);
                }
                else if(cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    connections[i] = default(NetworkConnection);
                }
            }
        }
    }

    public virtual void OnData(DataStreamReader stream)
    {
        NetMessage msg = null;
        var opCode = (OpCode)stream.ReadByte();
        switch (opCode)
        {
            case OpCode.CHAT_MESSAGE: msg = new Net_ChatMessage(stream); break;
            case OpCode.PLAYER_POSITION: msg = new Net_PlayerPosition(stream); break;
            default:
                Debug.Log("Message received had no OpCode");
                break;
        }

        msg.ReceivedOnServer(this);
    }
    public virtual void Broadcast(NetMessage msg)
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (connections[i].IsCreated)
            {
                SendToClient(connections[i], msg);
            }
        }
    }

    public virtual void SendToClient(NetworkConnection connection, NetMessage msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }

}
