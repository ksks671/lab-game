using UnityEngine;

public class TrapDoor : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string openTrigger = "Open";
    [SerializeField] private Collider floorCollider; // the part you stand on
    [SerializeField] private float colliderDelay = 0.2f;

    public void Open()
    {
        Debug.Log($"TrapDoor [{name}]: opening.", this);

        if (animator != null) animator.SetTrigger(openTrigger);
        else Debug.LogWarning($"TrapDoor [{name}]: no Animator assigned.", this);

        Invoke(nameof(DisableFloor), colliderDelay);
    }

    private void DisableFloor()
    {
        if (floorCollider != null)
        {
            floorCollider.enabled = false;
            Debug.Log($"TrapDoor [{name}]: floor collider off.", this);
        }
    }
}
