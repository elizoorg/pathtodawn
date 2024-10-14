using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCanvasScript : MonoBehaviour
{
    [SerializeField]
    GameObject StartButton;
    [SerializeField]
    GameObject FindMatchButton;
    [SerializeField]
    GameObject ConnectingText;
    [SerializeField]
    GameObject CancelSearchButton;
    [SerializeField]
    Launcher Launcher;
    // Start is called before the first frame update
    void Start()
    {
        Launcher.OnSearchStart += Launcher_OnSearchStart;
        Launcher.OnSearchStop += Launcher_OnSearchStop;
    }

    private void Launcher_OnSearchStart()
    {
        StartButton.SetActive(false);
        FindMatchButton.SetActive(false);
        ConnectingText.SetActive(true);
        CancelSearchButton.SetActive(true);
    }

    private void Launcher_OnSearchStop()
    {
        StartButton.SetActive(true);
        FindMatchButton.SetActive(true);
        ConnectingText.SetActive(false);
        CancelSearchButton.SetActive(false);
    }
}
