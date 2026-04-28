using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ConsumableItem : MonoBehaviour, IEquippable
{
    protected Rigidbody rb;
    protected Collider col;
    private int originalLayer; 

    [Header("Item Details")]
    [SerializeField] protected string _itemName = "Default Item";
    [SerializeField] protected Sprite icon; 

    public virtual Sprite itemIcon => icon; 
    public virtual string itemName => _itemName; 

    [Header("Equip Positioning")]
    [SerializeField] protected Vector3 equipPositionOffset = Vector3.zero;
    [SerializeField] protected Vector3 equipRotationOffset = Vector3.zero;

    public bool isDroppable => true; 
    public virtual bool isInUse => false; 

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        originalLayer = gameObject.layer; 
    }

    public virtual void OnEquip() => gameObject.SetActive(true);
    public virtual void OnUnequip() => gameObject.SetActive(false);
    
    public virtual void OnUse() 
    {
        Debug.Log($"Using item: {itemName}");
    }

    public virtual void OnPickup(Transform holdParent)
    {
        rb.isKinematic = true;
        col.enabled = false;
        
        transform.SetParent(holdParent);
        transform.localPosition = equipPositionOffset;
        transform.localRotation = Quaternion.Euler(equipRotationOffset);
        
        SetLayerRecursively(gameObject, 7); 
    }

    public virtual void OnDrop()
    {
        transform.SetParent(null); 
        rb.isKinematic = false;
        col.enabled = true;
        gameObject.SetActive(true); 
        SetLayerRecursively(gameObject, originalLayer); 
    }

    protected void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}