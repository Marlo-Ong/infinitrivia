using System.Collections;
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
    Next,
    HomeConfirmation,
    HomeConfirmationOK,
    HomeConfirmationCancel,
    Mute,
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
        _startScale = transform.localScale;
    }

    private void InitializeEventTriggers()
    {
        EventTrigger _eventTrigger = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { OnPointerClick((PointerEventData)data); });
        _eventTrigger.triggers.Add(entry);
    }

    private void OnPointerClick(PointerEventData data)
    {
        SoundManager.Instance.PlaySFX(Random.Range(4,6));
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
            case ButtonTypes.Home:
                StateMachine.Instance.ChangeToState(State.MainMenu);
                break;
            case ButtonTypes.HomeConfirmation:
                StateMachine.Instance.Canvas_HomeConfirmation.SetActive(true);
                break;
            case ButtonTypes.HomeConfirmationOK:
                StateMachine.Instance.Canvas_HomeConfirmation.SetActive(false);
                GameplayManager.Instance.Interrupt_QuitToHome();
                break;
            case ButtonTypes.HomeConfirmationCancel:
                StateMachine.Instance.Canvas_HomeConfirmation.SetActive(false);
                break;
            case ButtonTypes.Mute:
                SoundManager.Instance.ToggleAudio();
                break;
        }
    }

    private void AnimateButton()
    {
        if (gameObject.GetComponent<Button>().interactable)
        {
            const float DURATION = 0.15f;
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
