using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.Networking;

[System.Serializable]
public class Tool
{
    public string id;
    public string nameKey;
    public string category;
}

public class ParentHomeManager : MonoBehaviour
{
    public TMP_Dropdown parentsDropdown;
    public TMP_Dropdown childrenDropdown;
    public TMP_Dropdown toolDropdown1;
    public TMP_Dropdown toolDropdown2;
    public TMP_Dropdown toolDropdown3;
    public TMP_Dropdown toolDropdown4;

    public List<string> parents = new List<string> { "Parent 1", "Parent 2" };
    public List<string> children = new List<string> { "Child 1", "Child 2", "Child 3" };
    private List<Tool> tools = new List<Tool>();

    void Start()
    {
        StartCoroutine(LoadToolsFromJsonCoroutine());
    }

    IEnumerator LoadToolsFromJsonCoroutine()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "tools.json");
        string json = null;

#if UNITY_ANDROID && !UNITY_EDITOR
    using (UnityWebRequest www = UnityWebRequest.Get(path))
    {
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            json = www.downloadHandler.text;
        }
        else
        {
            Debug.LogError("Failed to load tools.json: " + www.error);
            yield break; // <--- This prevents the warning
        }
    }
#else
        if (File.Exists(path))
        {
            json = File.ReadAllText(path);
        }
        else
        {
            Debug.LogError("tools.json not found at path: " + path);
            yield break; // <--- This prevents the warning
        }
#endif

        if (!string.IsNullOrEmpty(json))
        {
            tools = JsonHelper.FromJson<Tool>(json);

            PopulateDropdown(parentsDropdown, parents);
            PopulateDropdown(childrenDropdown, children);

            PopulateToolDropdown(toolDropdown1);
            PopulateToolDropdown(toolDropdown2);
            PopulateToolDropdown(toolDropdown3);
            PopulateToolDropdown(toolDropdown4);
        }

        yield break; // <--- Ensures all code paths return a value
    }

    void PopulateDropdown(TMP_Dropdown dropdown, List<string> items)
    {
        dropdown.options.Clear();
        foreach (string item in items)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(item));
        }
        dropdown.RefreshShownValue();
    }

    void PopulateToolDropdown(TMP_Dropdown dropdown)
    {
        dropdown.options.Clear();
        foreach (Tool tool in tools)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(tool.id));
        }
        dropdown.RefreshShownValue();
    }
}

// Helper to parse a JSON array with Unity's JsonUtility
public static class JsonHelper
{
    public static List<T> FromJson<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return new List<T>(wrapper.array);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}