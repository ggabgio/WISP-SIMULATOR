using UnityEngine;

public class UserSession : MonoBehaviour
{
    public static UserSession Instance;

    public string username;
    public int userId;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }
}
