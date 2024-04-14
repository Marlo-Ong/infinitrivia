using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameplayManager : Singleton<GameplayManager>
{
    [SerializeField] private GameObject Canvas_GameScreen;
    [SerializeField] private GameObject Container_Answers;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private List<AnswerButtonController> answers;
    public int ShowAnswersDelay;
    public int TimePerQuestion;
    private int _changedAnswerCount = 0;
    private AnswerButtonController _chosenAnswer;
    private GameQuestion _currentQuestion;
    private List<GameQuestion> questions;
    private int _timeRemaining;
    private int _timeRemainingWhenAnswer;

    public void StartNewRound()
    {
        Canvas_GameScreen.SetActive(true);
        questions = ChatResponseSerializer.Instance.Questions;
        TryStartQuestion();
    }

    private void TryStartQuestion()
    {
        if (questions.Count > 0)
        {
            int randomQuestionIndex = Random.Range(0, questions.Count-1);
            GameQuestion randomQuestion = questions[randomQuestionIndex];
            questions.RemoveAt(randomQuestionIndex);
            StartQuestion(randomQuestion);
        }

        else
        {
            OnGameEnd();
        }
    }

    private void OnGameEnd()
    {
        Debug.Log("No more questions!");
    }

    public void OnAnswerClicked(AnswerButtonController a)
    {
        answers.ForEach((answer)=>{answer.Enable();});
        a.Disable();
        _chosenAnswer = a;
        _changedAnswerCount++;
        _timeRemainingWhenAnswer = _timeRemaining;
    }

    private void StartQuestion(GameQuestion q)
    {
        _currentQuestion = q;
        questionText.text = q.question;
        for (int i = 0; i < 4; i++)
        {
            answers[i].SetAnswer(q.answers[i]);
            if (answers[i].AnswerText == q.correctAnswer)
            {
                answers[i].IsCorrectAnswer = true;
            }
        }
        Container_Answers.SetActive(true);

        StartCoroutine(StartTimer(TimePerQuestion));
    }

    private void OnRoundTimerEnd()
    {
        StateMachine.Instance.ChangeToState(State.QuestionResults);
        Container_Answers.SetActive(false);

        if (_chosenAnswer != null && _chosenAnswer.IsCorrectAnswer)
        {
            Debug.Log("Correct! It was: " + _chosenAnswer.AnswerText);
            Debug.Log("Your score: " + GetQuestionScore());
        }
        TryStartQuestion();
    }

    private int GetQuestionScore()
    {
        int baseCorrectAnswerScore = 50; 
        int speedBonus = 100 * (_timeRemainingWhenAnswer / TimePerQuestion);

        int steadfastBonus = 100;
        if (_changedAnswerCount == 2) steadfastBonus /= 2;
        else if (_changedAnswerCount == 3) steadfastBonus /= 4;
        else if (_changedAnswerCount > 3) steadfastBonus = 0;

        return baseCorrectAnswerScore + speedBonus + steadfastBonus;
    }

    private IEnumerator StartTimer(int totalTime)
    {
        _timeRemaining = totalTime;
        while (_timeRemaining > 0)
        {
            timerText.text = "Time: " + _timeRemaining;
            yield return new WaitForSeconds(1);
            _timeRemaining -= 1;
        }

        OnRoundTimerEnd();
    }
}
