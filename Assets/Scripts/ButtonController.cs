using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public enum ButtonTypes
{
    Back,
    Home,
    Play,
    Submit,
    ErrorOK,
    Answer,
    Next
};

public class ButtonController : MonoBehaviour
{
    [SerializeField] public ButtonTypes ButtonType;
    [SerializeField] public AnimationCurve ReactiveAnim;

    protected Coroutine _handleButtonAnimation = null;
    protected Vector3 _startScale;

    // public static event System.EventHandler<ButtonPressedEventArgs> OnButtonPressed;
    // public static event System.EventHandler<ButtonPressedEventArgs> OnButtonDown;
    // public static event System.EventHandler<ButtonPressedEventArgs> OnButtonUp;

    void Start()
    {
        InitializeEventTriggers();
    }

    private void InitializeEventTriggers()
    {
        EventTrigger _eventTrigger = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { OnPointerClick((PointerEventData)data); });
        _eventTrigger.triggers.Add(entry);

        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { OnPointerEnter((PointerEventData)data); });
        _eventTrigger.triggers.Add(entry);

        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerExit;
        entry.callback.AddListener((data) => { OnPointerExit((PointerEventData)data); });
        _eventTrigger.triggers.Add(entry);

        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        entry.callback.AddListener((data) => { OnPointerUp((PointerEventData)data); });
        _eventTrigger.triggers.Add(entry);

        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((data) => { OnPointerDown((PointerEventData)data); });
        _eventTrigger.triggers.Add(entry);
    }

    private void OnPointerClick(PointerEventData data)
    {
        AnimateButton();
    }

    private void ContinueOnPointerClick()
    {
        switch (ButtonType)
        {
            case ButtonTypes.Play:
                StateMachine.Instance.ChangeToState(State.TopicSelect);
                break;
            case ButtonTypes.Submit:
                StateMachine.Instance.ChangeToState(State.TopicSubmit);
                break;
            case ButtonTypes.Back:
                StateMachine.Instance.ChangeToState(State.MainMenu);
                break;
            case ButtonTypes.ErrorOK:
                StateMachine.Instance.Canvas_ErrorMessage.SetActive(false);
                break;
            case ButtonTypes.Answer:
                var answerClicked = GetComponentInParent<AnswerButtonController>();
                GameplayManager.Instance.OnAnswerClicked(answerClicked);
                break;
        }
    }

    private void OnPointerDown(PointerEventData data)
    {
    }

    private void OnPointerUp(PointerEventData data)
    {
    }

    private void OnPointerExit(PointerEventData data)
    {
    }

    private void OnPointerEnter(PointerEventData data)
    {
    }

    private void AnimateButton()
    {
        if (gameObject.GetComponent<Button>().interactable)
        {
            const float DURATION = 0.15f;
            _startScale = transform.localScale;
            _handleButtonAnimation ??= StartCoroutine(ContinueButtonAnimateScale(DURATION));
        }
    }

    private IEnumerator ContinueButtonAnimateScale(float duration)
    {
        if(duration <= 0)
        {
            yield return null;
        }

        float elapsedTime = 0;
        do
        {
            gameObject.transform.localScale = _startScale * ReactiveAnim.Evaluate(elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        } 
        while (elapsedTime < duration);

        transform.localScale = _startScale;
        _handleButtonAnimation = null;
        ContinueOnPointerClick();
    }
}
