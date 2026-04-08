using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [SerializeField] private bool isOpen = false;
    [SerializeField] private float openAngle = 90f; 
    [SerializeField] private float swingSpeed = 5f; 
    [SerializeField] private Transform handleLocation; 

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine swingCoroutine;

    public string promptText => isOpen ? "Close Door" : "Open Door";
    public Transform promptLocation => handleLocation; 

    private void Awake()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
        if (isOpen) transform.rotation = openRotation;
    }

    public void Interact()
    {
        isOpen = !isOpen;

        if (swingCoroutine != null)
        {
            StopCoroutine(swingCoroutine);
        }

        swingCoroutine = StartCoroutine(SwingDoor(isOpen ? openRotation : closedRotation));
    }

    private IEnumerator SwingDoor(Quaternion targetRotation)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * swingSpeed);
            yield return null; // Wait for the next frame
        }
        transform.rotation = targetRotation;
    }
}