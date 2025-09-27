using UnityEngine;

public class SaveLoadUI : MonoBehaviour
{
    // ������������ģʽ
    public enum PanelMode { Save, Load }
    private PanelMode currentMode;

    [SerializeField] private GameObject saveSlotPrefab;
    [SerializeField] private Transform slotsParent; // ����Ӧ��ָ��ScrollView��Content����
    [SerializeField] private int numberOfSlots = 6;

    private void Start()
    {
        gameObject.SetActive(false);

        for (int i = 0; i < numberOfSlots; i++)
        {
            GameObject slotGO = Instantiate(saveSlotPrefab, slotsParent);
            SaveSlot slot = slotGO.GetComponent<SaveSlot>();
            slot.slotIndex = i;
            int index = i;
            slot.button.onClick.AddListener(() => OnSlotClicked(index));
        }
    }

    /// <summary>
    /// ����壬��������ģʽ
    /// </summary>
    public void OpenPanel(PanelMode mode)
    {
        currentMode = mode; // ��¼��ǰģʽ
        gameObject.SetActive(true);
        RefreshUI();
    }

    // ��������Ϊ�˷����ڰ�ť�� Inspector �е��ã����Ǵ������������Ĺ�������
    public void OpenInSaveMode()
    {
        OpenPanel(PanelMode.Save);
    }

    public void OpenInLoadMode()
    {
        OpenPanel(PanelMode.Load);
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    private void RefreshUI()
    {
        foreach (Transform child in slotsParent)
        {
            SaveSlot slot = child.GetComponent<SaveSlot>();
            GameData data = SaveManager.Instance.GetSaveInfo(slot.slotIndex);

            if (data == null) // �浵Ϊ��
            {
                slot.slotInfoText.text = "�մ浵";
                slot.slotStatusText.text = (currentMode == PanelMode.Save) ? "����Ա���" : "";
            }
            else // �浵����
            {
                slot.slotInfoText.text = $"�� {data.currentDay} �� | ���: {data.gold}";
                slot.slotStatusText.text = (currentMode == PanelMode.Save) ? $"<color=orange>����Ը���</color>\n{data.saveTime}" : data.saveTime;
            }
        }
    }

    private void OnSlotClicked(int slotIndex)
    {
        if (currentMode == PanelMode.Save)
        {
            // �浵ģʽ�£����۲�λ�Ƿ�Ϊ�գ���ִ�б���/����
            SaveManager.Instance.SaveGame(slotIndex);
            RefreshUI(); // �����ˢ��UI
            // �����������һ����ʾ�����Զ��ر����
            // ClosePanel();
        }
        else // Loadģʽ
        {
            GameData data = SaveManager.Instance.GetSaveInfo(slotIndex);
            if (data != null)
            {
                // ֻ���ڲ�λ������ʱ��ִ�ж�ȡ
                SaveManager.Instance.LoadGame(slotIndex);
            }
            else
            {
                // �ղ�λ����ִ���κβ���
                Debug.Log($"��λ {slotIndex} Ϊ�գ��޷���ȡ��");
            }
        }
    }

    public void OnDeleteAllClicked()
    {
        SaveManager.Instance.DeleteAllSaves();
        RefreshUI();
    }
}