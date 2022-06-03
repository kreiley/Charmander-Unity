using UnityEngine;

public class SendPosition : MonoBehaviour
{
    private float lastSend;
    private PixieClient client;

    private void Start()
    {
        client = FindObjectOfType<PixieClient>();
    }

    private void Update()
    {
        if(Time.time - lastSend > 1.0f) //if time since last send is greater than one second
        {
            Net_PlayerPosition ps = new Net_PlayerPosition(666, transform.position.x, transform.position.y, transform.position.z);
            client.SendToServer(ps);
            lastSend = Time.time;
        }
    }

}
