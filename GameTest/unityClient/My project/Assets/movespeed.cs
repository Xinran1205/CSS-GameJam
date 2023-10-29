using UnityEngine;
using WebSocketSharp;
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
        ConnectToServer();
    }

    void ConnectToServer()
    {
        Debug.Log("Connecting to " + serverURL);
        websocket = new WebSocket(serverURL);
        websocket.OnOpen += OnWebSocketOpen;
        websocket.OnMessage += OnMessageReceived;
        websocket.OnClose += OnWebSocketClosed;
        websocket.OnError += OnWebSocketError;
        websocket.Connect();
    }

    private void OnWebSocketOpen(object sender, EventArgs e)
    {
        Debug.Log("WebSocket Connected!");
    }

    private void OnWebSocketError(object sender, ErrorEventArgs e)
    {
        Debug.LogError("WebSocket Error: " + e.Message);
        // Optionally, implement reconnect logic here
    }

    private void OnMessageReceived(object sender, MessageEventArgs e)
    {
        mainThreadActions.Enqueue(() =>
        {
            Debug.Log("message received: " + e.Data);
            try
            {
                ClientAction action = JsonUtility.FromJson<ClientAction>(e.Data);
                if (action == null)
                {
                    Debug.LogError("Failed to parse e.Data into ClientAction.");
                    return;
                }

                switch (action.Action)
                {
                    case "id":
                        playerID = action.PlayerID;
                        break;
                    case "join":
                        HandlePlayerJoined(action);
                        break;
                    case "move":
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
                Debug.LogError("Error while parsing e.Data: " + ex.Message);
            }
        });
    }

    private void OnWebSocketClosed(object sender, CloseEventArgs e)
    {
        Debug.Log("WebSocket Closed! Reason: " + e.Reason);
        // Optionally, implement reconnect logic here
    }

    private void HandlePlayerJoined(ClientAction action)
    {
        Debug.Log("Player " + action.PlayerID + " joined!");
        if (action.PlayerID == playerID) return;

        GameObject newPlayer = Instantiate(playerPrefab);
        newPlayer.transform.position = new Vector3(action.X, action.Y, 0);
        otherPlayers[action.PlayerID] = newPlayer;
    }

    private void HandlePlayerMoved(ClientAction action)
    {
        if (action.PlayerID == playerID) return;

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