using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TMP_InputField parentNameInput;
    public TMP_InputField parentAgeInput;
    public TMP_Dropdown parentGenderDropdown;
    public TMP_Dropdown childGenderDropdown;
    [SerializeField] Canvas Login;
    [SerializeField] Canvas Home;
    public TMP_InputField childNameInput;
    public TMP_InputField childAgeInput;
    public TMP_InputField childHobbiesInput;

    public TMP_Dropdown activityDropdown;

    private ParentChildDataManager dataManager;

    private IEnumerator Start()
    {
        dataManager = ParentChildDataManager.Instance;

        // ParentChildDataManager'ýn hazýr olmasýný bekle
        while (dataManager == null)
        {
            yield return null;
            dataManager = ParentChildDataManager.Instance;
        }

        // FirestoreManager'ýn hazýr olmasýný bekle
        while (FirestoreManager.Instance == null)
            yield return null;

        while (!FirestoreManager.Instance.firebaseReady)
            yield return null;

        // AuthManager'ýn hazýr olmasýný bekle
        while (AuthManager.Instance == null)
            yield return null;

        // Baþlangýçta UI durumunu kontrol et
        UpdateUIBasedOnAuthState();

        // Veri yüklenme durumunu izle
        StartCoroutine(MonitorDataChanges());
    }

    private void UpdateUIBasedOnAuthState()
    {
        // Eðer kullanýcý zaten giriþ yapmýþsa ve veri varsa
        if (AuthManager.Instance.CurrentUser != null && dataManager.CurrentParent != null)
        {
            ShowHomeScreen();
            FillUIWithParentAndChildData();
        }
        else
        {
            ShowLoginScreen();
        }
    }

    private IEnumerator MonitorDataChanges()
    {
        ParentData lastParent = null;
        ChildData lastChild = null;

        while (true)
        {
            // Parent verisi deðiþti mi kontrol et
            if (dataManager.CurrentParent != lastParent)
            {
                lastParent = dataManager.CurrentParent;
                UpdateUIBasedOnAuthState();
            }

            // Child verisi deðiþti mi kontrol et
            if (dataManager.CurrentChild != lastChild)
            {
                lastChild = dataManager.CurrentChild;
                if (dataManager.CurrentParent != null) // Sadece parent varsa child UI'ý güncelle
                {
                    FillChildData();
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void ShowHomeScreen()
    {
        Home.GetComponent<CanvasGroup>().alpha = 1;
        Home.GetComponent<CanvasGroup>().interactable = true;
        Home.GetComponent<CanvasGroup>().blocksRaycasts = true;
        Login.GetComponent<CanvasGroup>().alpha = 0;
        Login.GetComponent<CanvasGroup>().interactable = false;
        Login.GetComponent<CanvasGroup>().blocksRaycasts = false;

        Debug.Log("Home screen activated");
    }

    private void ShowLoginScreen()
    {
        Home.GetComponent<CanvasGroup>().alpha = 0;
        Home.GetComponent<CanvasGroup>().interactable = false;
        Home.GetComponent<CanvasGroup>().blocksRaycasts = false;
        Login.GetComponent<CanvasGroup>().alpha = 1;
        Login.GetComponent<CanvasGroup>().interactable = true;
        Login.GetComponent<CanvasGroup>().blocksRaycasts = true;

        Debug.Log("Login screen activated");
    }

    public void OnParentNameChanged() => dataManager.SetParentName(parentNameInput.text);

    public void OnParentAgeChanged()
    {
        if (int.TryParse(parentAgeInput.text, out int age))
            dataManager.SetParentAge(age);
    }

    public void OnParentGenderChanged()
    {
        dataManager.SetParentGender(parentGenderDropdown.value);
        parentGenderDropdown.RefreshShownValue();
        
        Debug.Log($"Parent gender changed to: {parentGenderDropdown.value}");
    }

    public void OnChildNameChanged() => dataManager.SetChildName(childNameInput.text);

    public void OnChildAgeChanged()
    {
        if (int.TryParse(childAgeInput.text, out int age))
            dataManager.SetChildAge(age);
    }

    public void OnChildGenderChanged()
    {
        dataManager.SetChildGender(childGenderDropdown.value);
        childGenderDropdown.RefreshShownValue();
        
        Debug.Log($"Child gender changed to: {childGenderDropdown.value}");
    }

    public void OnChildHobbiesChanged()
    {
        var hobbies = childHobbiesInput.text
            .Split(',')
            .Select(h => h.Trim())
            .Where(h => !string.IsNullOrEmpty(h))
            .Distinct()
            .ToList();
        dataManager.SetChildHobbies(hobbies);
    }

    public void PopulateMatchingActivitiesDropdown()
    {
        PopulateMatchingActivities(matchingActivities =>
        {
            activityDropdown.ClearOptions();
            var options = matchingActivities.Select(a => a.Title).ToList();
            if (options.Count == 0)
                options.Add("Uygun aktivite bulunamadý");
            activityDropdown.AddOptions(options);
        });
    }

    public void PopulateMatchingActivities(System.Action<List<ActivityData>> onActivitiesPopulated)
    {
        var child = dataManager.CurrentChild;
        if (child == null)
        {
            Debug.LogWarning("Çocuk verisi bulunamadý.");
            onActivitiesPopulated?.Invoke(new List<ActivityData>());
            return;
        }

        FirestoreManager.Instance.GetActivities(allActivities =>
        {
            var matchingActivities = allActivities.Where(activity =>
                child.Age >= activity.MinAge &&
                child.Age <= activity.MaxAge &&
                (activity.RelatedHobbies == null || child.Hobbies.Any(h => activity.RelatedHobbies.Contains(h)))
            ).ToList();

            onActivitiesPopulated?.Invoke(matchingActivities);
        });
    }

    private void FillUIWithParentAndChildData()
    {
        var parent = dataManager.CurrentParent;
        
        if (parent != null)
        {
            parentNameInput.text = parent.Name ?? "";
            parentAgeInput.text = parent.Age.ToString();
            parentGenderDropdown.value = parent.Gender;
            
            Debug.Log($"Parent data filled: {parent.Name}");
        }

        FillChildData();
    }

    private void FillChildData()
    {
        var child = dataManager.CurrentChild;
        
        if (child != null)
        {
            childNameInput.text = child.Name ?? "";
            childAgeInput.text = child.Age.ToString();
            childGenderDropdown.value = child.Gender;
            childHobbiesInput.text = string.Join(", ", child.Hobbies ?? new List<string>());
            
            Debug.Log($"Child data filled: {child.Name}");
        }
        else
        {
            // Child verisi yoksa alanlarý temizle
            childNameInput.text = "";
            childAgeInput.text = "";
            childGenderDropdown.value = 0;
            childHobbiesInput.text = "";
        }
    }

    public void SaveCurrentChild()
    {
        var child = dataManager.CurrentChild;
        if (child == null)
        {
            Debug.LogWarning("Kaydedilecek çocuk verisi yok.");
            return;
        }
        dataManager.SaveOrUpdateChild(child);
        Debug.Log("Çocuk verisi kaydedildi/güncellendi.");
    }

    // Manuel logout için kullanýlabilir
    public void OnLogoutClicked()
    {
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.Logout();
            ShowLoginScreen();
        }
    }

    // Parent verilerini sýfýrlama fonksiyonu
    public void ResetParentData()
    {
        if (dataManager != null)
        {
            dataManager.ClearCurrentData();
            ClearUIFields();
            ShowLoginScreen();
            Debug.Log("Parent data reset completed");
        }
        else
        {
            Debug.LogWarning("DataManager bulunamadý, reset iþlemi yapýlamadý.");
        }
    }

    // UI alanlarýný temizleme fonksiyonu
    private void ClearUIFields()
    {
        // Parent alanlarýný temizle
        parentNameInput.text = "";
        parentAgeInput.text = "";
        parentGenderDropdown.value = 0;

        // Child alanlarýný temizle
        childNameInput.text = "";
        childAgeInput.text = "";
        childGenderDropdown.value = 0;
        childHobbiesInput.text = "";

        // Activity dropdown'ý temizle
        activityDropdown.ClearOptions();
        activityDropdown.AddOptions(new List<string> { "Aktivite seçiniz" });

        Debug.Log("UI fields cleared");
    }
}