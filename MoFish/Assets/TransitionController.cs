using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // �������� TextMeshPro �����ռ�

public class TransitionController : MonoBehaviour
{
    // �ȴ��������������ʵ��ӳ�����ҿ����ܽ�
    public float delay = 3.5f;

    // ��Unity�༭���У���Ҫ�ѳ������UI�ı��ϵ�����
    public TextMeshProUGUI summaryText;

    void Start()
    {
        // ȷ���ҵ���UI�ı���
        if (summaryText == null)
        {
            Debug.LogError("���ɳ�����SummaryTextû����Inspector�����ã�");
            return;
        }

        // ȷ��GameManagerʵ������
        if (GameManager.Instance != null)
        {
            // ��GameManager��ȡ�ܽ��ı�
            string summary = GameManager.Instance.endOfDaySummary;

            // ����ܽ��ı���Ϊ�գ�����ʾ��
            if (!string.IsNullOrEmpty(summary))
            {
                summaryText.text = summary;
            }
            else
            {
                // ����ǿյģ������һ�쿪ʼʱ��������ʾĬ������
                summaryText.text = "�µ�һ�쿪ʼ��...";
            }
        }

        // ����Э�̣����ӳٺ����������
        StartCoroutine(LoadMainSceneAfterDelay());
    }

    IEnumerator LoadMainSceneAfterDelay()
    {
        // �ȴ�ָ��������
        yield return new WaitForSeconds(delay);

        // ����������
        SceneManager.LoadScene("MainScene");
    }
}
