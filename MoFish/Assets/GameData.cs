using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [System.Serializable] ��һ����Ҫ�����ԣ�������Unity�������Ա����л���ת����JSON��
[System.Serializable]
public class GameData
{
    // ��Ҫ�������������
    public int currentDay;
    public int hp;
    public int sanity;
    public int gold;
    public int actionPoints;

    // ���ǻ����Ա���һЩ������Ϣ�������ڴ浵��λ����ʾ
    public string saveTime; // ���磺"2025/09/27 08:30"
}