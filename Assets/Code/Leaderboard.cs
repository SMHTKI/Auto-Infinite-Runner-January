using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Dan.Main;
using Unity.VisualScripting;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> names;
    [SerializeField] private List<TextMeshProUGUI> scores;
    [SerializeField] private string[] badWords;

    private string publicLeaderboardKey = "413d5cff602097bc5c704cf53012bdc84cda1c3be771f01d70b916ae7f9dd319";

    void Start()
    {
        GetLeaderBoard();
    }

    public void GetLeaderBoard()
    {
        LeaderboardCreator.GetLeaderboard(publicLeaderboardKey,((msg) =>
        {
            int loopLenght = (msg.Length < names.Count) ? msg.Length : names.Count;
            for (int i = 0; i < loopLenght; i++)
            {
                names[i].text = msg[i].Username;
                scores[i].text = msg[i].Score.ToString();
            }
        }) );
    }

    public void SetLeaderBoardEntry(string username, int score)
    {
        int substringLenght = (username.Length > 4) ? 4 : username.Length;
        username = username.Substring(0, substringLenght);
        if (System.Array.IndexOf(badWords, username) != -1) return;
        LeaderboardCreator.UploadNewEntry(publicLeaderboardKey, username, score, ((msg) =>
        {
            GetLeaderBoard();
        }));
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
