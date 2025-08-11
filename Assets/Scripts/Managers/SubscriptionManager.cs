using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SubscriptionManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text currentPlanText;
    public TMP_Text planDetailsText;
    public Button standardUpgradeButton;
    public Button premiumUpgradeButton;
    public TMP_InputField voucherCodeInput;
    public Button applyVoucherButton;
    public TMP_Text voucherFeedbackText;
    
    [Header("Plan Prices")]
    public TMP_Text standardPriceText;
    public TMP_Text premiumPriceText;
    
    private ParentChildDataManager dataManager;
    
    void Start()
    {
        dataManager = ParentChildDataManager.Instance;
        
        // Setup button listeners
        standardUpgradeButton.onClick.AddListener(() => UpgradeToPlan(SubscriptionPlan.Standard));
        premiumUpgradeButton.onClick.AddListener(() => UpgradeToPlan(SubscriptionPlan.Premium));
        applyVoucherButton.onClick.AddListener(ApplyVoucher);
        
        RefreshUI();
    }
    
    void RefreshUI()
    {
        if (dataManager == null) return;
        
        // Update current plan display
        switch (dataManager.CurrentPlan)
        {
            case SubscriptionPlan.Free:
                currentPlanText.text = "Ücretsiz Plan";
                planDetailsText.text = "• Günde 1 aktivite\n• Kamera desteði yok\n• Ýlerleme takibi yok";
                break;
            case SubscriptionPlan.Standard:
                currentPlanText.text = "Standart Plan";
                planDetailsText.text = "• Sýnýrsýz aktivite\n• Ýlerleme takibi\n• Çoklu ebeveyn";
                break;
            case SubscriptionPlan.Premium:
                currentPlanText.text = "Premium Plan";
                planDetailsText.text = "• Tüm özellikler\n• Kamera desteði\n• Video kayýt";
                break;
        }
        
        // Calculate prices (with discounts if multiple children)
        int childCount = dataManager.CurrentParent?.Children?.Count ?? 1;
        float standardPrice = CalculatePrice(7.99f, childCount);
        float premiumPrice = CalculatePrice(12.99f, childCount);
        
        standardPriceText.text = $"${standardPrice:F2}/ay";
        premiumPriceText.text = $"${premiumPrice:F2}/ay";
        
        // Hide upgrade buttons for current plan
        standardUpgradeButton.gameObject.SetActive(dataManager.CurrentPlan != SubscriptionPlan.Standard && dataManager.CurrentPlan != SubscriptionPlan.Premium);
        premiumUpgradeButton.gameObject.SetActive(dataManager.CurrentPlan != SubscriptionPlan.Premium);
    }
    
    float CalculatePrice(float basePrice, int childCount)
    {
        float totalPrice = basePrice * childCount;
        
        if (childCount == 2)
        {
            totalPrice *= 0.85f; // 15% discount
        }
        else if (childCount >= 3)
        {
            totalPrice *= 0.75f; // 25% discount
        }
        
        return totalPrice;
    }
    
    void UpgradeToPlan(SubscriptionPlan newPlan)
    {
        // In a real app, integrate with payment system here
        Debug.Log($"Upgrading to {newPlan}");
        
        // For demo purposes, directly update
        dataManager.UpdateSubscription(newPlan);
        RefreshUI();
        
        // Show success message
        voucherFeedbackText.text = $"{newPlan} planýna baþarýyla yükseltildi!";
        voucherFeedbackText.color = Color.green;
    }
    
    void ApplyVoucher()
    {
        string voucherCode = voucherCodeInput.text.Trim();
        
        if (string.IsNullOrEmpty(voucherCode))
        {
            voucherFeedbackText.text = "Lütfen voucher kodu girin";
            voucherFeedbackText.color = Color.red;
            return;
        }
        
        dataManager.ApplyVoucher(voucherCode, (success, message) =>
        {
            voucherFeedbackText.text = message;
            voucherFeedbackText.color = success ? Color.green : Color.red;
            
            if (success)
            {
                voucherCodeInput.text = "";
                RefreshUI();
            }
        });
    }
}