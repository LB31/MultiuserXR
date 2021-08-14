using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public Button ContinueButton;
    public Button ExitButton;

    private void Awake()
    {
        GameStateHandler gsh = FindObjectOfType<GameStateHandler>();
        ContinueButton.onClick.AddListener(() => gsh.Menu());
        ExitButton.onClick.AddListener(() => ExitGame());
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    private void ExitGame()
    {
        Application.Quit();
    }

}
