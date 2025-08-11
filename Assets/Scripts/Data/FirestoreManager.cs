using Firebase.Firestore;
using System;
using System.Collections.Generic;
using UnityEngine;

[FirestoreData]
public class ChildData
{
    [FirestoreProperty]
    public string Name { get; set; }
    [FirestoreProperty]
    public int Age { get; set; }
    [FirestoreProperty]
    public int Gender { get; set; }
    [FirestoreProperty]
    public List<string> Hobbies { get; set; }
    [FirestoreProperty]
    public bool HasScreenAddiction { get; set; }
    [FirestoreProperty]
    public int ScreenUsageFrequency { get; set; } // 0: az, 1: normal, 2: çok
    [FirestoreProperty]
    public bool UsesScreenDuringMeals { get; set; }
    [FirestoreProperty]
    public int DailyPlayTime { get; set; } // dakika cinsinden
    [FirestoreProperty]
    public bool HasCameraPermission { get; set; }
    [FirestoreProperty]
    public string PasswordHash { get; set; }
}

[FirestoreData]
public class ParentData
{
    [FirestoreProperty]
    public string Name { get; set; }
    [FirestoreProperty]
    public int Age { get; set; }
    [FirestoreProperty]
    public int Gender { get; set; }
    [FirestoreProperty]
    public int Relationship { get; set; } // 0: Anne, 1: Baba, 2: Veli, 3: Yasal Temsilci
    [FirestoreProperty]
    public List<ChildData> Children { get; set; }
    [FirestoreProperty]
    public string Email { get; set; }
}

[FirestoreData]
public class ActivityData
{
    [FirestoreProperty]
    public string Title { get; set; }
    [FirestoreProperty]
    public string Description { get; set; }
    [FirestoreProperty]
    public List<string> RequiredTools { get; set; }
    [FirestoreProperty]
    public List<string> RelatedHobbies { get; set; }
    [FirestoreProperty]
    public int DurationMinutes { get; set; }
    [FirestoreProperty]
    public int MinAge { get; set; }
    [FirestoreProperty]
    public int MaxAge { get; set; }
    [FirestoreProperty]
    public ActivityType Type { get; set; }
    [FirestoreProperty]
    public bool RequiresParent { get; set; }
    [FirestoreProperty]
    public bool RequiresCamera { get; set; }
    [FirestoreProperty]
    public int EnergyLevel { get; set; } // 0: Düþük, 1: Orta, 2: Yüksek
    [FirestoreProperty]
    public bool HasAudioGuide { get; set; }
    [FirestoreProperty]
    public bool HasVisualGuide { get; set; }
    [FirestoreProperty]
    public bool IsFreeActivity { get; set; }
    [FirestoreProperty]
    public bool CanProduceOutput { get; set; }
}

[FirestoreData]
public class ToolData
{
    [FirestoreProperty]
    public string Name { get; set; }
    [FirestoreProperty]
    public string Category { get; set; }
    [FirestoreProperty]
    public string ImageUrl { get; set; }
}

public enum ActivityType
{
    Creative = 0,
    Physical = 1,
    Musical = 2,
    Educational = 3,
    FreePlay = 4
}

