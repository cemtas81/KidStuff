using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using TMPro;
using UnityEngine.SceneManagement;

public class ParentLoginUI : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public TMP_Text feedbackText;

    private FirebaseAuth auth;
    private bool firebaseReady = false;

    void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
        feedbackText.text = "";

        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                firebaseReady = true;
            }
            else
            {
                feedbackText.text = "Firebase initialization failed: " + status;
            }
        });
    }

    void OnLoginClicked()
    {
        feedbackText.text = "";

        if (!firebaseReady)
        {
            feedbackText.text = "Firebase is not ready. Please wait...";
            return;
        }

        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            feedbackText.text = "Email and password are required.";
            return;
        }

        loginButton.interactable = false;

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            loginButton.interactable = true;
            if (task.IsCanceled || task.IsFaulted)
            {
                var errorMsg = "Login error: ";
                if (task.Exception != null)
                {
                    foreach (var e in task.Exception.Flatten().InnerExceptions)
                    {
                        if (e is FirebaseException fe)
                            errorMsg += $"Firebase error code: {fe.ErrorCode}";
                        errorMsg += $" {e.Message}";
                    }
                }
                else
                    errorMsg += "Unknown error";
                ShowFeedback(errorMsg);
                return;
            }
            var authResult = task.Result;
            FirebaseUser user = authResult?.User;
            if (user == null)
            {
                ShowFeedback("Login error: User not found.");
                return;
            }
            if (!user.IsEmailVerified)
            {
                ShowFeedback("Please verify your email before logging in!");
                return;
            }
            // Login successful, proceed to ParentHome scene
            SceneManager.LoadScene(1);
        }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
    }

    void ShowFeedback(string msg)
    {
        feedbackText.text = msg;
    }
}