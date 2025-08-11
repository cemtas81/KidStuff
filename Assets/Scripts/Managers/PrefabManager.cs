using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform anchor;
    public ProfileSectionFiller filler;

    public void LoadPrefab()
    {
        if (prefab != null && anchor != null)
        {
    
            // Alternatif olarak direkt olarak parent belirterek instantiate edebilirsiniz:
            GameObject instantiatedObject = Instantiate(prefab, anchor);
            filler = instantiatedObject.GetComponent<ProfileSectionFiller>();
        }
        else
        {
            if (prefab == null)
                Debug.LogError("Prefab not assigned!");
            if (anchor == null)
                Debug.LogError("Anchor not assigned!");
        }
    }
    public void FillProfile()
    {
       filler.FillChildSection();
    }
}
