using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionController : MonoBehaviour
{
    // 等待的秒数
    public float delay = 2f;

    // Start is called before the first frame update
    void Start()
    {
        // 启动一个协程，用于等待
        StartCoroutine(LoadMainSceneAfterDelay());
    }

    IEnumerator LoadMainSceneAfterDelay()
    {
        // 等待指定的秒数
        yield return new WaitForSeconds(delay);

        // 加载主场景
        SceneManager.LoadScene("MainScene");
    }
}