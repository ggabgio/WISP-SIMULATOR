using Firebase.Firestore;

[FirestoreData]
public class AttemptData
{
    [FirestoreProperty] public float performanceScore { get; set; }
    [FirestoreProperty] public long timestamp { get; set; } // UNIX timestamp

    // Add other data points you might want to track here in the future
    // [FirestoreProperty] public float timeTaken { get; set; }
    // [FirestoreProperty] public int errorsMade { get; set; }

    public AttemptData() { }

    public AttemptData(float score, long time)
    {
        performanceScore = score;
        timestamp = time;
    }
}