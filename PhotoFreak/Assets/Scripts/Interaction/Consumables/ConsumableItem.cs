using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ConsumableItem : MonoBehaviour, IEquippable
{
    private Rigidbody rb;
    private Collider col;

    public bool isDroppable => true; 
    public bool isInUse => true; 

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void OnEquip() => gameObject.SetActive(true);
    public void OnUnequip() => gameObject.SetActive(false);
    public void OnUse() => Debug.Log($"Using item: {gameObject.name}");

    public void OnPickup(Transform holdParent)
    {
        rb.isKinematic = true;
        col.enabled = false;
        
        transform.SetParent(holdParent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        SetLayerRecursively(gameObject, 7);
    }

    public void OnDrop()
    {
        transform.SetParent(null);
        rb.isKinematic = false;
        col.enabled = true;
        gameObject.SetActive(true); 
        SetLayerRecursively(gameObject, 0);
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}