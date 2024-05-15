using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class GameplayManager : Singleton<GameplayManager>
{
    [SerializeField] private GameObject Container_Answers;
    [SerializeField] private GameObject Container_ScoreHistory;
    [SerializeField] private GameObject checkmarkIcon;
    [SerializeField] private GameObject redxIcon;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text firstTryBonusText;
    [SerializeField] private TMP_Text speedBonusText;
    [SerializeField] private TMP_Text correctAnswerBonusText;
    [SerializeField] private List<AnswerButtonController> answerButtons;
    public int ShowAnswersDelay;
    public int TimePerQuestion;
    public int ResultsScreenDuration;
    private int totalScore;
    private int _changedAnswerCount = 0;
    private AnswerButtonController _chosenAnswer;
    private GameQuestion _currentQuestion;
    private List<GameQuestion> questions;
    private int _timeRemaining;
    private int _answerTime;
    private List<GameObject> _scoreHistoryIcons;

    # region Non-Loop Methods
    public void OnAnswerClicked(AnswerButtonController a)
    {
        answerButtons.ForEach((answer)=>{answer.Enable();});
        a.Disable();
        _chosenAnswer = a;
        _changedAnswerCount++;
        _answerTime = _timeRemaining;
    }

    private int GetQuestionScore()
    {
        int baseCorrectAnswerScore = 50; 
        float preSpeedBonus = (float)_answerTime / (float)TimePerQuestion;
        int speedBonus = (int)(100f * preSpeedBonus);

        int firstTryBonus = 100;
        if (_changedAnswerCount == 2) firstTryBonus /= 2;
        else if (_changedAnswerCount == 3) firstTryBonus /= 4;
        else if (_changedAnswerCount > 3) firstTryBonus = 0;

        if (speedBonus > 0)
        {
            speedBonusText.text = "+" + speedBonus + " speedy answer!";
            speedBonusText.gameObject.SetActive(true);
        }
        if (firstTryBonus > 0)
        {
            firstTryBonusText.text = "+" + firstTryBonus + " first try!";
            firstTryBonusText.gameObject.SetActive(true);
        }

        return baseCorrectAnswerScore + speedBonus + firstTryBonus;
    }

    # endregion

    # region Gameplay Flow (in call order)
    public void StartNewRound()
    {
        _scoreHistoryIcons = new();
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
        _answerTime = 0;
        _currentQuestion = null;
        _changedAnswerCount = 0;
        questions.Clear();

        // reset round score history
        foreach (GameObject icon in _scoreHistoryIcons)
        {
            Destroy(icon);
        }
        _scoreHistoryIcons.Clear();

        StateMachine.Instance.ChangeToState(State.OverallResults);
    }

    private void StartQuestion(GameQuestion q)
    {
        foreach (AnswerButtonController button in answerButtons)
        {
            button.gameObject.SetActive(false);
        }

        _currentQuestion = q;
        questionText.text = q.question;
        var shuffledAnswers = q.answers.OrderBy( x => Random.value ).ToArray();
        for (int i = 0; i < q.answers.Count; i++)
        {
            answerButtons[i].gameObject.SetActive(true);
            answerButtons[i].SetAnswer(shuffledAnswers[i]);
            if (answerButtons[i].AnswerText == q.correctAnswer)
            {
                answerButtons[i].IsCorrectAnswer = true;
            }
        }

        StartCoroutine(ShowAnswers());
    }

    private IEnumerator ShowAnswers()
    {
        yield return new WaitForSeconds(ShowAnswersDelay);
        SoundManager.Instance.PlaySFX(Random.Range(0,2));
        scoreText.gameObject.SetActive(false);
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
        answerButtons.ForEach((answer)=>
        {
            answer.Enable();
            if (answer.IsCorrectAnswer) answer.ChangeButtonColor(Color.green);
            else answer.ChangeButtonColor(Color.red);
        });

        if (_chosenAnswer != null)
        {
            if (_chosenAnswer.IsCorrectAnswer)
            {
                GetCorrectAnswer();
            }
            else
            {
                GetIncorrectAnswer();
            }
        }

        yield return new WaitForSeconds(ResultsScreenDuration);
        SoundManager.Instance.PlaySFX(5);

        Container_Answers.SetActive(false);
        scoreText.gameObject.SetActive(true);
        answerButtons.ForEach((answer)=>{answer.Reset();});
        TryStartQuestion();
    }

    private void GetCorrectAnswer()
    {
        SoundManager.Instance.PlaySFX(2);
        correctAnswerBonusText.gameObject.SetActive(true);
        AddIconToHistory(checkmarkIcon);
        totalScore += GetQuestionScore();
        scoreText.text = "Score: " + totalScore;
    }

    private void GetIncorrectAnswer()
    {
        AddIconToHistory(redxIcon);
        SoundManager.Instance.PlaySFX(3);
    }

    private GameObject AddIconToHistory(GameObject parentPrefab)
    {
        GameObject icon = Instantiate(parentPrefab);
        icon.transform.SetParent(Container_ScoreHistory.transform, worldPositionStays: false);
        _scoreHistoryIcons.Add(icon);
        icon.SetActive(true);
        icon.transform.localScale = new(1,1,1);

        return icon;
    }

    # endregion
}
