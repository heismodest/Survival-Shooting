using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{

    public static int score { get; private set;}
    public int bestScore;
    float lastEnemyKillTime;
    int streakCount;
    float streakExpiryTime = 1;


    void Start()
    {
        bestScore = PlayerPrefs.GetInt ("BestScore", score);    //bestScore를 불러옴
        Enemy.OnDeathStatic += OnEnemyKilled;
        FindObjectOfType<Player> ().OnDeath += OnPlayerDeath;
    }

    void OnEnemyKilled()
    {
        if (Time.time < lastEnemyKillTime + streakExpiryTime)
        {
            streakCount ++;
        }
        else
        {
            streakCount = 0;
        }
            score += 5 + (int) Mathf.Pow(2, streakCount);
    }
    void OnPlayerDeath()
    {
        Enemy.OnDeathStatic -= OnEnemyKilled;
        if (bestScore < score)
        {
            PlayerPrefs.SetInt ("BestScore", score);
            PlayerPrefs.Save ();
        }
        score = 0;  //reset score 하지 않으면 점수가 계속 누적
    }

}
