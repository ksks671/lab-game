using UnityEngine;

public class CageTrigger : MonoBehaviour
{
    [SerializeField] private ExperimentDirector director;
    private bool used;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"CageTrigger: '{other.name}' entered (tag: {other.tag})", this);

        if (used) return;
        if (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player")) return;

        used = true;
        Debug.Log("CageTrigger: player in cage, starting experiment.", this);
        director.StartExperiment();
    }
}