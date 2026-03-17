using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

[FirestoreData]
public class PlayerProfileData
{
    [FirestoreProperty] public string username { get; set; }
    [FirestoreProperty] public string userID { get; set; }
    [FirestoreProperty] public string email { get; set; }
    [FirestoreProperty] public string role { get; set; }
    [FirestoreProperty] public string firstName { get; set; }
    [FirestoreProperty] public string middleName { get; set; }
    [FirestoreProperty] public string lastName { get; set; }
    [FirestoreProperty] public int age { get; set; }
    [FirestoreProperty] public string birthdate { get; set; }

    [FirestoreProperty] public Dictionary<string, LevelData> trainingData { get; set; }
    [FirestoreProperty] public Dictionary<string, LevelData> quizData { get; set; }
    [FirestoreProperty] public Dictionary<string, LevelData> assessmentData { get; set; }
    [FirestoreProperty] public Dictionary<string, LevelData> maintenanceData { get; set; }

    public PlayerProfileData()
    {
         username = "Guest";
         userID = "";
         email = "";
         role = "user";
         firstName = "Guest";
         lastName = "";
         middleName = "";
         age = 0;
         trainingData = new Dictionary<string, LevelData>();
         quizData = new Dictionary<string, LevelData>();
         assessmentData = new Dictionary<string, LevelData>();
         maintenanceData = new Dictionary<string, LevelData>();
    }

     public PlayerProfileData(string uName, string fName, string mName, string lName, int userAge, string uid, string userEmail, string userRole)
     {
         username = uName;
         firstName = fName;
         middleName = mName;
         lastName = lName;
         age = userAge;
         userID = uid;
         email = userEmail;
         role = userRole;
         trainingData = new Dictionary<string, LevelData>();
         quizData = new Dictionary<string, LevelData>();
         assessmentData = new Dictionary<string, LevelData>();
         maintenanceData = new Dictionary<string, LevelData>();
     }
}

public class UserSessionData : MonoBehaviour
{
    public static UserSessionData Instance { get; private set; }
    public PlayerProfileData profileData { get; private set; }

    public FirebaseAuth firebaseAuth { get; private set; }
    public FirebaseFirestore firestoreDb { get; private set; }
    public FirebaseUser firebaseUser { get; private set; }

