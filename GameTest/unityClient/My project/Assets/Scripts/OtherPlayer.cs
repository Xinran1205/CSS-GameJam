using System.Collections.Generic;
using UnityEngine;

public class OtherPlayer : MonoBehaviour
{
    public string PlayerID;

    private static Dictionary<string, OtherPlayer> otherPlayers = new Dictionary<string, OtherPlayer>();

    public static void SpawnOtherPlayer(string playerID, Vector2 position,int Order)
    {
        if (otherPlayers.ContainsKey(playerID))
        {
            return; // 如果玩家已经存在，就直接返回
        }
        Debug.Log($"Spawning player with ID: {playerID} at position: {position}");

        GameObject playerPrefab;
       
        if (Order == 1)
        {
            playerPrefab = FindObjectOfType<TCPConnection>().playerPrefab2;
        } else {
            playerPrefab = FindObjectOfType<TCPConnection>().playerPrefab;
        }
        
        // 使用预制实例化物体
        GameObject otherPlayerObj = Instantiate(playerPrefab, position, Quaternion.identity);

        var otherPlayer = otherPlayerObj.GetComponent<OtherPlayer>();
        if (otherPlayer == null)
        {
            otherPlayer = otherPlayerObj.AddComponent<OtherPlayer>();
        }
        otherPlayer.PlayerID = playerID;
        otherPlayer.transform.position = position;
        otherPlayers[playerID] = otherPlayer;
    }

    public static void RemoveOtherPlayer(string playerID)
    {
        if (otherPlayers.ContainsKey(playerID))
        {
            Destroy(otherPlayers[playerID].gameObject);
            otherPlayers.Remove(playerID);
        }
    }

    public static void MoveOtherPlayer(string playerID, Vector2 position, float direction)
    {
        if (otherPlayers.ContainsKey(playerID))
        {
            otherPlayers[playerID].transform.position = position;
            otherPlayers[playerID].transform.localScale = new Vector3(direction, 4, 1);
        }
    }

    public static Vector2 GetPositionOfPlayer(string playerID)
    {
        if (otherPlayers.ContainsKey(playerID))
        {
            return otherPlayers[playerID].transform.position;
        }
        return Vector2.zero;  // 返回一个默认值
    }
}