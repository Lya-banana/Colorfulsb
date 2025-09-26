using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // 必须引入 TextMeshPro 命名空间

public class TransitionController : MonoBehaviour
{
    // 等待的秒数，可以适当延长让玩家看清总结
    public float delay = 3.5f;

    // 在Unity编辑器中，需要把场景里的UI文本拖到这里
    public TextMeshProUGUI summaryText;

    void Start()
    {
        // 确保找到了UI文本框
        if (summaryText == null)
        {
            Debug.LogError("过渡场景的SummaryText没有在Inspector中设置！");
            return;
        }

        // 确保GameManager实例存在
        if (GameManager.Instance != null)
        {
            // 从GameManager获取总结文本
            string summary = GameManager.Instance.endOfDaySummary;

            // 如果总结文本不为空，就显示它
            if (!string.IsNullOrEmpty(summary))
            {
                summaryText.text = summary;
            }
            else
            {
                // 如果是空的（比如第一天开始时），就显示默认文字
                summaryText.text = "新的一天开始了...";
            }
        }

        // 启动协程，在延迟后加载主场景
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
