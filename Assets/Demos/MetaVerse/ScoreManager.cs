using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public TMP_Text ScoreText;
    private int score = 0;

    void Awake() { Instance = this; }

    public void AddScore(int points)
    {
        score += points;
        if (ScoreText) ScoreText.text = "Score : " + score;
    }
}
