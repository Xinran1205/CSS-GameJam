using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    public string PlayerID; // This will be set by the server
    public int Order;
    private TCPConnection connection;

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

        // 创建一个ClientAction并发送给服务器
        //ClientAction initialPositionAction = new ClientAction
        //{
        //    Action = "move",
        //    X = transform.position.x,
        //    Y = transform.position.y,
        //    Direction = transform.localScale.x,
        //};
        //connection.SendMessageToServer(JsonUtility.ToJson(initialPositionAction));
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
    }
}