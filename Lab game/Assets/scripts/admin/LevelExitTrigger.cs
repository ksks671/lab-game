using UnityEngine;
using System.Collections;

public class LevelExitTrigger : MonoBehaviour
{
    [SerializeField] private LevelLoader loader;
    [SerializeField] private float delay = 0f;

    private bool used;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"LevelExitTrigger: '{other.name}' entered (tag: {other.tag})", this);

        if (used) return;
        if (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player")) return;

        used = true;
        StartCoroutine(GoAfterDelay());
    }

    private IEnumerator GoAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        loader.LoadNextLevel();
    }
}