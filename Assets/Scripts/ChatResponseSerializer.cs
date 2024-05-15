using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenAI;
using UnityEngine;

/// <summary>
/// Version of question object with in-game properties
/// </summary>
public class GameQuestion
{
    public string topic = "";
    public string question = "";
    public List<string> answers = new();
    public string correctAnswer = "";
    public GameQuestion DeepCopy()
    {
       GameQuestion other = (GameQuestion)MemberwiseClone();
       other.answers = answers.Select(a => new string(a)).ToList();
       other.topic = string.Copy(topic);
       other.question = string.Copy(question);
       other.correctAnswer = string.Copy(correctAnswer);
       return other;
    }
};

/// <summary>
/// Version of question that becomes JSONified
/// </summary>
[Serializable]
public class Question
{
    public string topic;
    public string answer1;
    public string answer2;
    public string answer3;
    public string answer4;
    public string correct_answer;
    public string question;
}

public class ChatResponseSerializer : Singleton<ChatResponseSerializer>
{
    public List<string> Topics = new();
    public List<GameQuestion> Questions = new();
    void Start()
    {
        OpenAI.ChatGPT.OnChatCompletion += OpenAI_ChatGPT_OnChatCompletion;
    }

    void OnDestroy()
    {
        OpenAI.ChatGPT.OnChatCompletion -= OpenAI_ChatGPT_OnChatCompletion;
    }

    private void OpenAI_ChatGPT_OnChatCompletion(string rTopics, string response)
    {
        Topics.Clear();
        Questions.Clear();

        Topics = rTopics.Split(',').ToList();
        for (int i = 0; i < Topics.Count; i++)
        {
            Topics[i] = Topics[i].Trim().ToLower();
        }

        if (SerializeChatResponse(response))
        {
            foreach(GameQuestion q in Questions)
            {
                Question json = JSONifyQuestion(q);
                if (json != null)
                {
                    APIGetter.Instance.PostQuestion(json);
                }
            }
        }
    }

    /// <returns> Boolean success of try-catch serialization </returns>
    private bool SerializeChatResponse(string response)
    {
        try
        {
            int topicsIndex = 0;
            GameQuestion currentQuestion = new()
            {
                answers = new()
            };

            foreach (string line in response.Split('\n'))
            {
                if (line == "") continue;

                Debug.Log(line);
                if (Regex.IsMatch(line, "^[0-9]. (.*)$") || line.StartsWith("Question")) // line is a numbered question
                {
                    // add a copy of this question to the list
                    currentQuestion.answers.Distinct().ToList();
                    GameQuestion newQ = currentQuestion.DeepCopy();
                    Questions.Add(newQ);

                    // reset question to act as new
                    currentQuestion.answers.Clear();
                    currentQuestion.correctAnswer = "";
                    currentQuestion.question = line[3..line.Length];
                }

                if (line[0] == '1') // new topic
                {
                    currentQuestion.topic = Topics[topicsIndex++];
                }

                if (line[0] == '#') // line begins with # (custom designator for wrong answer)
                {
                    currentQuestion.answers.Add(line[1..line.Length]);
                }

                else if (line[0] == '*') // line beings with * (custom designator for right answer)
                {
                    string answerText = line[1..line.Length];
                    currentQuestion.answers.Add(answerText);
                    currentQuestion.correctAnswer = answerText;
                }
            }
        
            // add last question
            currentQuestion.answers.Distinct().ToList();
            GameQuestion lastQuestion = currentQuestion.DeepCopy();
            Questions.Add(lastQuestion);

            Questions.RemoveAt(0); // remove junk first question
            StateMachine.Instance.ChangeToState(State.RoundStart);
            return true;
        }

        catch (Exception err)
        {
            StateMachine.Instance.ThrowError(err.Message, "Serialization of ChatGPT response failed - please try again", State.MainMenu);
            return false;
        }
    }

    private Question JSONifyQuestion(GameQuestion q)
    {
        try
        {
            Question newQ = new()
            {
                topic = q.topic,
                answer1 = q.answers[0],
                answer2 = q.answers[1],
                answer3 = q.answers[2],
                answer4 = q.answers[3],
                correct_answer = q.correctAnswer,
                question = q.question
            };
            return newQ;
        }

        catch (Exception err)
        {
            Debug.Log(err.Message); // non-essential error
        }

        return null;

    }
}
