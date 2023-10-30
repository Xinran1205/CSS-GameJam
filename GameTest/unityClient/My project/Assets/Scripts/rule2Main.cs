using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class rule2Main : MonoBehaviour
{
    // Start is called before the first frame update
    public void ChangeSceneWithDelay(string sceneName, float delay)
    {
        StartCoroutine(LoadSceneAfterDelay(sceneName, delay));
    }

    public void StartGame()
    {
        ChangeSceneWithDelay("2Dmap", 0.2f); // �滻Ϊ��������Ϸ����������
    }

    private IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
}
