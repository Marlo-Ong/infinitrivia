using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenAI;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Version of question object with in-game properties
/// </summary>
public struct GameQuestion
{
    public int number;
    public string topic;
    public string question;
    public List<string> answers;
    public string correctAnswer;
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
        SerializeChatResponse(response);

        foreach(GameQuestion q in Questions)
        {
            APIGetter.Instance.PostQuestion(JSONifyQuestion(q));
        }
    }

    private void SerializeChatResponse(string response)
    {
        int topicsIndex = 0;
        GameQuestion currentQuestion = new()
        {
            answers = new()
        };

        foreach (string line in response.Split('\n'))
        {
            if (line != "") 
            {
                Debug.Log(line);
                if (Regex.IsMatch(line, "^[0-9]. (.*)$")) // line is a numbered question
                {
                    // add a copy of this question to the list
                    GameQuestion newQ = new()
                    {
                        answers = new()
                    };

                    newQ = currentQuestion;
                    Questions.Add(newQ);

                    // reset question to act as new
                    currentQuestion.answers.Clear();
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
        }
    
        Questions.RemoveAt(0); // remove junk currentQuestion
    }

    private Question JSONifyQuestion(GameQuestion q)
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
}
