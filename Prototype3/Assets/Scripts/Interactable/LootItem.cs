using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootItem : Interactable
{
    public float m_lootValue = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Interact()
    {
        Debug.Log("Activate");
        m_interactFunction.Invoke();

        GameManager.instance.lootValue += m_lootValue;
        Destroy(gameObject);
    }
}
