using System.Collections.Generic;
using UnityEngine;

public class ParentChildDataManager : MonoBehaviour
{
    public static ParentChildDataManager Instance { get; private set; }

    private FirestoreManager firestoreManager;
    private ParentData _currentParent;
    private ChildData _currentChild;

    // Public properties for global access
    public ParentData CurrentParent
    {
        get { return _currentParent; }
        set { _currentParent = value; }
    }

    public ChildData CurrentChild
    {
        get { return _currentChild; }
        set { _currentChild = value; }
    }

    void Awake()
    {
        // Singleton pattern for global access (optional but recommended)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        firestoreManager = FirestoreManager.Instance;
       
    }

    // Start or reset current parent
    public void StartNewParent()
    {
        _currentParent = new ParentData
        {
            Children = new List<ChildData>()
        };
    }

    public void SetParentName(string name)
    {
        if (_currentParent == null) StartNewParent();
        _currentParent.Name = name;
        SaveParent(); // Save immediately after setting name
    }

    public void SetParentAge(int age)
    {
        if (_currentParent == null) StartNewParent();
        _currentParent.Age = age;
        SaveParent(); // Save immediately after setting age
    }

    public void SetParentGender(int gender)
    {
        if (_currentParent == null) StartNewParent();
        _currentParent.Gender = gender;
        Debug.Log($"Parent gender set to: {gender}");
        SaveParent();
    }

    // Save parent to Firestore
    public void SaveParent()
    {
        if (!string.IsNullOrEmpty(_currentParent?.Name))
        {
            firestoreManager.AddParent(_currentParent);
        }
    }

    // Child methods
    public void StartNewChild()
    {
        _currentChild = new ChildData
        {
            Hobbies = new List<string>()
        };
    }

    public void SetChildName(string name)
    {
        if (_currentChild == null) StartNewChild();
        _currentChild.Name = name;
        SaveChildToParent(); // Save immediately after setting name
    }

    public void SetChildAge(int age)
    {
        if (_currentChild == null) StartNewChild();
        _currentChild.Age = age;
        SaveChildToParent(); // Save immediately after setting age
    }

    public void SetChildGender(int gender)
    {
        if (_currentChild == null) StartNewChild();
        _currentChild.Gender = gender;
        Debug.Log($"Child gender set to: {gender}");
        SaveChildToParent();
    }

    public void SetChildHobbies(List<string> hobbies)
    {
        if (_currentChild == null) StartNewChild();
        _currentChild.Hobbies = hobbies;
        SaveChildToParent(); // Save immediately after setting hobbies
    }

    // Save child under current parent in Firestore
    public void SaveChildToParent()
    {
        if (!string.IsNullOrEmpty(_currentParent?.Name) && !string.IsNullOrEmpty(_currentChild?.Name))
        {
            firestoreManager.AddChildToParent(_currentParent.Name, _currentChild);
            _currentParent.Children.Add(_currentChild);
            //_currentChild = null;
        }
    }

    // Optionally, retrieve all children for a parent
    public void GetChildrenOfParent(string parentName, System.Action<List<ChildData>> callback)
    {
        firestoreManager.GetChildrenOfParent(parentName, callback);
    }

    public void LoadParentFromFirestore(string parentName, System.Action onLoaded = null)
    {
        firestoreManager.GetParent(parentName, parentData =>
        {
            _currentParent = parentData;
            onLoaded?.Invoke();
        });
    }

    public void LoadChildFromFirestore(string parentName, string childName, System.Action onLoaded = null)
    {
        firestoreManager.GetChildOfParent(parentName, childName, childData =>
        {
            _currentChild = childData;
            onLoaded?.Invoke();
        });
    }
}