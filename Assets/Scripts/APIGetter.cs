using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

// via https://medium.com/bina-nusantara-it-division/how-to-fetch-data-from-api-in-unity-99e58820b2d4

[Serializable]
public class Key
{
    public string api_key;
}

[Serializable]
public class Wrapper<T>
{
    public T[] Items;
}

public class APIGetter : Singleton<APIGetter>
{
    public static string URL = "https://66199d83125e9bb9f29a6b60.mockapi.io";
    public static event Action<string> OnGetAPIKey;
    public static event Action<List<Question>> OnGetQuestions;

    void Start()
    {
        StartCoroutine(FetchAPIKey());
        StartCoroutine(FetchQuestions());
    }

    /// <remarks>
    /// FromJson doesn't work when arrays are root element,
    /// but mockapi.io always requires arrays as root
    /// [ and ] characters are removed from the JSON string
    /// </remarks>
    public IEnumerator FetchAPIKey()
    {
        using UnityWebRequest req = UnityWebRequest.Get(URL + "/openai");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(req.error);
        }
        else
        {
            Key myKey = JsonUtility.FromJson<Key>(req.downloadHandler.text[1..^1]);
            Debug.Log("Got API key: " + myKey.api_key);
            OnGetAPIKey?.Invoke(myKey.api_key);
        }
    }

    public void PostQuestion(Question q)
    {
        StartCoroutine(ContinuePostQuestion(q));
    }
    
    private IEnumerator ContinuePostQuestion(Question q)
    {
        string stringifiedQuestion = JsonUtility.ToJson(q);
        using UnityWebRequest req = UnityWebRequest.Post(URL + "/questionsDB", stringifiedQuestion, "application/json");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError(req.error);
        }
        else
        {
            Debug.Log("Uploaded question to DB: " + q.question);
        }
    }

    public IEnumerator FetchQuestions()
    {
        using UnityWebRequest req = UnityWebRequest.Get(URL + "/questionsDB");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(req.error);
        }
        else
        {
            Wrapper<Question> questionWrapper = JsonUtility.FromJson<Wrapper<Question>>("{\"Items\":" + req.downloadHandler.text + "}");
            List<Question> questions = questionWrapper.Items.ToList();
            Debug.Log("Got questions: count " + questions.Count);
            OnGetQuestions?.Invoke(questions);
        }
    }
}