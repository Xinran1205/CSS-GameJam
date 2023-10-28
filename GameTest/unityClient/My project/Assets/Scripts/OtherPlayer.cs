using System.Collections.Generic;
using UnityEngine;

public class OtherPlayer : MonoBehaviour
{
    public string PlayerID;

    private static Dictionary<string, OtherPlayer> otherPlayers = new Dictionary<string, OtherPlayer>();

    public static void SpawnOtherPlayer(string playerID, Vector2 position)
    {
        if (otherPlayers.ContainsKey(playerID))
        {
            return; // 如果玩家已经存在，就直接返回
        }
        Debug.Log($"Spawning player with ID: {playerID} at position: {position}");
        GameObject otherPlayerObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        otherPlayerObj.transform.localScale = new Vector3(1, 1, 1); // 设置大小
        var renderer = otherPlayerObj.GetComponent<Renderer>();
        renderer.material.color = Color.white; // 设置颜色
        var otherPlayer = otherPlayerObj.AddComponent<OtherPlayer>();
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

    public static void MoveOtherPlayer(string playerID, Vector2 position)
    {
        if (otherPlayers.ContainsKey(playerID))
        {
            otherPlayers[playerID].transform.position = position;
        }
    }
}