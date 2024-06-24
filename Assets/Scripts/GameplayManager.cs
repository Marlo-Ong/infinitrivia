using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class GameplayManager : Singleton<GameplayManager>
{
    [SerializeField] private GameObject Container_Answers;
    [SerializeField] private GameObject Container_ScoreHistory;
    [SerializeField] private GameObject scoreBoxPrefab;
    [SerializeField] private Sprite checkmarkIcon;
    [SerializeField] private Sprite redxIcon;
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
    private int _currentQuestionIndex;
    private int _timeRemaining;
    private int _answerTime;
    private int _totalQuestionCount;
    private List<GameObject> _scoreHistoryIcons;
    private Coroutine showAnswersCoroutine;
    private Coroutine startTimerCoroutine;
    private Coroutine roundEndCoroutine;

    # region Non-Loop Methods
    public void OnAnswerClicked(AnswerButtonController a)
    {
        if (_chosenAnswer != a)
        {
            answerButtons.ForEach((answer)=>{answer.Enable();});
            a.Disable();
            if (_chosenAnswer != null) _changedAnswerCount++;
            _chosenAnswer = a;
            _answerTime = _timeRemaining;
        }
    }

    private int GetQuestionScore()
    {
        int baseCorrectAnswerScore = 50; 
        float preSpeedBonus = (float)_answerTime / (float)TimePerQuestion;
        int speedBonus = (int)(100f * preSpeedBonus);

        int firstTryBonus = _changedAnswerCount == 0 ? 100 : 0;

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

    public void Interrupt_QuitToHome()
    {
        if (this.showAnswersCoroutine != null)
            StopCoroutine(this.showAnswersCoroutine);
        if (this.startTimerCoroutine != null)
            StopCoroutine(this.startTimerCoroutine);
        if (this.roundEndCoroutine != null)
            StopCoroutine(this.roundEndCoroutine);

        OnGameEnd();
        StateMachine.Instance.ChangeToState(State.MainMenu);
    }

    # endregion

    # region Gameplay Flow (in call order)
    public void StartNewRound()
    {
        questions = ChatResponseSerializer.Instance.Questions;

        // Populate score bar
        _scoreHistoryIcons = new();
        for (int i = 0; i < questions.Count; i++)
        {
            GameObject icon = Instantiate(scoreBoxPrefab);
            icon.transform.GetChild(0).GetComponent<TMP_Text>().text = (i + 1).ToString();
            icon.transform.SetParent(Container_ScoreHistory.transform, worldPositionStays: false);
            _scoreHistoryIcons.Add(icon);
            icon.SetActive(true);
        }

        _totalQuestionCount = questions.Count;
        _currentQuestionIndex = -1;
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
            if (StateMachine.Instance.CurrentState == State.RoundStart)
                StateMachine.Instance.ChangeToState(State.OverallResults);
        }
    }

    private void OnGameEnd()
    {
        SoundManager.Instance.StopCurrentAudio();

        // reset game variables
        totalScore = 0;
        _currentQuestion = null;
        _currentQuestionIndex = -1;
        questions?.Clear();

        // reset round score history
        if (_scoreHistoryIcons != null)
        {
            foreach (GameObject icon in _scoreHistoryIcons)
            {
                Destroy(icon);
            }
            _scoreHistoryIcons.Clear();
        } 

        // reset round variables
        Container_Answers.SetActive(false);
        _changedAnswerCount = 0;
        _answerTime = 0;
        _chosenAnswer = null;
        answerButtons.ForEach((answer)=>{answer.Reset();});
    }

    private void StartQuestion(GameQuestion q)
    {
        foreach (AnswerButtonController button in answerButtons)
        {
            button.gameObject.SetActive(false);
        }

        _currentQuestion = q;
        _currentQuestionIndex++;
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

        if (_chosenAnswer != null && _chosenAnswer.IsCorrectAnswer)
        {
            GetCorrectAnswer();
        }
        else
        {
            GetIncorrectAnswer();
        }

        yield return new WaitForSeconds(ResultsScreenDuration);
        SoundManager.Instance.PlaySFX(5);

        Container_Answers.SetActive(false);
        scoreText.gameObject.SetActive(true);

        // reset round variables
        _changedAnswerCount = 0;
        _answerTime = 0;
        _chosenAnswer = null;
        answerButtons.ForEach((answer)=>{answer.Reset();});
        TryStartQuestion();
    }

    private void GetCorrectAnswer()
    {
        SoundManager.Instance.PlaySFX(2);
        correctAnswerBonusText.gameObject.SetActive(true);
        ChangeScoreHistoryIcon(correctAnswer: true);
        totalScore += GetQuestionScore();
        scoreText.text = "Score: " + totalScore + "/" + (250 * _totalQuestionCount).ToString();
    }

    private void GetIncorrectAnswer()
    {
        ChangeScoreHistoryIcon(correctAnswer: false);
        SoundManager.Instance.PlaySFX(3);
    }

    private void ChangeScoreHistoryIcon(bool correctAnswer)
    {
        var checkBox = _scoreHistoryIcons[_currentQuestionIndex].transform.GetChild(1);
        checkBox.GetComponent<SpriteRenderer>().sprite = correctAnswer ? checkmarkIcon : redxIcon;
    }

    # endregion
}
