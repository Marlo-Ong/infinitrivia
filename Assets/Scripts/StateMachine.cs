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
    QuestionDisplay,
    Answering,
    QuestionResults,
    OverallResults,
}

public class StateMachine : Singleton<StateMachine>
{
    public State CurrentState;
    public State PreviousState;
    [SerializeField] private GameObject Canvas_MainMenu;
    [SerializeField] private GameObject Canvas_Topics;
    [SerializeField] private GameObject Canvas_LoadingScreen;
    [SerializeField] public GameObject Canvas_ErrorMessage;

    # region Initializers

    void Start()
    {
        StateEnter(State.MainMenu);

        OpenAI.ChatGPT.OnChatCompletion += OpenAI_ChatGPT_OnChatCompletion;
        OpenAI.ChatGPT.OnChatError += OpenAI_ChatGPT_OnChatError;
    }

    void OnDestroy()
    {
        OpenAI.ChatGPT.OnChatCompletion -= OpenAI_ChatGPT_OnChatCompletion;
        OpenAI.ChatGPT.OnChatError -= OpenAI_ChatGPT_OnChatError;
    }

    void Update()
    {
        //StateUpdate(CurrentState);
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
                break;
            case State.TopicSelect:
                Canvas_Topics.SetActive(true);
                break;
            case State.TopicSubmit:
                Canvas_LoadingScreen.SetActive(true);
                OpenAI.ChatGPT.Instance.ValidateTopics();
                break;
            case State.RoundStart:
                break;
            case State.QuestionDisplay:
                break;
            case State.Answering:
                break;
            case State.QuestionResults:
                break;
            case State.OverallResults:
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
                break;
            case State.QuestionDisplay:
                break;
            case State.Answering:
                break;
            case State.QuestionResults:
                break;
            case State.OverallResults:
                break;
        }
    }

    private void StateUpdate(State state)
    {
        switch (state)
        {
            case State.MainMenu:
                break;
            case State.TopicSelect:
                break;
            case State.RoundStart:
                break;
            case State.QuestionDisplay:
                break;
            case State.Answering:
                break;
            case State.QuestionResults:
                break;
            case State.OverallResults:
                break;
        }
    }

    # endregion

    private void OpenAI_ChatGPT_OnChatCompletion(string _, string __)
    {
        if (CurrentState == State.TopicSubmit) 
        {
            ChangeToState(State.RoundStart);
        }
    }

    private void OpenAI_ChatGPT_OnChatError(string err)
    {
        if (CurrentState == State.TopicSubmit) 
        {
            Canvas_ErrorMessage.SetActive(true);
            Canvas_ErrorMessage.GetComponentInChildren<TMP_Text>().text = err;
            ChangeToState(State.TopicSelect);
        }
    }
}
