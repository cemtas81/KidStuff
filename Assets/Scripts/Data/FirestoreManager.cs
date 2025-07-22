using Firebase.Firestore;
using System;
using System.Collections.Generic;
using UnityEngine;

// Eksik veri sýnýflarýný ekliyoruz
[FirestoreData]
public class ChildData
{
    [FirestoreProperty]
    public string Name { get; set; }
    [FirestoreProperty]
    public int Age { get; set; }
    // Gerekirse baþka alanlar ekleyebilirsiniz
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
    FirebaseFirestore db;
    void Start() { db = FirebaseFirestore.DefaultInstance; }
    public void AddChild(ChildData child)
    {
        db.Collection("children").AddAsync(child);
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
}