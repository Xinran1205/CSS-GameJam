using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ChatController : MonoBehaviour
{
    public Text chatBox;  // ��������Ϣ��ʾ���ı���
    public InputField inputField; // ���������������
    public Button  sendButton; // �󶨷��Ͱ�ť
    public TCPConnection connection; // ���õ�TCPConnection���\
    public ScrollRect chatScrollRect;

    private void Start()
    {
        sendButton.onClick.AddListener(HandleSendButton);
    }

    private void Update()
    {
        // ������»س����������Ϊ�գ���������ǵ�ǰ�ĻUIԪ��
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
            // ����һ��������Ϣ���󲢷��͸�������
            ClientAction chatAction = new ClientAction
            {
                Action = "chat",
                PlayerID = connection.localPlayer.PlayerID,
                Message = message
            };
            connection.SendMessageToServer(JsonUtility.ToJson(chatAction));

            // ��������
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
        // �ȴ�ֱ�����е�UIԪ�ض���Ⱦ���
        yield return new WaitForEndOfFrame();
        // ǿ�Ƹ���Canvas��ȷ�����е�UIԪ�ض��Ѿ�����
        Canvas.ForceUpdateCanvases();
        // ���ù���λ�õ��ײ�
        chatScrollRect.verticalNormalizedPosition = 0;
    }
}