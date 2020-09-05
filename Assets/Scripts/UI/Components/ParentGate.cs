using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class ParentGate : Core
{
    public Text questionText;
    public Color rightColor;
    public Color wrongColor;

    class Question
    {
        public string text;
        public int answer;
        public Question(string text, int answer)
        {
            this.text = text;
            this.answer = answer;
        }
    }

    Question[] questions = new Question[] {
        new Question("1 + 5", 6),
        new Question("2 + 2", 4),
        new Question("3 + 3", 6),
        new Question("4 + 1", 5),
        new Question("5 + 4", 9),
        new Question("6 + 2", 8),
        new Question("7 + 1", 8),
        new Question("9 - 8", 1),
        new Question("8 - 4", 4),
        new Question("7 - 5", 2),
        new Question("6 - 2", 4),
        new Question("5 - 3", 2),
        new Question("4 - 1", 3),
        new Question("2 * 2", 4),
        new Question("3 * 2", 6),
        new Question("1 * 7", 7),
        new Question("4 * 2", 8),
        new Question("6 * 1", 6),
        new Question("3 * 3", 9),
        new Question("9 / 3", 3),
        new Question("8 / 2", 4),
        new Question("6 / 6", 1),
        new Question("4 / 2", 2),
        new Question("3 / 1", 3),
    };
    Question question;

    Action pass;
    Action block;
    public void Show(Action pass, Action block = null)
    {
        this.pass = pass;
        this.block = block;

        gameObject.SetActive(true);

        SetupQuestion();
    }


    void SetupQuestion()
    {
        question = questions[UnityEngine.Random.Range(0, questions.Length)];

        questionText.text = question.text + " = ?";
    }

    public void NumberSelect(int n)
    {
        if (isAnimation) return;

        StartCoroutine(AnswerAnimation(n));
    }

    bool isAnimation = false;
    IEnumerator AnswerAnimation(int n)
    {
        isAnimation = true;

        questionText.text = question.text + " = " + n;

        if (n == question.answer)
        {
            questionText.color = rightColor;
            yield return new WaitForSeconds(1f);
            questionText.color = Color.white;

            gameObject.SetActive(false);

            if (pass != null) pass();
        }
        else
        {
            questionText.color = wrongColor;
            yield return new WaitForSeconds(1f);
            questionText.color = Color.white;

            SetupQuestion();
        }

        isAnimation = false;
    }

    public void Close()
    {
        if (isAnimation) return;

        gameObject.SetActive(false);

        if (block != null) block();
    }
}
