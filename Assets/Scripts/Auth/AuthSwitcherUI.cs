using UnityEngine;
using UnityEngine.UI;

public class AuthSwitcherUI : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject registerPanel;
    public Button goToRegisterButton;
    public Button goToLoginButton;

    void Start()
    {
        ShowLogin();
        goToRegisterButton.onClick.AddListener(ShowRegister);
        goToLoginButton.onClick.AddListener(ShowLogin);
    }

    public void ShowLogin()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
    }

    public void ShowRegister()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
    }
}