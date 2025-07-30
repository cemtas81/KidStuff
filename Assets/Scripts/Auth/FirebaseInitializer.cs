using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Firebase;

public class FirebaseInitializer : MonoBehaviour
{
    // Your working Netlify endpoint (no .js)
    private string apiEndpoint = "https://kiddosapp.netlify.app/.netlify/functions/googleApiKey";

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        StartCoroutine(InitFirebase());
    }

    IEnumerator InitFirebase()
    {
        UnityWebRequest req = UnityWebRequest.Get(apiEndpoint);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to fetch API key: " + req.error);
            yield break;
        }

        string json = req.downloadHandler.text;
        string apiKey = JsonUtility.FromJson<ApiKeyResponse>(json).apiKey;

        // LOGGING: Check what you actually received!
        Debug.Log("API Key: " + apiKey);

        AppOptions options = new AppOptions()
        {
            ApiKey = apiKey,
            AppId = "1:884450493851:android:727a4728674b1e3b268d3e",
            ProjectId = "kidstuff-68c45"
        };

        Debug.Log("App ID: " + options.AppId);
        Debug.Log("Project ID: " + options.ProjectId);

        FirebaseApp.Create(options);

        Debug.Log("Firebase initialized with runtime API key.");
    }

    [System.Serializable]
    private class ApiKeyResponse
    {
        public string apiKey;
    }
}