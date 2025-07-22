using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using TMPro;

public class ParentRegisterUI : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public Button registerButton;
    public TMP_Text feedbackText;

    private FirebaseAuth auth;

    public AuthSwitcherUI authSwitcher;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        registerButton.onClick.AddListener(OnRegisterClicked);
    }

    void OnRegisterClicked()
    {
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

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
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
                authSwitcher.ShowLogin(); // Switch to login panel
            }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
        }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
    }
}