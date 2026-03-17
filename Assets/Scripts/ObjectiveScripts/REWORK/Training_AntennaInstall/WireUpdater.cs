using UnityEngine;

public class WireUpdater : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    void LateUpdate()
    {
        if (line == null || pointA == null || pointB == null) return;

        line.SetPosition(0, pointA.position);
        line.SetPosition(1, pointB.position);
    }
}