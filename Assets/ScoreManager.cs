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

    public int CurrentScore { get { return _currentScore; } }
    private int _currentScore;

    /// <summary>
    /// Adds to the overall Score of the current run
    /// </summary>
    /// <param name="additionalScore">The score to add to the current score of the run</param>
    /// <param name="isObstical">Modifier for obstical added score as apposed to turns.</param>
    public void AddScore(int additionalScore, bool isObstical = false)
    {
        GameEventsManager.Instance.ScoreAdded(CurrentScore, additionalScore, isObstical);
        _currentScore += additionalScore;
    }
}
