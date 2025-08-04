using UnityEngine;
using System.Collections.Generic;

public class GlobalProfileData : MonoBehaviour
{
    public static GlobalProfileData Instance { get; private set; }
    public ParentData CurrentParent { get; set; }
    public List<ChildData> CurrentChildren { get; set; }
    public List<ActivityData> Activities { get; set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}