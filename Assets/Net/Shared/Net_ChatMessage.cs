using Unity.Networking.Transport;
using UnityEngine;
using Unity.Collections;

public class Net_ChatMessage : NetMessage
{

    //0-8 OpCode
    //8-128 String Message
    public FixedString128Bytes ChatMessage { set; get; }

    public Net_ChatMessage()
    {
        Code = OpCode.CHAT_MESSAGE;

    }
    public Net_ChatMessage(DataStreamReader reader)
    {
        Code = OpCode.CHAT_MESSAGE;
        Deserialize(reader);
    }

    public Net_ChatMessage(string msg)
    {
        Code = OpCode.CHAT_MESSAGE;
        ChatMessage = msg;
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code); //OpCode is first always comes before message
        writer.WriteFixedString128(ChatMessage); //then string message of fixed length

    }
    public override void Deserialize(DataStreamReader reader)
    {
        //The first byte is handled already
        ChatMessage = reader.ReadFixedString128();
    }
    public override void ReceivedOnServer(BaseServer server)
    {
        Debug.Log("SERVER::" + ChatMessage);
        server.Broadcast(this);
    }
    public override void ReceivedOnClient()
    {
        Debug.Log("CLIENT::" + ChatMessage);
    }
}
