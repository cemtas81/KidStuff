using Firebase.Auth;
using UnityEngine;
using System;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }
    private FirebaseAuth auth;
    public FirebaseUser CurrentUser => auth?.CurrentUser;
    public event Action<FirebaseUser> OnUserLoggedIn;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            if (task.Result == Firebase.DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                if (auth.CurrentUser != null)
                {
                    // Sadece event tetikle. Email tabanlý yükleme/oluþturma YOK.
                    OnUserLoggedIn?.Invoke(auth.CurrentUser);
                }
            }
            else
            {
                Debug.LogError("Firebase Auth initialization failed: " + task.Result);
            }
        });
    }

    public void Register(string email, string password, Action<FirebaseUser, string> callback)
    {
        if (auth == null)
        {
            callback?.Invoke(null, "Firebase Auth not ready");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                callback?.Invoke(null, task.Exception?.Message ?? "Bilinmeyen hata");
                return;
            }
            var user = task.Result.User;
            OnUserLoggedIn?.Invoke(user); // Sadece event
            callback?.Invoke(user, null);
        });
    }

    public void Login(string email, string password, Action<FirebaseUser, string> callback)
    {
        if (auth == null)
        {
            callback?.Invoke(null, "Firebase Auth not ready");
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                callback?.Invoke(null, task.Exception?.Message ?? "Bilinmeyen hata");
                return;
            }
            var user = task.Result.User;
            OnUserLoggedIn?.Invoke(user); // Sadece event
            callback?.Invoke(user, null);
        });
    }

    public void Logout()
    {
        auth?.SignOut();
        if (ParentChildDataManager.Instance != null)
        {
            ParentChildDataManager.Instance.ClearCurrentData();
        }
    }
}