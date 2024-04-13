using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

// via: https://github.com/srcnalt/OpenAI-Unity

namespace OpenAI
{
    public class ChatGPT : Singleton<ChatGPT>
    {
        [SerializeField] private List<TMP_InputField> inputFields = new();
        private OpenAIApi openai;
        private List<ChatMessage> messages = new();
        private static string prompt = "You give trivia questions. For each list of topics I give you, give me 5 questions for each topic with 4 possible answers each. Desired format: <question> \n#<answer_1> \n#<answer_2> \n#<answer_3> \n*<correct_answer> \nTopics: ";
        private static Regex alphanumeric = new("^[a-zA-Z0-9.'& ]+$");
        public static event Action<string, string> OnChatCompletion;
        public static event Action<string> OnChatError;

        void Start()
        {
            APIGetter.OnGetAPIKey += APIGetter_OnGetAPIKey;
        }

        void OnDestroy()
        {
            APIGetter.OnGetAPIKey -= APIGetter_OnGetAPIKey;
        }

        void APIGetter_OnGetAPIKey(string key)
        {
            openai = new(key);
        }

        private bool ValidateString(string text)
        {
            if(alphanumeric.IsMatch(text)) return true;
            OnChatError?.Invoke("You just put an ILLEGAL character!");
            return false;
        }

        public void ValidateTopics()
        {
            string topics = "";

            foreach(TMP_InputField field in inputFields)
            {
                field.enabled = false;

                if (field.text != "" && ValidateString(field.text.Trim()))
                {
                    topics += field.text.Trim() + ", ";
                }
            }

            if (topics == "")
            {
                OnChatError?.Invoke("You must input at least one topic!");
            }
            else
            {
                Debug.Log("Topics chosen: " + topics);
                MakeQuestions(topics);
            }

            foreach(TMP_InputField field in inputFields)
            {
                field.enabled = true;
            }
        }

        public async void MakeQuestions(string topics)
        {
            messages.Clear();
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = prompt + "\n" + topics,
            };
            messages.Add(newMessage);
            
            // Complete the instruction
            OpenAI.CreateChatCompletionResponse completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo",
                Messages = messages
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                message.Content = message.Content.Trim();

                Debug.Log("Chat request completed for: " + topics);
                OnChatCompletion?.Invoke(topics, message.Content);
            }
            else
            {
                string errorMsg = "No text was generated from this prompt.";
                Debug.LogWarning(errorMsg);
                OnChatError?.Invoke(errorMsg);
            }
        }
    }
}
