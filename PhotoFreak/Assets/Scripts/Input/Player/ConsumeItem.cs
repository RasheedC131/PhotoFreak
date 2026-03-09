using UnityEngine;

public class DrinkItem : MonoBehaviour, IEquippable
{
    private Rigidbody rb;
    private Collider col;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void OnUse()
    {
        Debug.Log("Sipping drink");
    }

    public void OnEquip()
    {
        gameObject.SetActive(true);
    }

    public void OnUnequip()
    {
        gameObject.SetActive(false);
    }

    public void OnPickup()
    {
        if (rb) rb.isKinematic = true;
        if (col) col.enabled = false;
    }

    public void OnDrop()
    {
        if (rb) rb.isKinematic = false;
        if (col) col.enabled = true;
        gameObject.SetActive(true); // Ensure it's visible when dropped
    }
}