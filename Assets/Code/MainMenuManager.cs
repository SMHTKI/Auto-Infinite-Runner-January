using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject AreYouSureYouWantToQuit;
    [SerializeField] private GameObject LeaderBoardCanvas;
    [SerializeField] private GameObject SettingsPanel;
    [SerializeField] private GameObject GameOptionsPanel;
    [SerializeField] private GameObject ApplicationsOptionsPanel;
    [SerializeField] private GameObject InstructionssPanel;
    [SerializeField] private Slider VolumeSlider;

    // Start is called before the first frame update
    void Awake()
    {
        if (MusicManager.Instance)
        {
            MusicManager.Instance.MasterVolumeSlider = VolumeSlider;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(LeaderBoardCanvas.activeSelf)
            {
                GameOptionsPanel.SetActive(true);
                ApplicationsOptionsPanel.SetActive(true);
                LeaderBoardCanvas.SetActive(false);
            }
            else if (InstructionssPanel.activeSelf)
            {
                ApplicationsOptionsPanel.SetActive(true);
                GameOptionsPanel.SetActive(true);
                InstructionssPanel.SetActive(false);
            }
            else if (SettingsPanel.activeSelf)
            {
                ApplicationsOptionsPanel.SetActive(true);
                GameOptionsPanel.SetActive(true);
                SettingsPanel.SetActive(false);
            }
            else
            {
                AreYouSureYouWantToQuit.SetActive(!AreYouSureYouWantToQuit.activeSelf);
            }
        }
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(1);
    }

    public void ExitToDesktop()
    {
        Application.Quit();
    }

    public void ToggleSettingsMenu()
    {
        ApplicationsOptionsPanel.SetActive(SettingsPanel.activeSelf);
        GameOptionsPanel.SetActive(SettingsPanel.activeSelf);
        SettingsPanel.SetActive(!SettingsPanel.activeSelf);
    }

    public void MinimizeApp()
    {
        Screen.fullScreen = !Screen.fullScreen; 
    }
}
