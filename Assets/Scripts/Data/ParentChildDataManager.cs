using System.Collections.Generic;
using System.Collections;
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
        // Singleton pattern for global access
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // FirestoreManager'ýn hazýr olmasýný bekle
        StartCoroutine(InitializeFirestoreManager());
    }

    IEnumerator InitializeFirestoreManager()
    {
        // FirestoreManager'ýn instance'ýnýn hazýr olmasýný bekle
        while (FirestoreManager.Instance == null)
        {
            Debug.LogWarning("Waiting for FirestoreManager.Instance...");
            yield return new WaitForSeconds(0.1f);
        }
        
        firestoreManager = FirestoreManager.Instance;
        Debug.Log("FirestoreManager initialized in ParentChildDataManager");
    }

    // FirestoreManager'ýn hazýr olup olmadýðýný kontrol et
    private bool IsFirestoreReady()
    {
        if (firestoreManager == null)
        {
            Debug.LogWarning("FirestoreManager is not ready yet. Operation skipped.");
            return false;
        }
        return true;
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
        // Null kontrollerini ekle
        if (_currentParent == null)
        {
            Debug.LogWarning("Cannot save parent: _currentParent is null");
            return;
        }

        if (string.IsNullOrEmpty(_currentParent.Name))
        {
            Debug.LogWarning("Cannot save parent: Parent name is empty");
            return;
        }

        if (!IsFirestoreReady())
        {
            Debug.LogWarning("Cannot save parent: FirestoreManager not ready");
            return;
        }

        try
        {
            firestoreManager.AddParent(_currentParent);
            Debug.Log($"Parent saved: {_currentParent.Name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving parent: {e.Message}");
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
        if (_currentParent == null)
        {
            Debug.LogWarning("Cannot save child: _currentParent is null");
            return;
        }

        if (_currentChild == null)
        {
            Debug.LogWarning("Cannot save child: _currentChild is null");
            return;
        }

        if (string.IsNullOrEmpty(_currentParent.Name) || string.IsNullOrEmpty(_currentChild.Name))
        {
            Debug.LogWarning("Cannot save child: Parent or Child name is empty");
            return;
        }

        if (!IsFirestoreReady())
        {
            Debug.LogWarning("Cannot save child: FirestoreManager not ready");
            return;
        }

        try
        {
            firestoreManager.AddChildToParent(_currentParent.Name, _currentChild);
            
            // Parent'ýn children listesine ekle (eðer yoksa)
            if (_currentParent.Children == null)
                _currentParent.Children = new List<ChildData>();
                
            // Ayný isimde child varsa güncelle, yoksa ekle
            var existingChild = _currentParent.Children.Find(c => c.Name == _currentChild.Name);
            if (existingChild == null)
            {
                _currentParent.Children.Add(_currentChild);
            }
            else
            {
                var index = _currentParent.Children.IndexOf(existingChild);
                _currentParent.Children[index] = _currentChild;
            }
            
            Debug.Log($"Child saved: {_currentChild.Name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving child: {e.Message}");
        }
    }

    // Optionally, retrieve all children for a parent
    public void GetChildrenOfParent(string parentName, System.Action<List<ChildData>> callback)
    {
        if (!IsFirestoreReady())
        {
            callback?.Invoke(new List<ChildData>());
            return;
        }

        firestoreManager.GetChildrenOfParent(parentName, callback);
    }

    public void LoadParentFromFirestore(string parentName, System.Action onLoaded = null)
    {
        if (!IsFirestoreReady())
        {
            Debug.LogWarning("Cannot load parent: FirestoreManager not ready");
            onLoaded?.Invoke();
            return;
        }

        firestoreManager.GetParent(parentName, parentData =>
        {
            _currentParent = parentData;
            Debug.Log(parentData != null ? $"Parent loaded: {parentData.Name}" : "Parent not found");
            onLoaded?.Invoke();
        });
    }

    public void LoadChildFromFirestore(string parentName, string childName, System.Action onLoaded = null)
    {
        if (!IsFirestoreReady())
        {
            Debug.LogWarning("Cannot load child: FirestoreManager not ready");
            onLoaded?.Invoke();
            return;
        }

        firestoreManager.GetChildOfParent(parentName, childName, childData =>
        {
            _currentChild = childData;
            Debug.Log(childData != null ? $"Child loaded: {childData.Name}" : "Child not found");
            onLoaded?.Invoke();
        });
    }
}