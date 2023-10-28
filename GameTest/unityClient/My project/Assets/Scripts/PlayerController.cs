using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    public string PlayerID; // This will be set by the server
    private TCPConnection connection;

    private void Start()
    {
        connection = FindObjectOfType<TCPConnection>();
    }

    private void Update()
    {
        float moveX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float moveY = Input.GetAxis("Vertical") * speed * Time.deltaTime;

        transform.Translate(new Vector2(moveX, moveY));

        if (moveX != 0 || moveY != 0)
        {
            ClientAction action = new ClientAction
            {
                Action = "move",
                PlayerID = PlayerID,
                X = transform.position.x,
                Y = transform.position.y
            };
            connection.SendMessageToServer(JsonUtility.ToJson(action));
        }
    }
}