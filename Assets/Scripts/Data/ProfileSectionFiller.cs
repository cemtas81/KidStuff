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
        // Firestore'dan parent ve child verilerini çek
        //ParentChildDataManager.Instance.LoadParentFromFirestore(ParentChildDataManager.Instance._currentParent.Name, () =>
        //{
        //    FillParentSection();
        //    // Parent yüklendikten sonra child'ý da çekebilirsiniz
        //    if (ParentChildDataManager.Instance.CurrentParent?.Children?.Count > 0)
        //    {
        //        var firstChildName = ParentChildDataManager.Instance.CurrentParent.Children[0].Name;
        //        ParentChildDataManager.Instance.LoadChildFromFirestore(ParentChildDataManager.Instance._currentParent.Name, firstChildName, FillChildSection);
        //    }
        //});
        StartCoroutine(RefreshProfileSections());
    }

    IEnumerator RefreshProfileSections()
    {

        yield return new WaitForSeconds(.5f); // Refresh every second
        FillParentSection();
        FillChildSection();

    }
    public void FillParentSection()
    {
        var parent = ParentChildDataManager.Instance.CurrentParent;
        Debug.Log(parent != null ? $"Parent loaded: {parent.Name}" : "Parent is null!");
        if (parent != null)
        {
            parentNameInput.text = parent.Name;
            parentAgeInput.text = parent.Age.ToString();

            // Dropdown deðerini ayarla ve görsel olarak güncelle
            parentGenderDropdown.value = parent.Gender;
            parentGenderDropdown.RefreshShownValue();

            Debug.Log($"Parent gender:{parent.Gender}");
        }
        else
        {
            parentNameInput.text = "";
            parentAgeInput.text = "";

            parentGenderDropdown.value = 0;
            parentGenderDropdown.RefreshShownValue();
        }
    }

    public void FillChildSection()
    {
        var child = ParentChildDataManager.Instance.CurrentChild;
        Debug.Log(child != null ? $"Child loaded: {child.Name}" : "Child is null!");
        if (child != null)
        {
            childNameInput.text = child.Name;
            childAgeInput.text = child.Age.ToString();

            // Dropdown deðerini ayarla ve görsel olarak güncelle
            childGenderDropdown.value = child.Gender;
            childGenderDropdown.RefreshShownValue();

            childHobbiesInput.text = child.Hobbies != null ? string.Join(", ", child.Hobbies) : "";
        }
        else
        {
            childNameInput.text = "";
            childAgeInput.text = "";
            childGenderDropdown.value = 0;
            childGenderDropdown.RefreshShownValue();
            childHobbiesInput.text = "";
        }
    }
}