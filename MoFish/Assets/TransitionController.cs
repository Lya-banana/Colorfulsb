using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionController : MonoBehaviour
{
    // �ȴ�������
    public float delay = 2f;

    // Start is called before the first frame update
    void Start()
    {
        // ����һ��Э�̣����ڵȴ�
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