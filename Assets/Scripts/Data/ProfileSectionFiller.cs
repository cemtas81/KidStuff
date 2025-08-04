using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class ProfileSectionFiller : MonoBehaviour
{
    [Header("Parent Profile")]
    public TMP_Text parentNameInput;
    public TMP_Text parentAgeInput;
    public TMP_Dropdown parentGenderDropdown;

    [Header("Child Profile")]
    public TMP_Text childNameInput;
    public TMP_Text childAgeInput;
    public TMP_Dropdown childGenderDropdown;
    public TMP_Text childHobbiesInput;


    void Start()
    {
        //FillParentSection();
        //FillChildSection();
        StartCoroutine(RefreshProfileSections());
    }

    IEnumerator RefreshProfileSections()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // Refresh every second
            FillParentSection();
            FillChildSection();
        }
    }
    void FillParentSection()
    {
        var parent = ParentChildDataManager.Instance.CurrentParent;
        if (parent != null)
        {
            parentNameInput.text = parent.Name;
            parentAgeInput.text = parent.Age.ToString();
            int genderIndex = parentGenderDropdown.options.FindIndex(o => o.text == parent.Gender);
            parentGenderDropdown.value = genderIndex >= 0 ? genderIndex : 0;
        }
        else
        {
            parentNameInput.text = "";
            parentAgeInput.text = "";
            parentGenderDropdown.value = 0;
        }
    }

    void FillChildSection()
    {
        var child = ParentChildDataManager.Instance.CurrentChild;
        if (child != null)
        {
            childNameInput.text = child.Name;
            childAgeInput.text = child.Age.ToString();
            int genderIndex = childGenderDropdown.options.FindIndex(o => o.text == child.Gender);
            childGenderDropdown.value = genderIndex >= 0 ? genderIndex : 0;
            childHobbiesInput.text = child.Hobbies != null ? string.Join(", ", child.Hobbies) : "";
        }
        else
        {
            childNameInput.text = "";
            childAgeInput.text = "";
            childGenderDropdown.value = 0;
            childHobbiesInput.text = "";
        }
    }
}