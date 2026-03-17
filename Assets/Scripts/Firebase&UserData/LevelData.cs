using Firebase.Firestore;
using System.Collections.Generic;

[FirestoreData]
public class LevelData
{
    [FirestoreProperty] public bool isCompleted { get; set; }
    [FirestoreProperty] public List<AttemptData> attempts { get; set; }

    public LevelData()
    {
        isCompleted = false;
        attempts = new List<AttemptData>();
    }
}