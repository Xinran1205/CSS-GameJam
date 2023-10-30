using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public float speed = 10.0f;
    public string PlayerID; // This will be set by the server
    public int Order;
    private TCPConnection connection;

    private bool isInfecteda = false;
    //����һ��ӳ�䣬���ڸ���order�ҵ���Ӧ��playerID
    public Dictionary<int, string> orderToPlayerIDMap = new Dictionary<int, string>();

    public GameObject finalpage;
    public Animator animator;
    public bool isBoss=false;

    private void Start()
    {
        connection = FindObjectOfType<TCPConnection>();
        animator = GetComponent<Animator>();

        // ���������¼�
        connection.OnConnectedToServer += SendInitialPosition;

    }

    void SendInitialPosition()
    {
        float randomX =0;  
        float randomY =0;  
        //����boss�ĳ�ʼ��λ��
        if (Order == 1)
        {
            randomX = UnityEngine.Random.Range(-12f, -6f);  
            randomY = UnityEngine.Random.Range(-4.0f, -1.5f);  
        }else{
            int randomZone = UnityEngine.Random.Range(0, 4);  // ��ȡ0��3֮��������

            switch (randomZone)
            {
                case 0:
                    // ��һ����ʼ������
                    randomX = UnityEngine.Random.Range(-42f,-37f);
                    randomY = UnityEngine.Random.Range(9.5f,4.93f);
                    break;
                case 1:
                    // �ڶ�����ʼ������
                    randomX = UnityEngine.Random.Range(-42f,-37.7f);
                    randomY = UnityEngine.Random.Range(-13f,-18.9f);
                    break;
                case 2:
                    // ��������ʼ������
                    randomX = UnityEngine.Random.Range(20f,24.44f);
                    randomY = UnityEngine.Random.Range(8.31f,4.73f);
                    break;
                case 3:
                    // ���ĸ���ʼ������
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
        //�����һ����if�жϣ�
        if (isInfecteda)
        {
            //��������ڼ�һ��if�жϣ������ж�����Ƿ������˳���Ϸ�İ�ť
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

        // ��ȡ��Ⱦ���λ��
        Vector2 infectedHorsePosition = OtherPlayer.GetPositionOfPlayer(infectedHorsePlayerID);
        float distance = Vector2.Distance(transform.position, infectedHorsePosition);

        // �������С����ֵ�����ͱ���Ⱦ��Ϣ��������
        if (distance < 2f)  // �ɸ�����Ҫ���������ֵ
        {
            ClientAction infectedAction = new ClientAction
            {
                Action = "infected",
                PlayerID = PlayerID,
                Order = Order
            };
            //�ѱ���Ⱦ����Ϣ������������Ȼ��������ͻ�������Ϣ�������пͻ��ˣ�Ȼ��ͻ���ʲô��������
            connection.SendMessageToServer(JsonUtility.ToJson(infectedAction));
            isInfecteda = true;


            if (isInfecteda)
            {
                finalpage.SetActive(true);
                Invoke("HideFinalPage", 5.0f); // ���磬5 ������� finalpage

            }

            //��������������
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
        return null;  // �򷵻�����Ĭ��ֵ
    }
}