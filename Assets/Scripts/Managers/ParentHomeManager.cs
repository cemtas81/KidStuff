using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.Networking;

public class ParentHomeManager : MonoBehaviour
{
    public TMP_Dropdown parentsDropdown;
    public TMP_Dropdown childrenDropdown;
    public TMP_Dropdown toolDropdown1;
    public TMP_Dropdown toolDropdown2;
    public TMP_Dropdown toolDropdown3;
    public TMP_Dropdown toolDropdown4;

    private List<string> parentNames = new();
    private List<string> childNames = new();
    private List<Tool> tools = new();
    private ParentChildDataManager dataManager;

    void Start()
    {
        dataManager = ParentChildDataManager.Instance;
        StartCoroutine(LoadDataAndPopulateDropdowns());
    }

    IEnumerator LoadDataAndPopulateDropdowns()
    {
        // FirestoreManager'ýn hazýr olmasýný bekle
        while (FirestoreManager.Instance == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        // Paralel olarak tools ve parent verilerini yükle
        yield return StartCoroutine(LoadToolsFromJsonCoroutine());
        yield return StartCoroutine(LoadParentsFromFirestore());
        
        // Ýlk parent seçiliyse onun children'larýný yükle
        if (parentNames.Count > 0)
        {
            yield return StartCoroutine(LoadChildrenForParent(parentNames[0]));
        }
    }

    IEnumerator LoadParentsFromFirestore()
    {
        bool isLoaded = false;
        
        // FirestoreManager'dan tüm parent'larý çekmek için GetAllParents metodunu kullan
        FirestoreManager.Instance.GetAllParents(parents =>
        {
            parentNames.Clear();
            foreach (var parent in parents)
            {
                if (!string.IsNullOrEmpty(parent.Name))
                {
                    parentNames.Add(parent.Name);
                }
            }
            PopulateDropdown(parentsDropdown, parentNames);
            isLoaded = true;
            Debug.Log($"Loaded {parentNames.Count} parents from Firestore");
        });

        yield return new WaitUntil(() => isLoaded);
    }

    IEnumerator LoadChildrenForParent(string parentName)
    {
        bool isLoaded = false;
        
        //dataManager.GetChildrenOfParent(parentName, children =>
        //{
        //    childNames.Clear();
        //    foreach (var child in children)
        //    {
        //        if (!string.IsNullOrEmpty(child.Name))
        //        {
        //            childNames.Add(child.Name);
        //        }
        //    }
        //    PopulateDropdown(childrenDropdown, childNames);
        //    isLoaded = true;
        //    Debug.Log($"Loaded {childNames.Count} children for parent: {parentName}");
        //});

        yield return new WaitUntil(() => isLoaded);
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
                yield break;
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
            yield break;
        }
#endif

        if (!string.IsNullOrEmpty(json))
        {
            // DropdownPopulator'daki JsonHelper'ý kullan
            tools = DropdownJsonHelper.FromJson<Tool>(json);

            PopulateToolDropdown(toolDropdown1);
            PopulateToolDropdown(toolDropdown2);
            PopulateToolDropdown(toolDropdown3);
            PopulateToolDropdown(toolDropdown4);
            
            Debug.Log($"Loaded {tools.Count} tools from JSON");
        }
    }

    // Parent seçimi deðiþtiðinde children'larý güncelle
    public void OnParentSelectionChanged()
    {
        int selectedIndex = parentsDropdown.value;
        if (selectedIndex >= 0 && selectedIndex < parentNames.Count)
        {
            string selectedParent = parentNames[selectedIndex];
            StartCoroutine(LoadChildrenForParent(selectedParent));
            
            // Seçilen parent'ý current parent olarak ayarla
            //dataManager.LoadParentFromFirestore(selectedParent, () =>
            //{
            //    Debug.Log($"Current parent set to: {selectedParent}");
            //});
        }
    }

    // Child seçimi deðiþtiðinde current child'ý güncelle
    public void OnChildSelectionChanged()
    {
        int selectedParentIndex = parentsDropdown.value;
        int selectedChildIndex = childrenDropdown.value;
        
        if (selectedParentIndex >= 0 && selectedParentIndex < parentNames.Count &&
            selectedChildIndex >= 0 && selectedChildIndex < childNames.Count)
        {
            string selectedParent = parentNames[selectedParentIndex];
            string selectedChild = childNames[selectedChildIndex];
            
            //dataManager.LoadChildFromFirestore(selectedParent, selectedChild, () =>
            //{
            //    Debug.Log($"Current child set to: {selectedChild}");
            //});
        }
    }

    // Refresh butonu için
    public void RefreshDropdowns()
    {
        StartCoroutine(LoadDataAndPopulateDropdowns());
    }

    void PopulateDropdown(TMP_Dropdown dropdown, List<string> items)
    {
        if (dropdown == null) return;
        
        dropdown.options.Clear();
        foreach (string item in items)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(item));
        }
        dropdown.RefreshShownValue();
    }

    void PopulateToolDropdown(TMP_Dropdown dropdown)
    {
        if (dropdown == null) return;
        
        dropdown.options.Clear();
        foreach (Tool tool in tools)
        {
            // tool.id yerine direkt string kullan
            string toolId = tool.id ?? "";
            dropdown.options.Add(new TMP_Dropdown.OptionData(toolId));
        }
        dropdown.RefreshShownValue();
    }
}

// Çakýþmayý önlemek için farklý bir isim kullan
public static class DropdownJsonHelper
{
    public static List<T> FromJson<T>(string json)
    {
        string newJson = "{ \"items\": " + json + "}";
        JsonWrapper<T> wrapper = JsonUtility.FromJson<JsonWrapper<T>>(newJson);
        return new List<T>(wrapper.items);
    }

    [System.Serializable]
    private class JsonWrapper<T>
    {
        public T[] items;
    }

}