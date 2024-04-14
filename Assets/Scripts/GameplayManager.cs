using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class GameplayManager : Singleton<GameplayManager>
{
    [SerializeField] private GameObject Container_Answers;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private List<AnswerButtonController> answers;
    public int ShowAnswersDelay;
    public int TimePerQuestion;
    public int ResultsScreenDuration;
    private int totalScore;
    private int _changedAnswerCount = 0;
    private AnswerButtonController _chosenAnswer;
    private GameQuestion _currentQuestion;
    private List<GameQuestion> questions;
    private int _timeRemaining;
    private int _timeRemainingWhenAnswer;

    # region Non-Loop Methods
    public void OnAnswerClicked(AnswerButtonController a)
    {
        answers.ForEach((answer)=>{answer.Enable();});
        a.Disable();
        _chosenAnswer = a;
        _changedAnswerCount++;
        _timeRemainingWhenAnswer = _timeRemaining;
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

    # endregion

    # region Gameplay Flow (in call order)
    public void StartNewRound()
    {
        questions = ChatResponseSerializer.Instance.Questions;
        TryStartQuestion();
    }

    private void TryStartQuestion()
    {
        if (questions.Count > 0)
        {
            int randomQuestionIndex = UnityEngine.Random.Range(0, questions.Count-1);
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
        _chosenAnswer = null;
        totalScore = 0;
        _timeRemainingWhenAnswer = 0;
        _currentQuestion = null;
        questions.Clear();
        StateMachine.Instance.ChangeToState(State.OverallResults);
    }

    private void StartQuestion(GameQuestion q)
    {
        _currentQuestion = q;
        questionText.text = q.question;
        var shuffledAnswers = q.answers.OrderBy( x => Random.value ).ToArray();
        for (int i = 0; i < 4; i++)
        {
            answers[i].SetAnswer(shuffledAnswers[i]);
            if (answers[i].AnswerText == q.correctAnswer)
            {
                answers[i].IsCorrectAnswer = true;
            }
        }
        StartCoroutine(ShowAnswers());
    }

    private IEnumerator ShowAnswers()
    {
        yield return new WaitForSeconds(ShowAnswersDelay);
        SoundManager.Instance.PlaySFX(Random.Range(0,2));
        Container_Answers.SetActive(true);
        StartCoroutine(StartTimer(TimePerQuestion));
    }

    private IEnumerator StartTimer(int totalTime)
    {
        _timeRemaining = totalTime;
        while (_timeRemaining >= 0)
        {
            timerText.text = "Time: " + _timeRemaining;
            yield return new WaitForSeconds(1);
            _timeRemaining -= 1;
        }

        StartCoroutine(OnRoundTimerEnd());
    }

    private IEnumerator OnRoundTimerEnd()
    {
        answers.ForEach((answer)=>
        {
            answer.Enable();
            if (answer.IsCorrectAnswer) answer.ChangeButtonColor(Color.green);
            else answer.ChangeButtonColor(Color.red);
        });

        if (_chosenAnswer != null && _chosenAnswer.IsCorrectAnswer)
        {
            SoundManager.Instance.PlaySFX(2);
            Debug.Log("Correct! It was: " + _chosenAnswer.AnswerText);
            Debug.Log("Your round score: " + GetQuestionScore());
            totalScore += GetQuestionScore();
            scoreText.text = "Score: " + totalScore;
        }

        yield return new WaitForSeconds(ResultsScreenDuration);

        Container_Answers.SetActive(false);
        answers.ForEach((answer)=>{answer.Reset();});
        TryStartQuestion();
    }

    # endregion
}
