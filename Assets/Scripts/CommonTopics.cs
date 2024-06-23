using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class CommonTopics : MonoBehaviour
{
    public float TopicDisplayDuration;
    public TMP_Text TopicText;
    public IdleTextAnimation TopicPopInAnim;
    public GameObject TopicsSection;
    private string[] topics;
    private bool hasTopicLoopStarted;
    private bool stopTopicLoop = false;

    void OnEnable()
    {
        APIGetter.OnGetQuestions += APIGetter_OnGetQuestions;
    }

    void OnDisable()
    {
        APIGetter.OnGetQuestions -= APIGetter_OnGetQuestions;
    }

    private void APIGetter_OnGetQuestions(List<Question> questions)
    {
        if (this.hasTopicLoopStarted || this.TopicText == null || this.TopicsSection == null)
            return;

        this.topics = questions
            .Select(x => x.topic)
            .OrderBy(x => Random.Range(-1.0f, 1.0f))
            .ToHashSet()
            .ToArray();

        if (this.topics.Length < 1)
        {
            Destroy(this.TopicsSection);
            return;
        }

        this.hasTopicLoopStarted = true;
        this.TopicsSection.SetActive(true);
        StartCoroutine(DisplayTopicLoop(0));
    }

    private IEnumerator DisplayTopicLoop(int index)
    {
        this.TopicText.text = this.topics[index];
        this.TopicPopInAnim.StartAnimation();

        yield return new WaitForSeconds(this.TopicDisplayDuration);

        if (!stopTopicLoop)
        {
            int nextTopicIndex = index + 1;
            nextTopicIndex %= this.topics.Length;
            StartCoroutine(DisplayTopicLoop(nextTopicIndex));
        }
    }
}
