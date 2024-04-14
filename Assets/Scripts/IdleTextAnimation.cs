using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;

public enum IdleAnimation
{
    Rotate,
    PulseScale,
    FadeAndRaise,
}

public class IdleTextAnimation : MonoBehaviour
{
    [SerializeField] private IdleAnimation anim;
    [SerializeField] private AnimationCurve _idleCurve;
    [SerializeField] private float _duration;
    [SerializeField] private bool _loop;
    [SerializeField] private float _raiseMagnitude;
    private Vector3 _startingPos;

    void Start()
    {
        _startingPos = transform.localPosition;
    }

    void OnEnable()
    {
        StartAnimation();
    }

    private void StartAnimation()
    {
        switch (anim)
        {
            case IdleAnimation.Rotate:
                StartCoroutine(AnimateRotation());
                break;
            case IdleAnimation.PulseScale:
                StartCoroutine(AnimatePulseScale());
                break;
            case IdleAnimation.FadeAndRaise:
                StartCoroutine(AnimateFadeAndRaise());
                break;
        }
    }

    private IEnumerator AnimateRotation()
    {
        float elapsedTime = 0;
        while (elapsedTime < _duration)
        {
            Quaternion newRotation = transform.localRotation;
            newRotation.z = _idleCurve.Evaluate(elapsedTime / _duration);
            transform.localRotation = newRotation;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (_loop) StartAnimation();
    }

    private IEnumerator AnimatePulseScale()
    {
        float elapsedTime = 0;
        while (elapsedTime < _duration)
        {
            float scale = _idleCurve.Evaluate(elapsedTime / _duration);
            transform.localScale = new Vector3(scale, scale, scale);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (_loop) StartAnimation();
    }

    private IEnumerator AnimateFadeAndRaise()
    {
        float elapsedTime = 0;
        while (elapsedTime < _duration)
        {
            GetComponent<TMP_Text>().alpha = 1 - (elapsedTime / _duration);
            transform.localPosition = new(transform.localPosition.x, transform.localPosition.y + _raiseMagnitude);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = _startingPos;
        gameObject.SetActive(false);
        if (_loop) StartAnimation();
    }
}
