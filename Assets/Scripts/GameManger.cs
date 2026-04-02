using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("Timer")]
    [SerializeField] private float totalTime = 120f;
    private float currentTime;

    [Header("Collection")]
    [SerializeField] private int totalCubes;
    private int collectedCubes;

    [Header("Cube Spawn")]
    [SerializeField] private CollectibleCube cubePrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int totalCubesToCollect = 5;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text cubeText;
    [SerializeField] private GameObject startMenuPanel;
    [SerializeField] private GameObject gameUIPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text resultText;

    private bool gameEnded;
    private bool gameStarted = false;
    private CollectibleCube currentCube;
    private List<int> usedSpawnIndices = new List<int>();

    public Transform CurrentCubeTarget => currentCube != null ? currentCube.transform : null;

    private void Start()
    {
        Time.timeScale = 0f; // ⛔ pause game at start

        if (startMenuPanel) startMenuPanel.SetActive(true);
        if (gameUIPanel) gameUIPanel.SetActive(false);

        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (winPanel) winPanel.SetActive(false);
    }

    private void Update()
    {
        if (!gameStarted || gameEnded) return;

        currentTime -= Time.deltaTime;
        currentTime = Mathf.Max(currentTime, 0f);

        UpdateTimerUI();

        if (currentTime <= 0f)
        {
            GameOver("Time Up!");
        }
    }

    public void StartGame()
    {
        Time.timeScale = 1f;

        if (startMenuPanel) startMenuPanel.SetActive(false);
        if (gameUIPanel) gameUIPanel.SetActive(true);

        currentTime = totalTime;
        collectedCubes = 0;
        gameEnded = false;
        gameStarted = true;

        UpdateTimerUI();
        UpdateCubeUI();

        SpawnNextCube();
    }

    public void OnCubeCollected(CollectibleCube cube)
    {
        if (gameEnded) return;
        if (cube != currentCube) return;

        collectedCubes++;
        UpdateCubeUI();

        currentCube = null;

        if (collectedCubes >= totalCubesToCollect)
        {
            WinGame();
            return;
        }

        SpawnNextCube();
    }

    private void SpawnNextCube()
    {
        if (spawnPoints == null || spawnPoints.Length == 0 || cubePrefab == null)
            return;

        if (usedSpawnIndices.Count >= spawnPoints.Length)
            usedSpawnIndices.Clear();

        int randomIndex = GetRandomUnusedSpawnIndex();
        Transform spawnPoint = spawnPoints[randomIndex];

        currentCube = Instantiate(cubePrefab, spawnPoint.position, spawnPoint.rotation);
        usedSpawnIndices.Add(randomIndex);
    }

    private int GetRandomUnusedSpawnIndex()
    {
        List<int> available = new List<int>();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!usedSpawnIndices.Contains(i))
                available.Add(i);
        }

        return available[Random.Range(0, available.Count)];
    }

    public void GameOver(string reason)
    {
        if (gameEnded) return;

        gameEnded = true;
        Time.timeScale = 0f;

        if (gameUIPanel) gameUIPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (resultText) resultText.text = reason;
    }

    public void WinGame()
    {
        if (gameEnded) return;

        gameEnded = true;
        Time.timeScale = 0f;

        if (gameUIPanel) gameUIPanel.SetActive(false);
        if (winPanel) winPanel.SetActive(true);
    }

    public bool IsGameEnded()
    {
        return gameEnded;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        gameStarted = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UpdateTimerUI()
    {
        if (!timerText) return;

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        timerText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    private void UpdateCubeUI()
    {
        if (!cubeText) return;
        cubeText.text = $"Cubes: {collectedCubes}/{totalCubesToCollect}";
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
