using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum RuleType
{
    Color,
    Rotation
}
public class GameManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public Transform gridParent;

    [Header("Rule Colors")]
    public Color normalColor = Color.blue;

    [Header("Fade Settings")]
    public float defaultFadeTime = 3f;
    private float fadeTime;

    [Header("Scoring")]
    public int Score = 100;
    public float timePenalty= 30f;
    public int wrongClickscore = 50;

    [Header("Adaptive Difficulty")]
    public int reactionCount = 5;
    public float fastReaction = 0.8f;
    public float slowReaction = 1.5f;

    [Header("Rotation Rule")]
    public float easyRotation = 45f;
    public float hardRotation = 20f;

    [Header("UI")]
    public Text scoreText;
    public Text wrongText;
    public Button stopButton;
    public Button resetButton;

    private List<Tile> tiles = new List<Tile>();
    private Queue<float> recentReactionTimes = new Queue<float>();

    private RuleType currentRule;
    private float roundStartTime;

    private int totalScore = 0;
    private int wrongClicks = 0;
    private bool gameRunning = true;

    void Start()
    {
        stopButton.onClick.AddListener(StopGame);
        resetButton.onClick.AddListener(ResetGame);
        fadeTime = defaultFadeTime;
        resetButton.gameObject.SetActive(false);

        UpdateUI();
        StartRound();
    }

    void StartRound()
    {
        if (!gameRunning) return;

        ClearGrid();
        roundStartTime = Time.time;

        int oddIndex = Random.Range(0, 9);
        currentRule = (RuleType)Random.Range(0, 2);

        for (int i = 0; i < 9; i++)
        {
            GameObject tileObj = Instantiate(tilePrefab, gridParent);
            Tile tile = tileObj.GetComponent<Tile>();

            bool isOdd = (i == oddIndex);
            tile.Setup(this, isOdd);

            ApplyRule(tile, isOdd);
            tiles.Add(tile);
        }

        StartFadeForWrongTiles();
    }

    void StartFadeForWrongTiles()
    {
        foreach (Tile tile in tiles)
            tile.StartFade(fadeTime);
    }

    void ApplyRule(Tile tile, bool isOdd)
    {
        var image = tile.GetComponent<Image>();
        if (currentRule == RuleType.Color)
        {
            if (isOdd)
            {
                image.color = new Color(Random.value, Random.value, Random.value);
            }
            else
            {
                image.color = normalColor;
            }

            tile.transform.rotation = Quaternion.identity;
        }
        else
        {
            image.color = normalColor;
            float angle = GetRotationAngle();
            tile.transform.rotation = Quaternion.Euler(0, 0, isOdd ? angle : 0f);
        }
    }

    float GetRotationAngle()
    {
        return GetAverageReactionTime() < fastReaction
            ? hardRotation
            : easyRotation;
    }

    public void OnTileClicked(Tile tile)
    {
        if (!gameRunning) return;

        float reactionTime = Time.time - roundStartTime;

        if (tile.isOdd)
        {
            ReactionTime(reactionTime);

            int score = Mathf.Max(
                10,
                Score- Mathf.RoundToInt(reactionTime * timePenalty)
            );

            totalScore += score;
            AdjustDifficulty();
            UpdateUI();
            StartRound();
        }
        else
        {
            wrongClicks++;
            totalScore = Mathf.Max(0, totalScore - wrongClickscore);
            UpdateUI();
        }
    }

    void ReactionTime(float time)
    {
        recentReactionTimes.Enqueue(time);
        if (recentReactionTimes.Count > reactionCount)
            recentReactionTimes.Dequeue();
    }

    float GetAverageReactionTime()
    {
        if (recentReactionTimes.Count == 0) return 1.2f;

        float sum = 0f;
        foreach (float t in recentReactionTimes)
            sum += t;

        return sum / recentReactionTimes.Count;
    }

    void AdjustDifficulty()
    {
        float avg = GetAverageReactionTime();

        if (avg < fastReaction)
            fadeTime = Mathf.Max(1.5f, defaultFadeTime - 1f);
        else if (avg > slowReaction)
            fadeTime = defaultFadeTime + 1f;
        else
            fadeTime = defaultFadeTime;
    }

    void StopGame()
    {
        gameRunning = false;
        ClearGrid();
        resetButton.gameObject.SetActive(true);

        Debug.Log("GAME STOPPED " + totalScore);
    }

    void ResetGame()
    {
        totalScore = 0;
        wrongClicks = 0;
        recentReactionTimes.Clear();
        fadeTime = defaultFadeTime;
        gameRunning = true;

        resetButton.gameObject.SetActive(false);
        UpdateUI();
        StartRound();
    }

    void UpdateUI()
    {
        scoreText.text = "Score: " + totalScore;
        wrongText.text = "Wrong: " + wrongClicks;
    }

    void ClearGrid()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        tiles.Clear();
    }
}
