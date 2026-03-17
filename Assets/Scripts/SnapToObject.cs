using UnityEngine;

public class SnapToObject : MonoBehaviour
{
    public Transform targetObject;

    void Start()
    {
        if (targetObject != null)
        {
            transform.position = targetObject.position;
            transform.rotation = targetObject.rotation;
        }
    }
}
