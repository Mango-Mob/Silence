using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_MultipleObjectives : MonoBehaviour
{
    private TMP_Text ObjectiveText;
    private float m_maxMoney = 0.0f;
    public bool m_escaped = false;
    public bool m_objectivesComplete = false;
    public CanvasGroup m_canvasGroup;
    public float m_fadeSpeed = 0.2f;
    public float m_alphaMin = 0.3f;

    public GameObject m_LootVFX = null;
    private float m_currentLoot = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        LootManager[] lootArray = FindObjectsOfType<LootManager>();
        foreach (var loot in lootArray)
        {
            m_maxMoney += loot.m_currentValue;
        }

        m_maxMoney *= 0.75f; // only need 75% of money
        m_maxMoney -= m_maxMoney % 10; // rounding

        ObjectiveText = GetComponent<TMP_Text>();
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

        if(m_currentLoot < GameManager.instance.lootValue)
        {
            Instantiate(m_LootVFX, transform.parent);
            m_currentLoot = GameManager.instance.lootValue;
        }

        if (GameManager.instance.lootValue >= m_maxMoney)
        {
            m_objectivesComplete = true;
            checkValue = (m_escaped) ? "•" : "○";
            ObjectiveText.text += $"\n{checkValue} Get to the roof to escape!";
        }

        float newAlpha = m_canvasGroup.alpha - Time.deltaTime * m_fadeSpeed;
        newAlpha = Mathf.Clamp(newAlpha, m_alphaMin, 1.0f);
        m_canvasGroup.alpha = newAlpha;
    }

    public void Appear()
    {
        m_canvasGroup.alpha = 1.0f;
    }
}
