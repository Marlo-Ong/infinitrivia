using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum State
{
    MainMenu,
    TopicSelect,
    TopicSubmit,
    RoundStart,
    OverallResults,
    None
}

public class StateMachine : Singleton<StateMachine>
{
    public State CurrentState;
    public State PreviousState;
    [SerializeField] private GameObject Canvas_MainMenu;
    [SerializeField] private GameObject Canvas_Topics;
    [SerializeField] private GameObject Canvas_LoadingScreen;
    [SerializeField] public GameObject Canvas_ErrorMessage;
    [SerializeField] private GameObject Canvas_OverallResults;
    [SerializeField] private GameObject Canvas_GameScreen;

    # region Initializers

    void Start()
    {
        StateEnter(State.MainMenu);
    }

    # endregion
    # region State Operations

    public void ChangeToState(State state)
    {
        StateExit(CurrentState);
        Debug.Log($"{this}: State change to: {state}");
        PreviousState = CurrentState;
        CurrentState = state;
        StateEnter(CurrentState);
    }

    public void RevertState()
    {
        if (CurrentState != State.MainMenu)
        {
            ChangeToState(PreviousState);
        }
    }

    private void StateEnter(State state)
    {
        switch(state)
        {
            case State.MainMenu:
                Canvas_MainMenu.SetActive(true);
                SoundManager.Instance.PlayBackgroundMusic();
                break;
            case State.TopicSelect:
                Canvas_Topics.SetActive(true);
                break;
            case State.TopicSubmit:
                Canvas_LoadingScreen.SetActive(true);
                SoundManager.Instance.FadeOutMusic(3f);
                OpenAI.ChatGPT.Instance.ValidateTopics();
                break;
            case State.RoundStart:
                Canvas_GameScreen.SetActive(true);
                GameplayManager.Instance.StartNewRound();
                break;
            case State.OverallResults:
                Canvas_OverallResults.SetActive(true);
                break;
        }
    }

    private void StateExit(State state)
    {
        switch (state)
        {
            case State.MainMenu:
                Canvas_MainMenu.SetActive(false);
                break;
            case State.TopicSelect:
                Canvas_Topics.SetActive(false);
                break;
            case State.TopicSubmit:
                Canvas_LoadingScreen.SetActive(false);
                break;
            case State.RoundStart:
                Canvas_GameScreen.SetActive(false);
                break;
            case State.OverallResults:
                Canvas_OverallResults.SetActive(false);
                break;
        }
    }

    # endregion

    public void ThrowError(string err, State revertState = State.None)
    {
        Debug.LogError(err);
        Canvas_ErrorMessage.SetActive(true);
        Canvas_ErrorMessage.GetComponentInChildren<TMP_Text>().text = err;
        if (revertState != State.None)
        {
            ChangeToState(revertState);
        }
    }
}
