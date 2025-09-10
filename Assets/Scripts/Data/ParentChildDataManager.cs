using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Firebase.Auth;

public class ParentChildDataManager : MonoBehaviour
{
    public static ParentChildDataManager Instance { get; private set; }

    private FirestoreManager firestoreManager;
    private string _parentDocId; // UID
    public ParentData CurrentParent { get; private set; }
    public ChildData CurrentChild { get; private set; }

    private SubscriptionPlan _currentPlan = SubscriptionPlan.Free;
    private bool _hasValidSubscription = false;
    public SubscriptionPlan CurrentPlan => _currentPlan;
    public bool HasValidSubscription => _hasValidSubscription;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (transform.parent == null) DontDestroyOnLoad(gameObject);

        StartCoroutine(InitializeFirestoreManager());
        StartCoroutine(SubscribeAndAutoLoad());
    }

    IEnumerator InitializeFirestoreManager()
    {
        while (FirestoreManager.Instance == null) yield return new WaitForSeconds(0.1f);
        firestoreManager = FirestoreManager.Instance;
        LoadSubscriptionData();
    }

    private IEnumerator SubscribeAndAutoLoad()
    {
        while (AuthManager.Instance == null) yield return null;
        AuthManager.Instance.OnUserLoggedIn += OnUserLoggedIn;

        while (!IsFirestoreReady()) yield return null;

        if (AuthManager.Instance.CurrentUser != null)
            yield return TryLoadForCurrentUser();
    }

    void OnDestroy()
    {
        if (AuthManager.Instance != null)
            AuthManager.Instance.OnUserLoggedIn -= OnUserLoggedIn;
    }

    private void OnUserLoggedIn(FirebaseUser user)
    {
        StartCoroutine(TryLoadForCurrentUser());
        // Kullanýcý geldiðinde bekleyen parent/child yazýmlarýný da tetikleyelim
        if (CurrentParent != null)
            StartCoroutine(SaveParentWhenReadyIncludingAuth());
        if (CurrentChild != null)
            StartCoroutine(SaveChildWhenReadyIncludingAuth(CurrentChild));
    }

    private IEnumerator TryLoadForCurrentUser()
    {
        while (!IsFirestoreReady()) yield return null;

        var user = AuthManager.Instance?.CurrentUser;
        if (user == null) yield break;

        _parentDocId = user.UserId; // DOC ID = UID

        bool done = false;
        LoadParentAndChildrenByUserId(_parentDocId, _ => done = true);
        while (!done) yield return null;

        // Otomatik varsayýlan oluþturma YOK.
    }

    private bool IsFirestoreReady() => firestoreManager != null && firestoreManager.firebaseReady;

    public void ClearCurrentData()
    {
        CurrentParent = null;
        CurrentChild = null;
        _currentPlan = SubscriptionPlan.Free;
        _hasValidSubscription = false;
        _parentDocId = null;
    }

    // === UID tabanlý yükleme ===
    private void LoadParentAndChildrenByUserId(string userId, System.Action<bool> onLoaded)
    {
        if (!IsFirestoreReady()) { onLoaded?.Invoke(false); return; }

        firestoreManager.GetParentByUserId(userId, parent =>
        {
            CurrentParent = parent;
            if (parent == null) { onLoaded?.Invoke(false); return; }

            firestoreManager.GetChildrenOfParentByUserId(userId, children =>
            {
                CurrentParent.Children = children;
                CurrentChild = (children != null && children.Count > 0) ? children[0] : null;
                onLoaded?.Invoke(true);
            });
        });
    }

    // Geriye dönük API: varsa UID ile çaðýr
    public void LoadParentAndChildren(string parentName, System.Action<bool> onLoaded)
    {
        if (!string.IsNullOrEmpty(_parentDocId))
            LoadParentAndChildrenByUserId(_parentDocId, onLoaded);
        else
            onLoaded?.Invoke(false);
    }

    // KAYDETME: Auth ve Firestore hazýr olana kadar bekleyip sonra yaz
    public void SaveParent()
    {
        if (CurrentParent == null) return;
        StartCoroutine(SaveParentWhenReadyIncludingAuth());
    }

    private IEnumerator SaveParentWhenReadyIncludingAuth()
    {
        // AuthManager ve kullanýcýyý bekle
        while (AuthManager.Instance == null || AuthManager.Instance.CurrentUser == null)
            yield return null;

        // Firestore'u bekle
        while (!IsFirestoreReady())
            yield return null;

        var user = AuthManager.Instance.CurrentUser;
        _parentDocId = user.UserId;
        CurrentParent.Email = user.Email;

        firestoreManager.AddOrUpdateParent(_parentDocId, CurrentParent);
    }

    public void SaveOrUpdateChild(ChildData child)
    {
        if (child == null || string.IsNullOrEmpty(child.Name)) return;

        // Yerel modele yaz
        if (CurrentParent != null)
        {
            if (CurrentParent.Children == null) CurrentParent.Children = new List<ChildData>();
            var idx = CurrentParent.Children.FindIndex(c => c.Name == child.Name);
            if (idx >= 0) CurrentParent.Children[idx] = child;
            else CurrentParent.Children.Add(child);
        }
        CurrentChild = child;

        // Kalýcý yazýmý kuyrukla
        StartCoroutine(SaveChildWhenReadyIncludingAuth(child));
    }

    private IEnumerator SaveChildWhenReadyIncludingAuth(ChildData child)
    {
        if (child == null || string.IsNullOrEmpty(child.Name)) yield break;

        // Auth ve Firestore hazýr olana kadar bekle
        while (AuthManager.Instance == null || AuthManager.Instance.CurrentUser == null)
            yield return null;
        while (!IsFirestoreReady())
            yield return null;

        var user = AuthManager.Instance.CurrentUser;
        firestoreManager.AddChildToParentByUserId(user.UserId, child);
    }

    // UI set’leri – sadece alanlarý günceller; kalýcý yazým yukarýda kuyrukla
    public void SetParentName(string name)
    {
        if (CurrentParent == null) CurrentParent = new ParentData { Children = new List<ChildData>() };
        CurrentParent.Name = name;
        SaveParent();
    }

    public void SetParentAge(int age)
    {
        if (CurrentParent == null) CurrentParent = new ParentData { Children = new List<ChildData>() };
        CurrentParent.Age = age;
        SaveParent();
    }

    public void SetParentGender(int gender)
    {
        if (CurrentParent == null) CurrentParent = new ParentData { Children = new List<ChildData>() };
        CurrentParent.Gender = gender;
        SaveParent();
    }

    public void SetChildName(string name)
    {
        if (CurrentChild == null) CurrentChild = new ChildData { Hobbies = new List<string>() };
        CurrentChild.Name = name;
        SaveOrUpdateChild(CurrentChild);
    }

    public void SetChildAge(int age)
    {
        if (CurrentChild == null) CurrentChild = new ChildData { Hobbies = new List<string>() };
        CurrentChild.Age = age;
        SaveOrUpdateChild(CurrentChild);
    }

    public void SetChildGender(int gender)
    {
        if (CurrentChild == null) CurrentChild = new ChildData { Hobbies = new List<string>() };
        CurrentChild.Gender = gender;
        SaveOrUpdateChild(CurrentChild);
    }

    public void SetChildHobbies(List<string> hobbies)
    {
        if (CurrentChild == null) CurrentChild = new ChildData { Hobbies = new List<string>() };
        CurrentChild.Hobbies = hobbies;
        SaveOrUpdateChild(CurrentChild);
    }

    public void LoadSubscriptionData()
    {
        if (!IsFirestoreReady()) return;
        var userId = AuthManager.Instance?.CurrentUser?.UserId;
        if (!string.IsNullOrEmpty(userId))
        {
            firestoreManager.GetUserSubscription(userId, subscriptionData =>
            {
                if (subscriptionData != null)
                {
                    _currentPlan = subscriptionData.Plan;
                    _hasValidSubscription = subscriptionData.IsActive && subscriptionData.ExpiryDate > System.DateTime.Now;
                }
            });
        }
    }

    public void UpdateSubscription(SubscriptionPlan newPlan)
    {
        _currentPlan = newPlan;
        _hasValidSubscription = true;
        var userId = AuthManager.Instance?.CurrentUser?.UserId;
        if (!string.IsNullOrEmpty(userId) && IsFirestoreReady())
        {
            var subscriptionData = new SubscriptionData
            {
                Plan = newPlan,
                IsActive = true,
                StartDate = System.DateTime.Now,
                ExpiryDate = System.DateTime.Now.AddMonths(1)
            };
            firestoreManager.UpdateUserSubscription(userId, subscriptionData);
        }
    }

    public bool CanAccessActivity(ActivityData activity) =>
        _currentPlan == SubscriptionPlan.Free ? activity.IsFreeActivity : true;

    public bool CanUseCamera() => _currentPlan == SubscriptionPlan.Premium;
    public bool CanAccessProgressTracking() => _currentPlan != SubscriptionPlan.Free;

    public void ApplyVoucher(string voucherCode, System.Action<bool, string> callback)
    {
        if (!IsFirestoreReady()) { callback?.Invoke(false, "Firestore not ready"); return; }
        firestoreManager.ValidateVoucher(voucherCode, voucherData =>
        {
            if (voucherData != null && voucherData.IsValid && !voucherData.IsUsed)
            {
                switch (voucherData.Type)
                {
                    case VoucherType.FreePremium: UpdateSubscription(SubscriptionPlan.Premium); break;
                    case VoucherType.FreeStandard: UpdateSubscription(SubscriptionPlan.Standard); break;
                }
                firestoreManager.MarkVoucherAsUsed(voucherCode);
                callback?.Invoke(true, "Voucher applied successfully");
            }
            else
            {
                callback?.Invoke(false, "Invalid or expired voucher");
            }
        });
    }
}

public enum SubscriptionPlan
{
    Free = 0,
    Standard = 1,
    Premium = 2
}

[System.Serializable]
public class SubscriptionData
{
    public SubscriptionPlan Plan;
    public bool IsActive;
    public System.DateTime StartDate;
    public System.DateTime ExpiryDate;
}

public enum VoucherType
{
    FreePremium,
    FreeStandard,
    LifetimeFree
}

[System.Serializable]
public class VoucherData
{
    public string Code;
    public VoucherType Type;
    public bool IsValid;
    public bool IsUsed;
    public System.DateTime ExpiryDate;
}