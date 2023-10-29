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

    public GameObject playerPrefab;
    public GameObject playerPrefab2;

    public Queue<string> messageQueue = new Queue<string>();

    public delegate void ConnectedToServerHandler();
    public event ConnectedToServerHandler OnConnectedToServer;

    void Start()
    {
        client = new TcpClient("13.48.183.56", 8000);
        NetworkStream stream = client.GetStream();
        writer = new StreamWriter(stream);
        reader = new StreamReader(stream);

        //直接从服务器读取初始消息
        string initialMessage = reader.ReadLine();
        //然后在这初始化playerId和order
        HandleInitialMessage(initialMessage);

        receiveThread = new Thread(ReceiveMessages);
        receiveThread.Start();

        OnConnectedToServer?.Invoke();
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

    void HandleInitialMessage(string message)
    {
        ClientAction action = JsonUtility.FromJson<ClientAction>(message);
        if (action.Action == "myID")
        {
            Debug.Log($"Initial MyID action for PlayerID: {action.PlayerID}, Order: {action.Order}");
            localPlayer.PlayerID = action.PlayerID;
            localPlayer.Order = action.Order;
            if (localPlayer.Order == 1)
            {
                localPlayer.isBoss = true;
            }
        }
        else
        {
            Debug.LogError("Expected 'myID' as the initial message but received: " + action.Action);
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
                OtherPlayer.SpawnOtherPlayer(action.PlayerID, new Vector2(action.X, action.Y),action.Order);

                localPlayer.orderToPlayerIDMap[action.Order] = action.PlayerID;
                break;
            case "move":
                if (action.PlayerID != localPlayer.PlayerID)
                {
                    Debug.Log($"Move action for PlayerID: {action.PlayerID}");
                    OtherPlayer.MoveOtherPlayer(action.PlayerID, new Vector2(action.X, action.Y), action.Direction);
                }
                break;
            case "infected":
                //被感染可以什么都不做，因为被感染的玩家会退出游戏
                Debug.Log($"Player with PlayerID: {action.PlayerID} got infected");
                //Debug.Log($"Leave action for PlayerID: {action.PlayerID}");
                //// 首先和leave一样，删除感染的玩家
                //OtherPlayer.RemoveOtherPlayer(action.PlayerID);
                break;
            case "leave":
                Debug.Log($"Leave action for PlayerID: {action.PlayerID}");
                OtherPlayer.RemoveOtherPlayer(action.PlayerID);
                break;
        }
    }

    //void EndGame()
    //{
    //    // 显示游戏结束界面或其他逻辑
    //    // ... 

    //    // 断开与服务器的连接
    //    if (receiveThread != null)
    //        receiveThread.Abort();
    //    client.Close();
    //}

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
            PlayerID = localPlayer.PlayerID,
            Order = localPlayer.Order
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
    public float Direction;
    public int Order;
}
