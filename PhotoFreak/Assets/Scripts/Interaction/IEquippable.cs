using UnityEngine;

// allows for swapping of different items 
public interface IEquippable
{
    void OnEquip(); 
    void OnUnequip(); 
    void OnUse(); 
    void OnPickup(Transform holdParent); 
    void OnDrop(); 
    bool isDroppable {get; }
    bool isInUse {get; }
    Sprite itemIcon { get; }
    GameObject gameObject { get; }
}
