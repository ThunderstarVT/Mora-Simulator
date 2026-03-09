using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreTracker : MonoBehaviour
{
    public static ScoreTracker Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
    
    
    [Header("Combo Settings")]
    [SerializeField, Min(0f)] private float comboDuration = 5.0f;
    
    [Header("UI Objects")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI scoreToAddText;
    [Space]
    [SerializeField] private Canvas comboCanvas;
    [SerializeField] private Slider comboTimerSlider;
    [SerializeField] private TextMeshProUGUI comboPointText;
    [SerializeField] private TextMeshProUGUI comboMultiplierText;
    [SerializeField] private TextMeshProUGUI comboDisplayText;
    
    [Header("Debug Stuff")]
    [SerializeField] private long score;
    
    private long scoreToAdd;
    private long addRate = 1;
    
    private bool comboRunning;
    private float comboTimer;

    private long comboPoints;
    
    private Dictionary<string, int> typeCounts = new();
    private HashSet<string> uniqueTypes = new();
    
    
    public event Action<long, long, long> OnScoreChanged;
    public event Action<string, long, int, long> OnPointsAwarded;


    private void Start()
    {
        OnScoreChanged += (_score, _toAdd, _added) =>
        {
            scoreText.text = _score == 0 ? "" : _score.ToString();
            scoreToAddText.text = _toAdd > 0 ? $"+{_toAdd}" : "";
        };

        OnPointsAwarded += (_displayText, _points, _multiplier, _added) =>
        {
            comboPointText.text = _points.ToString();
            comboMultiplierText.text = "×" + _multiplier;
            comboDisplayText.text = _displayText;
            
            //TODO: summon floating number that shows _added with initial size based on the value
        };
        
        
        EndCombo();
        
        OnScoreChanged?.Invoke(score, scoreToAdd, 0);
    }

    private void FixedUpdate()
    {
        UpdateScoreAdding();
    }

    private void Update()
    {
        if (!comboRunning) return;

        comboTimer -= Time.deltaTime;
        
        comboTimerSlider.value = Mathf.Clamp01(comboTimer/comboDuration);

        if (comboTimer <= 0)
        {
            EndCombo();
        }
    }


    private void UpdateScoreAdding()
    {
        if (scoreToAdd <= 0)
        {
            addRate = 1;
            return;
        }

        long amount = addRate > scoreToAdd ? scoreToAdd : addRate;
        
        score += amount;
        scoreToAdd -= amount;

        addRate++;
        
        OnScoreChanged?.Invoke(score, scoreToAdd, amount);
    }


    private void StartCombo()
    {
        comboCanvas.enabled = true;
        
        comboRunning = true;
        comboTimer = comboDuration;
        
        comboPoints = 0;
        typeCounts.Clear();
        uniqueTypes.Clear();
    }
    
    private void EndCombo()
    {
        comboCanvas.enabled = false;
        
        comboRunning = false;
        
        int comboMultiplier = uniqueTypes.Count;
        
        long finalScore = comboPoints * comboMultiplier;
        
        scoreToAdd += finalScore;
        
        comboPoints = 0;
        typeCounts.Clear();
        uniqueTypes.Clear();
    }


    public void AwardPoints(long points, string typeId, string displayText)
    {
        if (!comboRunning) StartCombo();
        
        int previousMultiplier = uniqueTypes.Count;

        typeCounts.TryAdd(typeId, 0);
        
        int timesOccurred = typeCounts[typeId];
        typeCounts[typeId]++;
        
        uniqueTypes.Add(typeId);
        
        int newMultiplier = uniqueTypes.Count;
        
        long pointsToAdd = CeilShift(points, timesOccurred);
        
        comboPoints += pointsToAdd;
        
        OnPointsAwarded?.Invoke(displayText, comboPoints, newMultiplier, pointsToAdd);
        
        if (newMultiplier > previousMultiplier) comboTimer = comboDuration;
    }


    private static long CeilShift(long num, int shift)
    {
        while (true)
        {
            if (num == 1 || num == 0 || shift == 0) return num;

            if (num % 2 == 1)
            {
                num = (num >> 1) + 1;
                shift--;
                continue;
            }

            num >>= 1;
            shift--;
        }
    }
}
