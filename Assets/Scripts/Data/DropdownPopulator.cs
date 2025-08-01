using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.Networking;

[System.Serializable] public class Tool { public string id; public string nameKey; public string category; }
[System.Serializable] public class Hobby { public string id; public string nameKey; public string category; }
[System.Serializable] public class Skill { public string id; public string nameKey; public string category; }
[System.Serializable] public class Activity { public string id; public string nameKey; public string category; }

public class DropdownPopulator : MonoBehaviour
{
    public TMP_Dropdown toolsDropdown;
    public TMP_Dropdown hobbiesDropdown;
    public TMP_Dropdown skillsDropdown;
    public TMP_Dropdown activitiesDropdown;

    void Start()
    {
        StartCoroutine(LoadFromJsonCoroutine<Tool>("tools.json", toolsDropdown, t => t.id));
        StartCoroutine(LoadFromJsonCoroutine<Hobby>("hobbies.json", hobbiesDropdown, h => h.id));
        StartCoroutine(LoadFromJsonCoroutine<Skill>("skills.json", skillsDropdown, s => s.id));
        StartCoroutine(LoadFromJsonCoroutine<Activity>("activities.json", activitiesDropdown, a => a.id));
    }

    IEnumerator LoadFromJsonCoroutine<T>(string fileName, TMP_Dropdown dropdown, System.Func<T, string> getId)
    {
        if (dropdown == null) yield break;

        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        string json = null;

#if UNITY_ANDROID && !UNITY_EDITOR
        using (UnityWebRequest www = UnityWebRequest.Get(path))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
                json = www.downloadHandler.text;
            else
            {
                Debug.LogError("Failed to load " + fileName + ": " + www.error);
                yield break;
            }
        }
#else
        if (File.Exists(path))
            json = File.ReadAllText(path);
        else
        {
            Debug.LogError(fileName + " not found at path: " + path);
            yield break;
        }
#endif

        if (!string.IsNullOrEmpty(json))
        {
            List<T> items = JsonHelper.FromJson<T>(json);
            PopulateDropdown(dropdown, items, getId);
        }
    }

    void PopulateDropdown<T>(TMP_Dropdown dropdown, List<T> items, System.Func<T, string> getId)
    {
        if (dropdown == null) return;

        dropdown.options.Clear();
        foreach (var item in items)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(getId(item)));
        }
        dropdown.RefreshShownValue();
    }
}

public static class JsonHelper
{
    public static List<T> FromJson<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return new List<T>(wrapper.array);
    }

    [System.Serializable]
    private class Wrapper<T> { public T[] array; }
}