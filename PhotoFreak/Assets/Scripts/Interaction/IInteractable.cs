using UnityEngine; 

public interface IInteractable
{
    string promptText { get; } 
    Transform promptLocation { get; }
    void Interact(); 
}