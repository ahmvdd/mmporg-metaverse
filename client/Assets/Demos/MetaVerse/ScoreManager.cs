using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public TMP_Text ScoreText;
    public GameObject GameOverPanel;
    public TMP_Text GameOverText;

    private int score = 0;
    private bool gameEnded = false;

    void Awake() { Instance = this; }

    public void AddScore(int points)
    {
        if (gameEnded) return;
        score += points;
        if (ScoreText) ScoreText.text = "Score : " + score;
        NetworkManager.Instance?.Send($"SCORE|{NetworkManager.Instance.PlayerId}|{score}");

        if (score >= 150)
        {
            NetworkManager.Instance?.Send($"GAME_OVER|{NetworkManager.Instance.PlayerId}");
            ShowGameOver("Tu as gagné !");
        }
    }

    public void OnOpponentWon()
    {
        ShowGameOver("L'adversaire a gagné !");
    }

    private void ShowGameOver(string message)
    {
        if (gameEnded) return;
        gameEnded = true;
        if (GameOverText) GameOverText.text = message;
        if (GameOverPanel) GameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}