public class FirestoreManager : MonoBehaviour
{
    public static FirestoreManager Instance { get; private set; }
    FirebaseFirestore db;
    private bool firebaseReady = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var status = task.Result;
            if (status == Firebase.DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                firebaseReady = true;
                Debug.Log("Firebase initialized successfully");
            }
            else
            {
                Debug.LogError("Firebase initialization failed: " + status);
            }
        });
    }

    // Parent operations
    public void AddParent(ParentData parent)
    {
        if (!firebaseReady) { Debug.LogWarning("Firebase not ready!"); return; }
        db.Collection("parents").Document(parent.Name).SetAsync(parent);
    }

    public void GetParent(string parentName, System.Action<ParentData> callback)
    {
        if (!firebaseReady) { callback?.Invoke(null); return; }
        
        db.Collection("parents").Document(parentName).GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                var doc = task.Result;
                if (doc.Exists)
                {
                    var parent = doc.ConvertTo<ParentData>();
                    callback?.Invoke(parent);
                }
                else
                {
                    callback?.Invoke(null);
                }
            }
            else
            {
                callback?.Invoke(null);
            }
        });
    }

    public void GetAllParents(System.Action<List<ParentData>> callback)
    {
        if (!firebaseReady)
        {
            Debug.LogWarning("Firebase not ready!");
            callback?.Invoke(new List<ParentData>());
            return;
        }

        db.Collection("parents").GetSnapshotAsync().ContinueWith(task =>
        {
            var parentsList = new List<ParentData>();
            if (task.IsCompleted && !task.IsFaulted)
            {
                var snapshot = task.Result;
                foreach (var doc in snapshot.Documents)
                {
                    if (doc.Exists)
                    {
                        var parent = doc.ConvertTo<ParentData>();
                        parentsList.Add(parent);
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to get parents from Firestore: " + task.Exception?.Message);
            }

            callback?.Invoke(parentsList);
        });
    }

    // Child operations
    public void AddChildToParent(string parentName, ChildData child)
    {
        if (!firebaseReady) { Debug.LogWarning("Firebase not ready!"); return; }
        db.Collection("parents").Document(parentName)
            .Collection("children").Document(child.Name).SetAsync(child);
    }

    public void GetChildrenOfParent(string parentName, Action<List<ChildData>> callback)
    {
        if (!firebaseReady) { callback?.Invoke(new List<ChildData>()); return; }
        
        db.Collection("parents").Document(parentName)
            .Collection("children").GetSnapshotAsync().ContinueWith(task =>
            {
                var childrenList = new List<ChildData>();
                if (task.IsCompleted && !task.IsFaulted)
                {
                    var snapshot = task.Result;
                    foreach (var doc in snapshot.Documents)
                    {
                        var child = doc.ConvertTo<ChildData>();
                        childrenList.Add(child);
                    }
                }
                callback?.Invoke(childrenList);
            });
    }

    public void GetChildOfParent(string parentName, string childName, System.Action<ChildData> callback)
    {
        if (!firebaseReady) { callback?.Invoke(null); return; }
        
        db.Collection("parents").Document(parentName)
            .Collection("children").Document(childName).GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                var doc = task.Result;
                if (doc.Exists)
                {
                    var child = doc.ConvertTo<ChildData>();
                    callback?.Invoke(child);
                }
                else
                {
                    callback?.Invoke(null);
                }
            }
            else
            {
                callback?.Invoke(null);
            }
        });
    }

    // Activity operations
    public void AddActivity(ActivityData activity)
    {
        if (!firebaseReady) { Debug.LogWarning("Firebase not ready!"); return; }
        var docRef = db.Collection("activities").Document();
        docRef.SetAsync(activity);
    }

    public void GetActivities(Action<List<ActivityData>> callback)
    {
        if (!firebaseReady) { callback?.Invoke(new List<ActivityData>()); return; }
        
        db.Collection("activities").GetSnapshotAsync().ContinueWith(task =>
        {
            var activityList = new List<ActivityData>();
            if (task.IsCompleted && !task.IsFaulted)
            {
                var snapshot = task.Result;
                foreach (var doc in snapshot.Documents)
                {
                    var activity = doc.ConvertTo<ActivityData>();
                    activityList.Add(activity);
                }
            }
            callback?.Invoke(activityList);
        });
    }

    public void GetActivitiesForChild(ChildData child, Action<List<ActivityData>> callback)
    {
        GetActivities(activities =>
        {
            var filteredActivities = new List<ActivityData>();
            foreach (var activity in activities)
            {
                // Age filtering
                if (child.Age >= activity.MinAge && child.Age <= activity.MaxAge)
                {
                    // Hobby matching
                    bool hasMatchingHobby = false;
                    if (child.Hobbies != null && activity.RelatedHobbies != null)
                    {
                        foreach (var hobby in child.Hobbies)
                        {
                            if (activity.RelatedHobbies.Contains(hobby))
                            {
                                hasMatchingHobby = true;
                                break;
                            }
                        }
                    }
                    
                    if (hasMatchingHobby || activity.RelatedHobbies == null || activity.RelatedHobbies.Count == 0)
                    {
                        filteredActivities.Add(activity);
                    }
                }
            }
            callback?.Invoke(filteredActivities);
        });
    }

    // Tool operations
    public void AddTool(ToolData tool)
    {
        if (!firebaseReady) { Debug.LogWarning("Firebase not ready!"); return; }
        var docRef = db.Collection("tools").Document();
        docRef.SetAsync(tool);
    }

    public void GetTools(Action<List<ToolData>> callback)
    {
        if (!firebaseReady) { callback?.Invoke(new List<ToolData>()); return; }
        
        db.Collection("tools").GetSnapshotAsync().ContinueWith(task =>
        {
            var toolList = new List<ToolData>();
            if (task.IsCompleted && !task.IsFaulted)
            {
                var snapshot = task.Result;
                foreach (var doc in snapshot.Documents)
                {
                    var tool = doc.ConvertTo<ToolData>();
                    toolList.Add(tool);
                }
            }
            callback?.Invoke(toolList);
        });
    }

    // Subscription operations
    public void GetUserSubscription(string userId, Action<SubscriptionData> callback)
    {
        if (!firebaseReady) { callback?.Invoke(null); return; }
        
        db.Collection("subscriptions").Document(userId).GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                var doc = task.Result;
                if (doc.Exists)
                {
                    var subscription = doc.ConvertTo<SubscriptionData>();
                    callback?.Invoke(subscription);
                }
                else
                {
                    callback?.Invoke(null);
                }
            }
            else
            {
                callback?.Invoke(null);
            }
        });
    }

    public void UpdateUserSubscription(string userId, SubscriptionData subscriptionData)
    {
        if (!firebaseReady) { Debug.LogWarning("Firebase not ready!"); return; }
        db.Collection("subscriptions").Document(userId).SetAsync(subscriptionData);
    }

    // Voucher operations
    public void ValidateVoucher(string voucherCode, Action<VoucherData> callback)
    {
        if (!firebaseReady) { callback?.Invoke(null); return; }
        
        db.Collection("vouchers").Document(voucherCode).GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                var doc = task.Result;
                if (doc.Exists)
                {
                    var voucher = doc.ConvertTo<VoucherData>();
                    callback?.Invoke(voucher);
                }
                else
                {
                    callback?.Invoke(null);
                }
            }
            else
            {
                callback?.Invoke(null);
            }
        });
    }

    public void MarkVoucherAsUsed(string voucherCode)
    {
        if (!firebaseReady) { Debug.LogWarning("Firebase not ready!"); return; }
        
        var updates = new Dictionary<string, object>
        {
            { "IsUsed", true }
        };
        
        db.Collection("vouchers").Document(voucherCode).UpdateAsync(updates);
    }
}