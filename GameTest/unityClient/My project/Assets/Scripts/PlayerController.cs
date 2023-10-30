using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public float speed = 10.0f;
    public string PlayerID; // This will be set by the server
    public int Order;
    private TCPConnection connection;

    private bool isInfecteda = false;
    //增加一个映射，用于根据order找到对应的playerID
    public Dictionary<int, string> orderToPlayerIDMap = new Dictionary<int, string>();

    public GameObject finalpage;
    public Animator animator;
    public bool isBoss=false;

    private void Start()
    {
        connection = FindObjectOfType<TCPConnection>();
        animator = GetComponent<Animator>();

        // 订阅连接事件
        connection.OnConnectedToServer += SendInitialPosition;

    }

    void SendInitialPosition()
    {
        float randomX =0;  
        float randomY =0;  
        //这是boss的初始化位置
        if (Order == 1)
        {
            randomX = UnityEngine.Random.Range(-12f, -6f);  
            randomY = UnityEngine.Random.Range(-4.0f, -1.5f);  
        }else{
            int randomZone = UnityEngine.Random.Range(0, 4);  // 获取0到3之间的随机数

            switch (randomZone)
            {
                case 0:
                    // 第一个初始化区域
                    randomX = UnityEngine.Random.Range(-42f,-37f);
                    randomY = UnityEngine.Random.Range(9.5f,4.93f);
                    break;
                case 1:
                    // 第二个初始化区域
                    randomX = UnityEngine.Random.Range(-42f,-37.7f);
                    randomY = UnityEngine.Random.Range(-13f,-18.9f);
                    break;
                case 2:
                    // 第三个初始化区域
                    randomX = UnityEngine.Random.Range(20f,24.44f);
                    randomY = UnityEngine.Random.Range(8.31f,4.73f);
                    break;
                case 3:
                    // 第四个初始化区域
                    randomX = UnityEngine.Random.Range(16.1f,21.5f);
                    randomY = UnityEngine.Random.Range(-18.7f,-21.4f);
                    break;
            }
        }

        transform.position = new Vector2(randomX, randomY);

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
        //这个做一个大if判断，
        if (isInfecteda)
        {
            //这个里面在加一个if判断，让他判断玩家是否点击了退出游戏的按钮
            return;
        }else{
            float moveX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
            float moveY = Input.GetAxis("Vertical") * speed * Time.deltaTime;

            if (Input.GetAxis("Horizontal") < 0)
                transform.localScale = new Vector3(4, 4, 1);
            else if (Input.GetAxis("Horizontal") > 0)
                transform.localScale = new Vector3(-4, 4, 1);

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
    }


    private void CheckDistanceWithInfectedHorse()
    {
        string infectedHorsePlayerID = GetPlayerIDByOrder(1);

        // 获取感染马的位置
        Vector2 infectedHorsePosition = OtherPlayer.GetPositionOfPlayer(infectedHorsePlayerID);
        float distance = Vector2.Distance(transform.position, infectedHorsePosition);

        // 如果距离小于阈值，发送被感染消息给服务器
        if (distance < 2f)  // 可根据需要调整这个阈值
        {
            ClientAction infectedAction = new ClientAction
            {
                Action = "infected",
                PlayerID = PlayerID,
                Order = Order
            };
            //把被感染的消息发给服务器，然后服务器就会把这个消息发给所有客户端，然后客户端什么都不用做
            connection.SendMessageToServer(JsonUtility.ToJson(infectedAction));
            isInfecteda = true;


            if (isInfecteda)
            {
                finalpage.SetActive(true);
                Invoke("HideFinalPage", 5.0f); // 例如，5 秒后隐藏 finalpage

            }

            //这把玩家物体隐藏
            this.gameObject.SetActive(false);

          
        }
    }

    void HideFinalPage()
    {
        finalpage.SetActive(false);
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