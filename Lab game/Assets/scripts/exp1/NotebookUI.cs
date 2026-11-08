using UnityEngine;
using TMPro;
using System;

// Put this on the Player (or any object that stays active - NOT on the notepad panel itself).
// The notepad shows the info of the last person you looked at. Looking at a wall keeps it showing.
public class NotebookUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject notebookPanel;     // the notepad panel (Image)
    public TMP_Text notebookText;        // the TMP text inside the panel

    [Header("Looking")]
    public Camera playerCamera;          // optional - falls back to Camera.main
    public LayerMask lookMask = ~0;      // which layers the ray can hit (Victim, or Everything)
    public float lookDistance = 100f;
    public float lookHoldTime = 1f;      // seconds you must look at someone before the notepad switches to them
    public bool aimWithMouse = true;     // true = ray from the cursor, false = from the screen centre
    public bool debugLogs = true;

    private PersonProfile displayed;     // whoever the notepad is currently showing
    private PersonProfile current;       // whoever you're looking at right now
    private float lookTimer;
    private string lastLoggedHit = "";

    private void Awake()
    {
        // A mask on an existing component can default to "Nothing", which hits nothing at all
        if (lookMask.value == 0)
        {
            Debug.LogWarning("NotebookUI: Look Mask was set to Nothing, using Everything instead.", this);
            lookMask = ~0;
        }
    }

    private void Start()
    {
        if (notebookPanel == null) Debug.LogError("NotebookUI: Notebook Panel is not assigned!", this);
        if (notebookText == null) Debug.LogError("NotebookUI: Notebook Text is not assigned!", this);

        if (notebookPanel != null) notebookPanel.SetActive(false);
        Refresh();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            if (notebookPanel != null)
                notebookPanel.SetActive(!notebookPanel.activeSelf);
            Refresh();
        }

        PersonProfile target = GetLookTarget();

        // 1) Hover: switch to someone after looking at them for lookHoldTime seconds
        if (target != null)
        {
            if (target != current)
            {
                current = target;
                lookTimer = 0f;
            }

            lookTimer += Time.deltaTime;

            if (lookTimer >= lookHoldTime)
                ShowPerson(target);
        }
        else
        {
            // Looking at a wall / nothing: keep showing the last person
            current = null;
            lookTimer = 0f;
        }

        // 2) Click: switch to someone instantly by clicking on them
        if (Input.GetMouseButtonDown(0) && target != null)
            ShowPerson(target);
    }

    // Returns the person under the ray, or null if the first thing hit isn't a person.
    private PersonProfile GetLookTarget()
    {
        Camera cam = playerCamera != null ? playerCamera : Camera.main;
        if (cam == null)
        {
            Debug.LogError("NotebookUI: no camera found (assign Player Camera or tag one MainCamera).", this);
            return null;
        }

        Ray ray = aimWithMouse
            ? cam.ScreenPointToRay(Input.mousePosition)
            : cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Debug.DrawRay(ray.origin, ray.direction * lookDistance, Color.yellow);

        RaycastHit[] hits = Physics.RaycastAll(ray, lookDistance, lookMask, QueryTriggerInteraction.Ignore);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            // Skip the player's own colliders so they can't block the ray
            if (hit.collider.transform.IsChildOf(transform))
                continue;

            if (debugLogs && hit.collider.name != lastLoggedHit)
            {
                lastLoggedHit = hit.collider.name;
                Debug.Log("NotebookUI ray hit: " + hit.collider.name);
            }

            return hit.collider.GetComponentInParent<PersonProfile>();
        }

        lastLoggedHit = "";
        return null;
    }

    private void ShowPerson(PersonProfile person)
    {
        person.discovered = true;   // kept in case you want to know who the player has seen

        if (person == displayed)
            return;

        displayed = person;
        Refresh();

        if (debugLogs)
            Debug.Log("NotebookUI: now showing " + person.personName);
    }

    private void Refresh()
    {
        if (notebookText == null)
            return;

        if (displayed == null)
        {
            notebookText.text = "Nothing noted yet...";
            return;
        }

        notebookText.text = "<b>" + displayed.personName + "</b>\n" + displayed.info;
    }
}