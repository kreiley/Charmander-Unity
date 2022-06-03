using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public class Net_PlayerPosition : NetMessage
{

    //0-8 OpCode
    public int PlayerId { set; get; }
    public float PositionX { set; get; }
    public float PositionY { set; get; }
    public float PositionZ { set; get; }

    public Net_PlayerPosition()
    {
        Code = OpCode.PLAYER_POSITION;

    }
    public Net_PlayerPosition(DataStreamReader reader)
    {
        Code = OpCode.PLAYER_POSITION;
        Deserialize(reader);
    }

    public Net_PlayerPosition(int playerId, float x, float y, float z)
    {
        Code = OpCode.PLAYER_POSITION;
        PlayerId = playerId;
        PositionX = x;
        PositionY = y;
        PositionZ = z;
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code); //OpCode is first always comes before message
        writer.WriteInt(PlayerId);
        writer.WriteFloat(PositionX);
        writer.WriteFloat(PositionY);
        writer.WriteFloat(PositionZ);

    }
    public override void Deserialize(DataStreamReader reader)
    {
        //The first byte is handled already
        PlayerId = reader.ReadInt();
        PositionX = reader.ReadFloat();
        PositionY = reader.ReadFloat();
        PositionZ = reader.ReadFloat();
    }
    public override void ReceivedOnServer(BaseServer server)
    {
        Debug.Log("SERVER::" + PlayerId + "::" + PositionX + "::" + PositionY + "::" + PositionZ);
        server.Broadcast(this);
    }
    public override void ReceivedOnClient()
    {
        Debug.Log("CLIENT::" + PlayerId + "::" + PositionX + "::" + PositionY + "::" + PositionZ);
    }
}
