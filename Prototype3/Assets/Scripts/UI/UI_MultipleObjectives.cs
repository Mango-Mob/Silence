using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_MultipleObjectives : MonoBehaviour
{
    private TMP_Text ObjectiveText;
    RectTransform rectTransform;
    private float m_maxMoney = 0.0f;
    public bool m_escaped = false;

    // Start is called before the first frame update
    void Start()
    {
        LootItem[] lootArray = FindObjectsOfType<LootItem>();
        foreach (var loot in lootArray)
        {
            m_maxMoney += loot.m_lootValue;
        }
        ObjectiveText = GetComponent<TMP_Text>();
        rectTransform = ObjectiveText.GetComponent<RectTransform>();
        TaskUpdate();
    }

    // Update is called once per frame
    void Update()
    {
        TaskUpdate();
    }
    public void TaskUpdate()
    {
        ObjectiveText.text = "";
        string checkValue = (GameManager.instance.lootValue >= m_maxMoney) ? "•" : "○";
        ObjectiveText.text += $"{checkValue} Steal Loot: $ {GameManager.instance.lootValue} / $ {m_maxMoney}";

        if (GameManager.instance.lootValue >= m_maxMoney)
        {
            checkValue = (m_escaped) ? "•" : "○";
            ObjectiveText.text += $"\n{checkValue} Escape to the exit";
        }
    }
}
