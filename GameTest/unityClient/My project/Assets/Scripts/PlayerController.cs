using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    public string PlayerID; // This will be set by the server
    public int Order;
    private TCPConnection connection;

    //增加一个映射，用于根据order找到对应的playerID
    public Dictionary<int, string> orderToPlayerIDMap = new Dictionary<int, string>();

    public Animator animator;
    public bool isBoss=false;

    private void Start()
    {
        connection = FindObjectOfType<TCPConnection>();
        animator = GetComponent<Animator>();

        // 订阅连接事件
        connection.OnConnectedToServer += SendInitialPosition;

        float randomX = UnityEngine.Random.Range(-3.0f, 3.0f);  // 假设场景宽度是从-10到10
        float randomY = UnityEngine.Random.Range(-3.0f, 3.0f);  // 假设场景高度是从-10到10
        transform.position = new Vector2(randomX, randomY);

    }

    void SendInitialPosition()
    {
        ClientAction initialPositionAction = new ClientAction
        {
            Action = "move",
            X = transform.position.x,
            Y = transform.position.y,
            Direction = transform.localScale.x,
            PlayerID = PlayerID,
            Order = Order
        };
        connection.SendMessageToServer(JsonUtility.ToJson(initialPositionAction));
    }

    private void Update()
    {
        float moveX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float moveY = Input.GetAxis("Vertical") * speed * Time.deltaTime;

        if (Input.GetAxis("Horizontal") < 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (Input.GetAxis("Horizontal") > 0)
            transform.localScale = new Vector3(-1, 1, 1);

        transform.Translate(new Vector2(moveX, moveY));

        if (isBoss)
        {
            animator.SetTrigger("isBoss");
        }

        if (moveX != 0 || moveY != 0)
        {
            ClientAction action = new ClientAction
            {
                Action = "move",
                PlayerID = PlayerID,
                X = transform.position.x,
                Y = transform.position.y,
                Direction = transform.localScale.x,
                Order = Order
            };
            connection.SendMessageToServer(JsonUtility.ToJson(action));
        }

        if (!isBoss)
        {
            CheckDistanceWithInfectedHorse();
        }
    }

    private void CheckDistanceWithInfectedHorse()
    {
        string infectedHorsePlayerID = GetPlayerIDByOrder(1);

        // 获取感染马的位置
        Vector2 infectedHorsePosition = OtherPlayer.GetPositionOfPlayer(infectedHorsePlayerID);
        float distance = Vector2.Distance(transform.position, infectedHorsePosition);

        // 如果距离小于阈值，发送被感染消息给服务器
        if (distance < 0.5f)  // 可根据需要调整这个阈值
        {
            ClientAction infectedAction = new ClientAction
            {
                Action = "infected",
                PlayerID = PlayerID
            };
            connection.SendMessageToServer(JsonUtility.ToJson(infectedAction));
      
            // 退出游戏
            QuitGame();
        }
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public string GetPlayerIDByOrder(int order)
    {
        if (orderToPlayerIDMap.ContainsKey(order))
        {
            return orderToPlayerIDMap[order];
        }
        return null;  // 或返回其他默认值
    }
}