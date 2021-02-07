﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

using System.Net;
using System;
using System.IO;

public class GameController : MonoBehaviour
{
    int questionIndex = 0;
    List<Question> list = new List<Question>();
    public GameObject obstaclePrefab;

    float score = 0;
    public Text scoreText;

    public HealthBarScript health;

    // Start is called before the first frame update
    void Start()
    {
        list = GetData().list;
        LoadNewQuestion();
        SetDifficulty(1);
        StartCoroutine(ObstacleSpawner());
    }


    // Update is called once per frame
    void Update()
    {
        UpdateScore();
    }

    void UpdateScore()
    {
        score += Time.deltaTime;
        scoreText.text = "Score: " + Mathf.FloorToInt(score).ToString();
    }
    public QuizPanelScript quizPanel;

    public void OnAnswer(int guessIndex)
    {
        if (answerIndex == guessIndex)
        {
            print("CORRECT");
        }
        else
        {
            SetDifficulty(difficulty + 0.5f);
        }

        if (questionIndex < list.Count - 1)
        {
            questionIndex++;
            LoadNewQuestion();
        }
        else
        {
            Debug.LogError("REACHED END OF QUESTIONS LIST");
        }
    }

    int answerIndex = 0;
    public void LoadNewQuestion()
    {
        quizPanel.questionText.text = list[questionIndex].question;

        answerIndex = UnityEngine.Random.Range(0, 4);
        quizPanel.choice.SetChoices(list[questionIndex].ShuffleChoicesAndAnswer(answerIndex).ToArray());
        ResetTimer();
    }

    void ResetTimer()
    {
        if (currentCountdown != null)
            StopCoroutine(currentCountdown);
        currentCountdown = StartCoroutine(TimerCountdown(8f));
    }

    Coroutine currentCountdown;
    IEnumerator TimerCountdown(float seconds)
    {
        float timer = 0;
        while (timer < seconds)
        {
            timer += Time.deltaTime;
            quizPanel.timeBar.fillAmount = timer / seconds;
            yield return null;
        }
        AnswerTimeOut();
    }

    void AnswerTimeOut()
    {
        ResetTimer();
        SetDifficulty(difficulty + 0.5f);
    }

    float difficulty = 1f;
    public float spawnIntervalDifficultyConstant;
    void SetDifficulty(float newDifficulty)
    {
        difficulty = newDifficulty;
        obstacleSpawnInterval = 7f - difficulty * spawnIntervalDifficultyConstant;
    }

    public float obstacleSpawnInterval;
    IEnumerator ObstacleSpawner()
    {
        SpawnObstacle();
        yield return new WaitForSeconds(obstacleSpawnInterval);
        StartCoroutine(ObstacleSpawner());
    }

    Queue<ObstacleScript> obstacleQueue = new Queue<ObstacleScript>();
    public Transform obstacleSpawn;
    float WORLD_LEFT_BOUND = -9;
    void SpawnObstacle()
    {
        ObstacleScript obj;
        if (obstacleQueue.Count == 0 || obstacleQueue.Peek().transform.position.x > WORLD_LEFT_BOUND)
        {
            obj = Instantiate(obstaclePrefab, obstacleSpawn.position, Quaternion.identity).GetComponent<ObstacleScript>();
        }
        else
        {
            obj = obstacleQueue.Dequeue();
        }
        obj.transform.position = obstacleSpawn.position;
        obstacleQueue.Enqueue(obj);
    }









    [Serializable]
    public class Question
    {
        public string question;
        public List<string> choices;
        public string answer;

        //shuffles the choices, but ensuring the answer is at answer index (between 0 and 4[exclusive])
        public List<String> ShuffleChoicesAndAnswer(int answerIndex)
        {
            List<string> temp = new List<string>(choices);
            System.Random rng = new System.Random();
            rng.Shuffle(temp);
            temp.Insert(answerIndex, answer);
            return temp;
        }

    }

    [Serializable]
    public class QuestionList
    {
        public List<Question> list;
    }

    private QuestionList GetData()
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://troygamedev.github.io/trivia-game-data/questions.json");
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());
        string jsonResponse = reader.ReadToEnd();
        QuestionList q = JsonUtility.FromJson<QuestionList>(jsonResponse);
        return q;
    }
}


static class RandomExtensions
{
    public static void Shuffle<T>(this System.Random rng, List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            int k = rng.Next(n--);
            T temp = list[n];
            list[n] = list[k];
            list[k] = temp;
        }
    }
}
