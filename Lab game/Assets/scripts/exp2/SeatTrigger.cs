using UnityEngine;
using System.Collections;

public class SeatTrigger : MonoBehaviour
{
    [SerializeField] private GameObject seatCamera;      // the disabled Cinemachine Camera
    [SerializeField] private Behaviour[] disableOnSeat;  // player movement/look scripts
    [SerializeField] private GameObject hideOnSeat;      // player capsule model (optional)
    [SerializeField] private ShockDialController dial;
    [SerializeField] private float blendTime = 2f;       // match the Brain's Default Blend

    private bool used;

    private void OnTriggerEnter(Collider other)
    {
        if (used || !other.CompareTag("Player")) return;
        used = true;
        StartCoroutine(SitDown());
    }

    private IEnumerator SitDown()
    {
        foreach (var b in disableOnSeat) b.enabled = false;
        seatCamera.SetActive(true);   // Brain blends to it automatically
        yield return new WaitForSeconds(blendTime);
        if (hideOnSeat != null) hideOnSeat.SetActive(false);
        dial.SetActive(true);
    }
}