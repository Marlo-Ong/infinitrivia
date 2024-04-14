using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnswerButtonController : ButtonController
{
    private Button button {get {return GetComponent<Button>();}}
    [SerializeField] private TMP_Text text;
    public bool IsCorrectAnswer = false;
    public string AnswerText { get { return text.text; }}

    public void SetAnswer(string s)
    {
        text.text = s;
    }

    public void Enable() { button.interactable = true; }
    public void Disable() { button.interactable = false; }

    public void Reset()
    {
        ChangeButtonColor(Color.white);
        SetAnswer("");
        IsCorrectAnswer = false;
        Enable();
    }

    public void ChangeButtonColor(Color newColor)
    {
        var newColorBlock = button.colors;
        newColorBlock.normalColor = newColor;
        newColorBlock.disabledColor = newColor;
        button.colors = newColorBlock;
    }
}
