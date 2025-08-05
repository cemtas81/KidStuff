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
        
        auth = FirebaseAuth.DefaultInstance;
        // Mevcut oturum kontrolü
        if (auth.CurrentUser != null)
        {
            OnUserLoggedIn?.Invoke(auth.CurrentUser);
        }
    }

    public void Register(string email, string password, Action<FirebaseUser, string> callback)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                callback?.Invoke(null, task.Exception?.Message ?? "Bilinmeyen hata");
                return;
            }
            callback?.Invoke(task.Result.User, null);
        });
    }

    public void Login(string email, string password, Action<FirebaseUser, string> callback)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                callback?.Invoke(null, task.Exception?.Message ?? "Bilinmeyen hata");
                return;
            }
            var user = task.Result.User;
            OnUserLoggedIn?.Invoke(user);
            callback?.Invoke(user, null);
        });
    }
    
    public void Logout()
    {
        auth.SignOut();
    }
}