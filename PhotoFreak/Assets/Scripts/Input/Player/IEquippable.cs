using UnityEngine;

// allows for swapping of different items 
public interface IEquippable
{
    void OnEquip(); 
    void OnUnequip(); 
    void OnUse(); 
    GameObject gameObject { get; }
}
