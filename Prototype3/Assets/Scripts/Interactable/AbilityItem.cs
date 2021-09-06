using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AbilitySlot
{
    head,
    arm,
    legs,
}
public class AbilityItem : Interactable
{
    public Sprite m_imageIcon; 

    [Header("Ability")]
    [Range(1,3)]
    public int m_slot = 1; // I am entirely aware of how scuffed this is but it should work probably.

    [ShowIf("m_slot", (int)AbilitySlot.head + 1)]
    public HeadAbility m_headAbility;
    [ShowIf("m_slot", (int)AbilitySlot.arm + 1)]
    public ArmAbility m_armAbility;
    [ShowIf("m_slot", (int)AbilitySlot.legs + 1)]
    public LegsAbility m_legsAbility;

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

        switch ((AbilitySlot)m_slot - 1)
        {
            case AbilitySlot.head:
                FindObjectOfType<PlayerMovement>().SetHeadAbility(m_headAbility);
                FindObjectOfType<UI_Abilities>().SetHeadSprite(m_imageIcon);
                break;
            case AbilitySlot.arm:
                FindObjectOfType<PlayerMovement>().SetArmAbility(m_armAbility);
                FindObjectOfType<UI_Abilities>().SetArmSprite(m_imageIcon);
                break;
            case AbilitySlot.legs:
                FindObjectOfType<PlayerMovement>().SetLegsAbility(m_legsAbility);
                FindObjectOfType<UI_Abilities>().SetLegsSprite(m_imageIcon);
                break;
        }
        Destroy(gameObject);
    }
}
