using UnityEngine;
using TMPro; // Import TextMeshPro namespace
using System.Collections.Generic;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public TMP_InputField parentNameInput;
    public TMP_InputField parentAgeInput;
    public TMP_Dropdown parentGenderDropdown;
    public TMP_Dropdown childGenderDropdown;

    public TMP_InputField childNameInput;
    public TMP_InputField childAgeInput;
    public TMP_InputField childHobbiesInput;

    private ParentChildDataManager dataManager;
    
    private void Start()
    {
        dataManager = ParentChildDataManager.Instance;
    }

    public void OnParentNameChanged() => dataManager.SetParentName(parentNameInput.text);

    public void OnParentAgeChanged()
    {
        if (int.TryParse(parentAgeInput.text, out int age))
            dataManager.SetParentAge(age);
    }

    public void OnParentGenderChanged()
    {
        // Önce veriyi kaydet, sonra görsel güncellemeyi yap
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
        // Önce veriyi kaydet, sonra görsel güncellemeyi yap
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
            .ToList();
        dataManager.SetChildHobbies(hobbies);
    }
}