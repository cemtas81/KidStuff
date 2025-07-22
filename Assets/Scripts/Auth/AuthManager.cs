using Firebase.Auth;
using UnityEngine;
public class AuthManager : MonoBehaviour
{
    public void Register(string email, string password)
    {
        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(email, password);
    }
    public void Login(string email, string password)
    {
        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(email, password);
    }
}