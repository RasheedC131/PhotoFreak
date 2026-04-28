using System.Collections;
using UnityEngine;

// Requires "Is Trigger" to be ticked on the door panel's Box Collider.
// Any NPC (AIContext) or Player that enters opens the door automatically.
// The door closes after closeDelay seconds once the last occupant exits.
//
// NavMesh note: because the collider is a trigger it no longer physically
// blocks agents, so they can walk through regardless of door state. Bake
// your NavMesh with the doorway open so agents always have a valid path.
public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [SerializeField] private bool isOpen = false;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float swingSpeed = 5f;
    [SerializeField] private Transform handleLocation;

    [Header("Auto-Open for NPCs / Player")]
    [Tooltip("Seconds to wait after the last occupant leaves before closing.")]
    [SerializeField] private float closeDelay = 1.5f;

    [Header("Double Door")]
    [Tooltip("Assign the other Door here to make both panels open and close together.")]
    [SerializeField] private Door linkedDoor;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine swingCoroutine;
    private Coroutine closeCoroutine;
    private int occupantCount = 0;

    public bool IsOpen => isOpen;
    public string promptText => isOpen ? "Close Door" : "Open Door";
    public Transform promptLocation => handleLocation;

    private void Awake()
    {
        closedRotation = transform.rotation;
        openRotation   = closedRotation * Quaternion.Euler(0, openAngle, 0);
        if (isOpen) transform.rotation = openRotation;
    }

    // ── Player interaction ────────────────────────────────────────────────────

    public void Interact()
    {
        if (isOpen) CloseDoor();
        else        OpenDoor();
    }

    // ── Auto-open trigger ─────────────────────────────────────────────────────

    // Called directly when the collider is on this GameObject,
    // or forwarded from DoorTriggerRelay when the collider is on a child.
    public void NotifyTriggerEnter(Collider other)
    {
        if (!IsNPC(other)) return;

        occupantCount++;

        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }

        OpenDoor();
    }

    public void NotifyTriggerExit(Collider other)
    {
        if (!IsNPC(other)) return;

        occupantCount = Mathf.Max(0, occupantCount - 1);

        if (occupantCount == 0)
            closeCoroutine = StartCoroutine(CloseAfterDelay());
    }

    private void OnTriggerEnter(Collider other) => NotifyTriggerEnter(other);
    private void OnTriggerExit(Collider other)  => NotifyTriggerExit(other);

    private bool IsNPC(Collider other)
    {
        return other.GetComponentInParent<AIContext>() != null;
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);
        CloseDoor();
        closeCoroutine = null;
    }

    // ── Door movement ─────────────────────────────────────────────────────────

    // notifyLinked = false is used internally to prevent the two doors
    // calling each other in an infinite loop.
    public void OpenDoor(bool notifyLinked = true)
    {
        if (isOpen) return;
        isOpen = true;
        SwingTo(openRotation);
        if (notifyLinked && linkedDoor != null) linkedDoor.OpenDoor(false);
    }

    public void CloseDoor(bool notifyLinked = true)
    {
        if (!isOpen) return;
        isOpen = false;
        SwingTo(closedRotation);
        if (notifyLinked && linkedDoor != null) linkedDoor.CloseDoor(false);
    }

    private void SwingTo(Quaternion target)
    {
        if (swingCoroutine != null) StopCoroutine(swingCoroutine);
        swingCoroutine = StartCoroutine(SwingDoor(target));
    }

    private IEnumerator SwingDoor(Quaternion target)
    {
        while (Quaternion.Angle(transform.rotation, target) > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * swingSpeed);
            yield return null;
        }
        transform.rotation = target;
        swingCoroutine = null;
    }
}