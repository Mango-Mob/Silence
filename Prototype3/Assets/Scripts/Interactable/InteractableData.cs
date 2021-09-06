using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Interactable", menuName = "ScriptableObjects/InteractableData", order = 1)]
public class InteractableData : ScriptableObject
{
    public string prefabName;
    public InteractableType type;

    public Sprite m_imageIcon; // Only required for abilities
    public AbilitySlot slot; // Only required for abilities
    public HeadAbility headAbility;
    public ArmAbility armAbility;
    public LegsAbility legsAbility;
}
public enum InteractableType
{
    ability,
    loot,
}