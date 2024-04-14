using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoIdleAnimation : MonoBehaviour
{
    [SerializeField] private AnimationCurve _idleCurve;
    [SerializeField] private float _duration;
    [SerializeField] private bool _loop;

    void OnEnable()
    {
        StartAnimation();
    }

    private void StartAnimation()
    {
        StartCoroutine(ContinueAnimation());
    }

    private IEnumerator ContinueAnimation()
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
}
