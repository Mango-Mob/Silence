using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class LootManager : MonoBehaviour
{
    [Range(0, 100)]
    public float m_spawnValuePercentage;
    [ReadOnly]
    public float m_totalValue = 0;
    [ReadOnly]
    public float m_currentValue = 0;

    private List<LootItem> m_items;
    // Start is called before the first frame update
    void Awake()
    {
        m_items = new List<LootItem>(GetComponentsInChildren<LootItem>());
        foreach (var item in m_items)
        {
            m_totalValue += item.m_lootValue;
        }
        m_currentValue = m_totalValue;
        float target = m_spawnValuePercentage / 100f * m_totalValue;

        while (m_currentValue > target)
        {
            int selected = Random.Range(0, m_items.Count);
            m_currentValue -= m_items[selected].m_lootValue;
            Destroy(m_items[selected].gameObject);
            m_items.RemoveAt(selected);
        };
    }
}
