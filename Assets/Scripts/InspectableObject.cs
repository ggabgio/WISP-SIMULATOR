using System.Collections.Generic;
using UnityEngine;

public class InspectableObject : MonoBehaviour
{
    // Assign the child camera positioned for inspection
    public Camera inspectCamera;

    // Optional: assign a trigger collider on the same object (isTrigger = true)
    public Collider interactionTrigger;

    [HideInInspector]
    public bool playerInside;

    private static readonly List<InspectableObject> instances = new List<InspectableObject>();
    public static IReadOnlyList<InspectableObject> Instances => instances;

    void OnEnable()
    {
        if (!instances.Contains(this)) instances.Add(this);
        if (inspectCamera != null) inspectCamera.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        instances.Remove(this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInside = false;
    }
}


