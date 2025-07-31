using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ButtonClickAndHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private bool isPointerDown = false;
    private float pointerDownTimer = 0f;
    private bool longPressTriggered = false;

    [SerializeField] private int sceneIndex;
    [SerializeField] private GameObject deleteScreen;
    [SerializeField] private float holdTime = 1.5f;

    void Update()
    {
        if (isPointerDown && !longPressTriggered)
        {
            pointerDownTimer += Time.deltaTime;
            if (pointerDownTimer >= holdTime)
            {
                longPressTriggered = true;
                isPointerDown = false; // Stop the timer to avoid multiple triggers
                ShowDeleteScreen();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        pointerDownTimer = 0f;
        longPressTriggered = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Only trigger short click if long press was NOT triggered
        if (!longPressTriggered)
        {
            OnShortClick();
        }
        Reset();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Reset();
    }

    private void Reset()
    {
        isPointerDown = false;
        pointerDownTimer = 0f;
        // Do NOT reset longPressTriggered here; it should only reset on next pointer down
    }

    private void OnShortClick()
    {
        SceneManager.LoadScene(sceneIndex);
    }

    private void ShowDeleteScreen()
    {
        if (deleteScreen != null)
            deleteScreen.SetActive(true);
        Debug.Log("Long press detected: Show Delete Screen");
    }
}