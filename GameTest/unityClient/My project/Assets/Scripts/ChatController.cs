using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ChatController : MonoBehaviour
{
    public Text chatBox;  // 绑定聊天信息显示的文本框
    public InputField inputField; // 绑定玩家输入的输入框
    public Button  sendButton; // 绑定发送按钮
    public TCPConnection connection; // 引用到TCPConnection组件\
    public ScrollRect chatScrollRect;

    private void Start()
    {
        sendButton.onClick.AddListener(HandleSendButton);
    }

    private void Update()
    {
        // 如果按下回车键，输入框不为空，且输入框是当前的活动UI元素
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            HandleSendButton();
        }
    }

    void HandleSendButton()
    {
        string message = inputField.text;
        if (!string.IsNullOrEmpty(message))
        {
            // 构建一个聊天消息对象并发送给服务器
            ClientAction chatAction = new ClientAction
            {
                Action = "chat",
                PlayerID = connection.localPlayer.PlayerID,
                Message = message
            };
            connection.SendMessageToServer(JsonUtility.ToJson(chatAction));

            // 清空输入框
            inputField.text = "";

            DisplayChatMessage(chatAction.PlayerID, chatAction.Message);
        }
    }

    public void DisplayChatMessage(string playerID, string message)
    {
        chatBox.text += $"{playerID}: {message}\n";
        StartCoroutine(ScrollToBottom());
    }

    private IEnumerator ScrollToBottom()
    {
        // 等待直到所有的UI元素都渲染完毕
        yield return new WaitForEndOfFrame();
        // 强制更新Canvas以确保所有的UI元素都已经更新
        Canvas.ForceUpdateCanvases();
        // 设置滚动位置到底部
        chatScrollRect.verticalNormalizedPosition = 0;
    }
}