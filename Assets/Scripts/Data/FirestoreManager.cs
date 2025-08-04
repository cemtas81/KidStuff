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
    public List<ChildData> Children { get; set; }
}

[FirestoreData]
public class ActivityData
{
    [FirestoreProperty]
    public string Title { get; set; }
    [FirestoreProperty]
    public string Description { get; set; }
    // could add more fields as needed
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
            }
            else
            {
                Debug.LogError("Firebase initialization failed: " + status);
            }
        });
    }


    // Add a parent document
    public void AddParent(ParentData parent)
    {
        if (!firebaseReady) { Debug.LogWarning("Firebase not ready!"); return; }
        db.Collection("parents").Document(parent.Name).SetAsync(parent);
    }

    // Add a child to a parent's subcollection
    public void AddChildToParent(string parentName, ChildData child)
    {
        if (!firebaseReady) { Debug.LogWarning("Firebase not ready!"); return; }
        db.Collection("parents").Document(parentName)
            .Collection("children").Document(child.Name).SetAsync(child);
    }

    // Add child independently (not under a parent)
    public void AddChild(ChildData child)
    {
        if (!firebaseReady) { Debug.LogWarning("Firebase not ready!"); return; }
        db.Collection("children").Document(child.Name).SetAsync(child);
    }

    public void GetActivities(Action<List<ActivityData>> callback)
    {
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

    // Get all children of a parent
    public void GetChildrenOfParent(string parentName, Action<List<ChildData>> callback)
    {
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

    public void GetParent(string parentName, System.Action<ParentData> callback)
    {
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

    public void GetChildOfParent(string parentName, string childName, System.Action<ChildData> callback)
    {
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
}