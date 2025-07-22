using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using TMPro;

public class ParentRegisterUI : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public Button registerButton;
    public TMP_Text feedbackText;
    public AuthSwitcherUI authSwitcher; // Assign in inspector

    private FirebaseAuth auth;
    private bool firebaseReady = false;

    void Start()
    {
        feedbackText.text = "";
        registerButton.onClick.AddListener(OnRegisterClicked);

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

    void OnRegisterClicked()
    {
        if (!firebaseReady)
        {
            feedbackText.text = "Firebase is not ready. Please wait...";
            return;
        }

        feedbackText.text = "";

        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            feedbackText.text = "Email and password are required.";
            return;
        }
        if (password != confirmPassword)
        {
            feedbackText.text = "Passwords do not match!";
            return;
        }

        registerButton.interactable = false;

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            registerButton.interactable = true;

            if (task.IsCanceled || task.IsFaulted)
            {
                feedbackText.text = "Registration error: " + (task.Exception != null ? task.Exception.Message : "Unknown error");
                return;
            }
            var authResult = task.Result;
            FirebaseUser newUser = authResult?.User;
            if (newUser == null)
            {
                feedbackText.text = "Registration error: User not created.";
                return;
            }
            newUser.SendEmailVerificationAsync().ContinueWith(verifyTask =>
            {
                if (verifyTask.IsCanceled || verifyTask.IsFaulted)
                {
                    feedbackText.text = "Could not send verification email: " + (verifyTask.Exception != null ? verifyTask.Exception.Message : "Unknown error");
                    return;
                }
                feedbackText.text = "Registration successful! Please check your email for a verification link.";
                // Switch to login panel right after registration
                authSwitcher.ShowLogin();
            }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
        }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
    }
}