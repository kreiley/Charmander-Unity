using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;

public class BaseClient : MonoSingleton<BaseClient>
{
    public string ipAddress = "2605:a601:aa36:3400:c87e:efe3:353b:885e";
    public ushort port = 8000; //do not put on 80 or 443
    public NetworkDriver driver;
    protected NetworkConnection connection;
    public int myConnectionId = -1;

    public int maxPlayers = 4; //maximum amount of players allowed on server

#if UNITY_EDITOR
    private void Start() { Init(); }
    private void Update() { UpdateServer(); }
    private void OnDestroy() { Shutdown(); }
#endif

    public virtual void Init()
    {
        //Initialize the Driver
        driver = NetworkDriver.Create();
        connection = default(NetworkConnection);

        NetworkEndPoint endpoint = NetworkEndPoint.Parse(ipAddress,port); 
        connection = driver.Connect(endpoint);

        Debug.Log("Attempting to connect to Server on " + endpoint.Address + " : " + endpoint.Port);
    }
    public virtual void Shutdown()
    {
        driver.Dispose();
    }
    public virtual void UpdateServer()
    {

        driver.ScheduleUpdate().Complete(); //you have to call a complete on your job so tread don't get locked
        CheckAlive();
        UpdateMessagePump(); // parse all messages client are sending us

    }
    private void CheckAlive()
    {
        if (!connection.IsCreated)
        {
            Debug.Log("Something went wrong, lost connection to server!");
        }
    }
    protected virtual void UpdateMessagePump()
    {
        DataStreamReader stream; //reads pretty much everything
        NetworkEvent.Type cmd;
        //while you are recieving a command from the user
        while ((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if(cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("We are now connected to the server");
            }
            else if(cmd == NetworkEvent.Type.Data)
            {
                OnData(stream); 
            }
            else if(cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                connection = default(NetworkConnection);
            }
        }
    }
    public virtual void SendToServer(NetMessage msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
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

        msg.ReceivedOnClient();
    }


}
