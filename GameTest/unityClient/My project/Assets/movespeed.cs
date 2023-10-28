using UnityEngine;
using WebSocketSharp;
using System.Text;
using System;
using System.Collections.Generic;

public class MoveSquare : MonoBehaviour
{
    public float speed = 5.0f;
    private WebSocket websocket;
    public string serverURL = "ws://localhost:8000/ws";
    public GameObject playerPrefab;
    private string playerID;

    private Dictionary<string, GameObject> otherPlayers = new Dictionary<string, GameObject>();

    private Queue<Action> mainThreadActions = new Queue<Action>();

    void Start()
    {
        Debug.Log("Connecting to " + serverURL);
        websocket = new WebSocket(serverURL);

        websocket.OnMessage += OnMessageReceived;
        websocket.OnClose += OnWebSocketClosed;

        websocket.Connect();
    }

    private void OnMessageReceived(object sender, MessageEventArgs e)
    {
        mainThreadActions.Enqueue(() =>
        {
            //这里全部是处理服务器发来的消息
            Debug.Log("message received");
            Debug.Log("Received data: " + e.Data);
            try
            {
                ClientAction action = JsonUtility.FromJson<ClientAction>(e.Data);

                if (action == null)
                {
                    Debug.LogError("Failed to parse e.Data into ClientAction.");
                    return;
                }

                if (action.Action == "id")
                {
                    playerID = action.PlayerID;
                    return;
                }

                switch (action.Action)
                {
                    case "join":
                        HandlePlayerJoined(action);
                        break;
                    case "move":
                        Debug.Log("new move");
                        HandlePlayerMoved(action);
                        break;
                    case "leave":
                        HandlePlayerLeft(action);
                        break;
                    default:
                        Debug.LogWarning("Unknown action type: " + action.Action);
                        break;
                }

            }
            catch (Exception ex)
            {
                Debug.LogError("Error while parsing e.Data: " + ex.Message);  // 打印异常信息
            }
        });
        
    }

    private void OnWebSocketClosed(object sender, CloseEventArgs e)
    {
        Debug.Log("WebSocket Closed!");
    }

    private void HandlePlayerJoined(ClientAction action)
    {
        //打印玩家加入的消息
        Debug.Log("Player " + action.PlayerID + " joined!");
        if (action.PlayerID == playerID) return;  // 忽略自己

        GameObject newPlayer = Instantiate(playerPrefab);
        newPlayer.transform.position = new Vector3(action.X, action.Y, 0);
        otherPlayers[action.PlayerID] = newPlayer;
    }

    private void HandlePlayerMoved(ClientAction action)
    {
        if (action.PlayerID == playerID) return;  // 忽略自己

        if (otherPlayers.ContainsKey(action.PlayerID))
        {
            GameObject playerObject = otherPlayers[action.PlayerID];
            playerObject.transform.position = new Vector3(action.X, action.Y, 0);
        }
    }

    private void HandlePlayerLeft(ClientAction action)
    {
        Debug.Log("Player " + action.PlayerID + " left!");
        if (otherPlayers.ContainsKey(action.PlayerID))
        {
            GameObject playerObject = otherPlayers[action.PlayerID];
            Destroy(playerObject);
            otherPlayers.Remove(action.PlayerID);
        }
    }

    void Update()
    {
        while (mainThreadActions.Count > 0)
        {
            var action = mainThreadActions.Dequeue();
            action();
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, vertical, 0);
        transform.Translate(movement * speed * Time.deltaTime, Space.World);

        if (movement != Vector3.zero)
            SendPositionToServer();
    }

    private void SendPositionToServer()
    {
        ClientAction position = new ClientAction
        {
            Action = "move",
            PlayerID = playerID,
            X = transform.position.x,
            Y = transform.position.y
        };

        string jsonPosition = JsonUtility.ToJson(position);
        if (websocket.ReadyState == WebSocketState.Open)
            websocket.Send(jsonPosition);
    }

    private void OnApplicationQuit()
    {
        if (websocket != null)
        {
            websocket.Close();
            websocket = null;
        }
    }

    [Serializable]
    public class ClientAction
    {
        public string Action;
        public string PlayerID;
        public float X;
        public float Y;
    }
}