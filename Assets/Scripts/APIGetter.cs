using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// via https://medium.com/bina-nusantara-it-division/how-to-fetch-data-from-api-in-unity-99e58820b2d4

[System.Serializable]
public class Key
{
    public string api_key;
}

public class APIGetter : Singleton<APIGetter>
{
    public static string URL = "https://66199d83125e9bb9f29a6b60.mockapi.io";
    public static event Action<string> OnGetAPIKey;

    void Start()
    {
        StartCoroutine(FetchData());
    }

    /// <remarks>
    /// FromJson doesn't work when arrays are root element,
    /// but mockapi.io always requires arrays as root
    /// [ and ] characters are removed from the JSON string
    /// </remarks>
    public IEnumerator FetchData()
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
            JsonUtility.ToJson(myKey);
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
        Debug.Log(stringifiedQuestion);
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
}