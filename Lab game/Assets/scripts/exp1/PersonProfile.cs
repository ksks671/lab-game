using UnityEngine;

public class PersonProfile : MonoBehaviour
{
    public string personName;
    [TextArea(2, 5)] public string info;      // what the notepad shows
    public int track;                          // 0 = the single track, 1 = the group track
    [Tooltip("Optional: hidden 'big picture' value, e.g. a surgeon who saves thousands")]
    public bool helpsManyPeople;

    [HideInInspector] public bool discovered;
}