using System.Net.Sockets;
using System.IO;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;

public class TCPConnection : MonoBehaviour
{
    private TcpClient client;
    private StreamWriter writer;
    private StreamReader reader;
    private Thread receiveThread;

    public PlayerController localPlayer;

    public Queue<string> messageQueue = new Queue<string>();

    void Start()
    {
        client = new TcpClient("127.0.0.1", 8000);
        NetworkStream stream = client.GetStream();
        writer = new StreamWriter(stream);
        reader = new StreamReader(stream);

        receiveThread = new Thread(ReceiveMessages);
        receiveThread.Start();
    }

    void Update()
    {
        while (messageQueue.Count > 0)
        {
            string message = messageQueue.Dequeue();
            HandleMessage(message);
        }
    }

    void ReceiveMessages()
    {
        while (client.Connected)
        {
            string message = reader.ReadLine();
            if (!string.IsNullOrEmpty(message))
            {
                Debug.Log($"Received message from server: {message}"); // 打印接收到的消息
                messageQueue.Enqueue(message);
            }
        }
    }

    void HandleMessage(string message)
    {
        Debug.Log($"Handling message: {message}"); // 打印正在处理的消息
        ClientAction action = JsonUtility.FromJson<ClientAction>(message);
        switch (action.Action)
        {
            case "join":
                Debug.Log($"Join action for PlayerID: {action.PlayerID}");
                OtherPlayer.SpawnOtherPlayer(action.PlayerID, new Vector2(action.X, action.Y));
                break;
            case "move":
                if (action.PlayerID != localPlayer.PlayerID)
                {
                    Debug.Log($"Move action for PlayerID: {action.PlayerID}");
                    OtherPlayer.MoveOtherPlayer(action.PlayerID, new Vector2(action.X, action.Y));
                }
                break;
            case "leave":
                Debug.Log($"Leave action for PlayerID: {action.PlayerID}");
                OtherPlayer.RemoveOtherPlayer(action.PlayerID);
                break;
        }
    }


    public void SendMessageToServer(string message)
    {
        if (client.Connected)
        {
            Debug.Log($"Sending message to server: {message}"); // 打印正在发送的消息
            writer.WriteLine(message);
            writer.Flush();
        }
    }

    void OnApplicationQuit()
    {
        ClientAction leaveAction = new ClientAction
        {
            Action = "leave",
            PlayerID = localPlayer.PlayerID
        };
        SendMessageToServer(JsonUtility.ToJson(leaveAction));

        if (receiveThread != null)
            receiveThread.Abort();
        client.Close();
    }
}

[System.Serializable]
public class ClientAction
{
    public string Action;
    public string PlayerID;
    public float X;
    public float Y;
}
