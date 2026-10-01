using UnityEngine;

public class ShockDialController : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] private Transform dial;    // red knob
    [SerializeField] private Transform needle;  // pin on the right gauge

    [Header("Rotation")]
    [SerializeField] private Vector3 dialAxis = Vector3.forward;
    [SerializeField] private Vector3 needleAxis = Vector3.forward;
    [SerializeField] private float dialMaxAngle = 270f;
    [SerializeField] private float needleMinAngle = -60f;
    [SerializeField] private float needleMaxAngle = 60f;
    [SerializeField] private float turnSpeed = 0.3f; // fraction of full range per second

    private Quaternion dialStart, needleStart;
    private bool active;

    public bool IsActive => active;

    public float Value { get; private set; }  // 0 to 1, use this for shock level later

    private void Awake()
    {
        dialStart = dial.localRotation;
        needleStart = needle.localRotation;
    }

    public void SetActive(bool state) => active = state;

    private void Update()
    {
        if (!active) return;

        float input = Input.GetAxisRaw("Horizontal"); // A = -1, D = +1
        Value = Mathf.Clamp01(Value + input * turnSpeed * Time.deltaTime);

        dial.localRotation = dialStart * Quaternion.AngleAxis(Value * dialMaxAngle, dialAxis);
        needle.localRotation = needleStart * Quaternion.AngleAxis(
            Mathf.Lerp(needleMinAngle, needleMaxAngle, Value), needleAxis);
    }
}