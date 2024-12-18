using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class UIGameOver : MonoBehaviour
{

    public int displayedPoints =0;
    public TextMeshProUGUI pointsUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.OnGameStateUpdated.AddListener(GameStateUpdated);
    }

    // Update is called once per frame
    private void OnDestroy()
    {
        GameManager.Instance.OnGameStateUpdated.RemoveListener(GameStateUpdated);
    }
    private void GameStateUpdated(GameManager.GameState  newState)
    {
        if(newState ==GameManager.GameState.GameOver){
            displayedPoints =0;
            StartCoroutine(DisplayPointsCoroutine());
        }
    }

    IEnumerator DisplayPointsCoroutine()
    {
        while(displayedPoints< GameManager.Instance.Points){
            displayedPoints++;
            pointsUI.text = displayedPoints.ToString();
            yield return new WaitForFixedUpdate();
        }
        displayedPoints = GameManager.Instance.Points;
        pointsUI.text = displayedPoints.ToString();
        yield return null;
    }

    public void PlayAgain(){
        GameManager.Instance.RestatGame();
    }
    public void ExitGame(){
        GameManager.Instance.ExitGame();
    }
}
