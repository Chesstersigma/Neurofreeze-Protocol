using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public void OnInteract()
    {
        // Your door logic here, e.g., open or close the door
        Debug.Log("Door interacted!");
    }
}