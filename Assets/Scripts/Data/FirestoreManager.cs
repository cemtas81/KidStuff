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
    public string Gender { get; set; }
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
    public string Gender { get; set; }
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

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        db = FirebaseFirestore.DefaultInstance;
    }

    void Start() { db = FirebaseFirestore.DefaultInstance; }

    // Add a parent document
    public void AddParent(ParentData parent)
    {
        db.Collection("parents").Document(parent.Name).SetAsync(parent);
    }

    // Add a child to a parent's subcollection
    public void AddChildToParent(string parentName, ChildData child)
    {
        db.Collection("parents").Document(parentName)
            .Collection("children").Document(child.Name).SetAsync(child);
    }

    // Add child independently (not under a parent)
    public void AddChild(ChildData child)
    {
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
}