    private string saveFolderPath;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        profileData = new PlayerProfileData();
        saveFolderPath = Path.Combine(Application.persistentDataPath, "PlayerProfiles");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                firebaseAuth = FirebaseAuth.DefaultInstance;
                firestoreDb = FirebaseFirestore.DefaultInstance;
                firebaseAuth.StateChanged += AuthStateChanged;
                AuthStateChanged(this, System.EventArgs.Empty);
            } else {
                Debug.LogError($"Could not resolve all Firebase dependencies: {task.Result}");
                SetupGuestProfileLocally();
            }
        });
    }

    void OnDestroy()
    {
        if (firebaseAuth != null)
        {
            firebaseAuth.StateChanged -= AuthStateChanged;
        }
    }

    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (firebaseAuth.CurrentUser != firebaseUser)
        {
            firebaseUser = firebaseAuth.CurrentUser;
            if (firebaseUser != null)
            {
                LoadProfileDataFromFirestore(firebaseUser.UserId);
            }
            else
            {
                SetupGuestProfileLocally();
            }
        }
    }

    private void SetupGuestProfileLocally()
    {
         profileData = new PlayerProfileData();
    }

    public async Task UserLoggedIn(FirebaseUser user)
    {
        this.firebaseUser = user;
        if (firebaseAuth.CurrentUser == user)
        {
            await LoadProfileDataFromFirestore(user.UserId);
        }
    }

    public async Task GuestLoggedIn(FirebaseUser user)
    {
        this.firebaseUser = user;
        profileData = new PlayerProfileData { userID = user.UserId };
        if (firebaseAuth.CurrentUser == user)
        {
            await LoadProfileDataFromFirestore(user.UserId);
        }
    }
    
    private async Task UpdateLevelProgress(Dictionary<string, LevelData> dataDict, string levelId, bool completed, float score, string category)
    {
        if (firebaseUser == null || firestoreDb == null) return;
        if (string.IsNullOrEmpty(levelId)) return;

        DocumentReference docRef = firestoreDb.Collection("userProfiles").Document(firebaseUser.UserId);
        long currentTimestamp = System.DateTimeOffset.Now.ToUnixTimeSeconds();
        AttemptData newAttempt = new AttemptData(score, currentTimestamp);

        try
        {
            await firestoreDb.RunTransactionAsync(async transaction =>
            {
                DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(docRef);
                PlayerProfileData freshProfileData = snapshot.ConvertTo<PlayerProfileData>();

                var freshDataDict = category switch
                {
                    "Training" => freshProfileData.trainingData ?? new Dictionary<string, LevelData>(),
                    "Quiz" => freshProfileData.quizData ?? new Dictionary<string, LevelData>(),
                    "Assessment" => freshProfileData.assessmentData ?? new Dictionary<string, LevelData>(),
                    "Maintenance" => freshProfileData.maintenanceData ?? new Dictionary<string, LevelData>(),
                    _ => null
                };
                if (freshDataDict == null) return;

                if (!freshDataDict.ContainsKey(levelId))
                {
                    freshDataDict[levelId] = new LevelData();
                }
                freshDataDict[levelId].attempts.Add(newAttempt);
                if (completed)
                {
                    freshDataDict[levelId].isCompleted = true;
                }

                switch (category)
                {
                    case "Training": freshProfileData.trainingData = freshDataDict; break;
                    case "Quiz": freshProfileData.quizData = freshDataDict; break;
                    case "Assessment": freshProfileData.assessmentData = freshDataDict; break;
                    case "Maintenance": freshProfileData.maintenanceData = freshDataDict; break;
                }
                
                // Only update the level data fields, preserving user profile fields like birthdate
                var updateData = new Dictionary<string, object>();
                if (category == "Training") updateData["trainingData"] = freshProfileData.trainingData;
                if (category == "Quiz") updateData["quizData"] = freshProfileData.quizData;
                if (category == "Assessment") updateData["assessmentData"] = freshProfileData.assessmentData;
                if (category == "Maintenance") updateData["maintenanceData"] = freshProfileData.maintenanceData;
                
                transaction.Update(docRef, updateData);
                
                // Update local profile data
                this.profileData = freshProfileData; 
            });
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to update {category} progress for level {levelId}: {e.Message}");
        }
    }
    
    public async Task UpdateTrainingLevelProgress(string levelId, bool completed, float score)
    {
        await UpdateLevelProgress(profileData.trainingData, levelId, completed, score, "Training");
    }
    
    public async Task UpdateAssessmentLevelProgress(string levelId, bool completed, float score)
    {
        await UpdateLevelProgress(profileData.assessmentData, levelId, completed, score, "Assessment");
    }

    public async Task UpdateQuizLevelProgress(string levelId, bool completed, float score)
    {
        await UpdateLevelProgress(profileData.quizData, levelId, completed, score, "Quiz");
    }
    public async Task UpdateMaintenanceLevelProgress(string levelId, bool completed, float score)
    {
        await UpdateLevelProgress(profileData.maintenanceData, levelId, completed, score, "Maintenance");
    }

    public async void SaveProfileData()
    {
        if (firebaseUser == null || firestoreDb == null)
        {
            SaveProfileDataLocally();
            return;
        }
        if (profileData == null) return;
        
        profileData.userID = firebaseUser.UserId;
        DocumentReference docRef = firestoreDb.Collection("userProfiles").Document(firebaseUser.UserId);
        try
        {
            // Check if document exists before saving - don't recreate deleted documents
            DocumentSnapshot currentSnapshot = await docRef.GetSnapshotAsync();
            if (!currentSnapshot.Exists)
            {
                Debug.LogWarning($"Cannot save profile data: Document for UID {firebaseUser.UserId} does not exist in Firestore. " +
                    "Profile may have been deleted. Skipping Firestore save to prevent recreation.");
                // Still save locally as cache, but don't recreate in Firestore
                SaveProfileDataLocally();
                return;
            }
            
            // Document exists - safe to save
            var currentData = currentSnapshot.ConvertTo<PlayerProfileData>();
            // Preserve birthdate from Firestore if it exists there but not in our local data
            if (!string.IsNullOrEmpty(currentData.birthdate) && string.IsNullOrEmpty(profileData.birthdate))
            {
                profileData.birthdate = currentData.birthdate;
            }
            
            // Use SetAsync with MergeAll - this will merge fields but preserve birthdate if it exists in Firestore
            await docRef.SetAsync(profileData, SetOptions.MergeAll);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save profile data to Firestore: {e.Message}");
        }
        finally
        {
            SaveProfileDataLocally();
        }
    }

    public void SaveProfileDataLocally()
    {
        if (profileData == null || string.IsNullOrEmpty(profileData.username)) return;
        
        string saveFileName = $"{profileData.username}_data.json";
        if (!Directory.Exists(saveFolderPath)) Directory.CreateDirectory(saveFolderPath);
        string filePath = Path.Combine(saveFolderPath, saveFileName);
        
        try 
        {
            string json = JsonUtility.ToJson(profileData, true);
            File.WriteAllText(filePath, json);
        } 
        catch (System.Exception e) 
        {
            Debug.LogError($"Failed to save local profile data to {filePath}: {e.Message}");
        }
    }

    public async Task LoadProfileDataFromFirestore(string userId)
    {
        if (firestoreDb == null || string.IsNullOrEmpty(userId))
        {
            SetupGuestProfileLocally();
            return;
        }

        DocumentReference docRef = firestoreDb.Collection("userProfiles").Document(userId);
        try
        {
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                PlayerProfileData loadedData = snapshot.ConvertTo<PlayerProfileData>();
                profileData = loadedData;

                if (profileData.trainingData == null) profileData.trainingData = new Dictionary<string, LevelData>();
                if (profileData.quizData == null) profileData.quizData = new Dictionary<string, LevelData>();
                if (profileData.assessmentData == null) profileData.assessmentData = new Dictionary<string, LevelData>();
                if (profileData.maintenanceData == null) profileData.maintenanceData = new Dictionary<string, LevelData>();

                profileData.userID = firebaseUser?.UserId ?? userId;
                profileData.email = firebaseUser?.Email ?? profileData.email;
            }
            else
            {
                // Document doesn't exist - don't auto-create it
                // This prevents deleted documents from being recreated
                Debug.LogWarning($"User profile document not found for UID {userId}. Profile may have been deleted.");
                
                // Try loading from local cache as fallback (but don't save back to Firestore)
                if (!LoadProfileDataLocally(userId))
                {
                    // No local data either - set up guest profile locally only
                    SetupGuestProfileLocally();
                    Debug.LogWarning($"No local profile found for UID {userId}. Using guest profile.");
                }
                else
                {
                    Debug.Log($"Loaded cached profile from local storage for UID {userId}. Profile will not be saved back to Firestore.");
                }
                
                // DO NOT call CreateAndSaveNewFirestoreProfile here - that would recreate deleted documents
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading profile from Firestore for UID {userId}: {e.Message}");
            // On error, try local cache but don't auto-create in Firestore
            if (!LoadProfileDataLocally(userId))
            {
                SetupGuestProfileLocally();
            }
        }
    }

    private async Task CreateAndSaveNewFirestoreProfile(string userId)
    {
        string uName = $"User_{userId.Substring(0, 5)}";
        string fName = "New";
        string lName = "User";
        string mail = firebaseUser?.Email ?? "";

        profileData = new PlayerProfileData(uName, fName, "", lName, 0, userId, mail, "user");

        DocumentReference docRef = firestoreDb.Collection("userProfiles").Document(userId);
        try 
        {
            await docRef.SetAsync(profileData);
            SaveProfileDataLocally();
        } 
        catch (System.Exception ex) 
        {
            Debug.LogError($"Failed to save new Firestore profile for UID {userId}: {ex.Message}");
        }
    }

    public bool LoadProfileDataLocally(string usernameOrUID)
    {
        if (string.IsNullOrEmpty(usernameOrUID)) return false;
        string saveFileName = $"{usernameOrUID}_data.json";
        string filePath = Path.Combine(saveFolderPath, saveFileName);

        if (File.Exists(filePath)) 
        {
            try 
            {
                string json = File.ReadAllText(filePath);
                profileData = JsonUtility.FromJson<PlayerProfileData>(json);
                return true;
            } 
            catch (System.Exception e) 
            {
                Debug.LogError($"Failed to load local profile from {filePath}: {e.Message}");
                return false;
            }
        } 
        return false;
    }
}