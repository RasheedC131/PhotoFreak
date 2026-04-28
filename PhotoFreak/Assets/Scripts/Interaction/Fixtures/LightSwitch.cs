using UnityEngine;

public class LightSwitch : MonoBehaviour, IInteractable
{
    [Header("Switch Settings")]
    [SerializeField] private Light targetLight; 
    [SerializeField] private bool isOn = true;  
    public string promptText => isOn ? "Turn Off Light" : "Turn On Light";

    private void Start()
    {
        if (targetLight != null) targetLight.enabled = isOn;
    }

    public void Interact()
    {
        isOn = !isOn;
        if (targetLight != null)targetLight.enabled = isOn;
    }
}