using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    public string PlayerID; // This will be set by the server
    public int Order;
    private TCPConnection connection;

    //����һ��ӳ�䣬���ڸ���order�ҵ���Ӧ��playerID
    public Dictionary<int, string> orderToPlayerIDMap = new Dictionary<int, string>();

    public Animator animator;
    public bool isBoss=false;

    private void Start()
    {
        connection = FindObjectOfType<TCPConnection>();
        animator = GetComponent<Animator>();

        // ���������¼�
        connection.OnConnectedToServer += SendInitialPosition;

        float randomX = UnityEngine.Random.Range(-3.0f, 3.0f);  // ���賡������Ǵ�-10��10
        float randomY = UnityEngine.Random.Range(-3.0f, 3.0f);  // ���賡���߶��Ǵ�-10��10
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

        // ��ȡ��Ⱦ���λ��
        Vector2 infectedHorsePosition = OtherPlayer.GetPositionOfPlayer(infectedHorsePlayerID);
        float distance = Vector2.Distance(transform.position, infectedHorsePosition);

        // �������С����ֵ�����ͱ���Ⱦ��Ϣ��������
        if (distance < 0.5f)  // �ɸ�����Ҫ���������ֵ
        {
            ClientAction infectedAction = new ClientAction
            {
                Action = "infected",
                PlayerID = PlayerID
            };
            connection.SendMessageToServer(JsonUtility.ToJson(infectedAction));
      
            // �˳���Ϸ
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
        return null;  // �򷵻�����Ĭ��ֵ
    }
}