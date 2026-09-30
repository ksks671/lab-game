using UnityEngine;

public class LightTrigger : MonoBehaviour
{
    [SerializeField] private Light lightToEnable;

    private bool used;

    private void Start()
    {
        if (lightToEnable == null)
        {
            Debug.LogError("LightTrigger: Light To Enable is NOT assigned!", this);
            return;
        }

        lightToEnable.gameObject.SetActive(false);
        Debug.Log($"LightTrigger [{name}]: '{lightToEnable.name}' starts OFF.", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"LightTrigger [{name}]: '{other.name}' entered (tag: {other.tag})", this);

        if (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player")) return;

        if (used)
        {
            Debug.Log($"LightTrigger [{name}]: already used, ignoring.", this);
            return;
        }

        used = true;
        lightToEnable.gameObject.SetActive(true);
        lightToEnable.enabled = true;
        Debug.Log($"LightTrigger [{name}]: '{lightToEnable.name}' turned ON.", this);
    }
}