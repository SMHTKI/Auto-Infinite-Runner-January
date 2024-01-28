using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{

    #region SINGLETON  
    private static ScoreManager _instance = null;
    public static ScoreManager Instance { get { return _instance; } }
    private ScoreManager() { }
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }
    #endregion

    private int _currentScore;
    public int CurrentScore { get { return _currentScore; } }
    public UnityEvent<int, int, bool> OnScoreAdded;

    public void AddScore(int additionalScore, bool isObstical = false)
    {
         OnScoreAdded?.Invoke(_currentScore, additionalScore, isObstical);
        _currentScore += additionalScore;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
