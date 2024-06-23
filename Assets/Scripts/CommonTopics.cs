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
        TryStartLoop();
    }

    void OnDisable()
    {
        this.hasTopicLoopStarted = false;
        APIGetter.OnGetQuestions -= APIGetter_OnGetQuestions;
    }

    private void APIGetter_OnGetQuestions(List<Question> questions)
    {
        TryStartLoop();
    }

    private void TryStartLoop()
    {
        if (this.hasTopicLoopStarted || this.TopicText == null || this.TopicsSection == null || APIGetter.DBQuestions == null)
            return;

        this.topics = APIGetter.DBQuestions
            .Select(x => x.topic)
            .OrderBy(x => Random.Range(-1.0f, 1.0f))
            .ToHashSet()
            .ToArray();

        if (this.topics.Length < 1)
        {
            Destroy(this.TopicsSection);
            return;
        }

        StartTopicLoop();
    }

    private void StartTopicLoop()
    {
        this.hasTopicLoopStarted = true;
        this.TopicsSection.SetActive(true);
        StartCoroutine(DisplayTopicLoop(0));
    }

    private void StopTopicLoop()
    {
        this.hasTopicLoopStarted = false;
        this.TopicsSection.SetActive(false);
    }

    private IEnumerator DisplayTopicLoop(int index)
    {
        if (!this.TopicText.gameObject.activeInHierarchy)
        {
            StopTopicLoop();
            stopTopicLoop = true;
            yield break;
        }

        this.TopicText.text = this.topics[index];
        this.TopicPopInAnim.StartAnimation();

        yield return new WaitForSeconds(this.TopicDisplayDuration);

        if (!stopTopicLoop)
        {
            int nextTopicIndex = index + 1;
            nextTopicIndex %= this.topics.Length;
            StartCoroutine(DisplayTopicLoop(nextTopicIndex));
        }
        else
        {
            stopTopicLoop = false;
            StopTopicLoop();
            yield break;
        }
    }
}
