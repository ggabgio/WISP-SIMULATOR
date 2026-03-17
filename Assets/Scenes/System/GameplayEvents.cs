// GameplayEvents.cs
using UnityEngine;
using System;

public static class GameplayEvents
{
    // Fired whenever an object is placed into the world
    public static event Action<GameObject> ObjectPlaced;
    public static System.Action<GameObject> ItemEquipped;

    // Helper to raise the event (only this class can invoke the event)
    public static void RaiseObjectPlaced(GameObject placedObject)
    {
        ObjectPlaced?.Invoke(placedObject);
    }
}