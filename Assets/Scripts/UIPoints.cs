using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class UIPoints : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    int displayedPoints =0;
    public  TextMeshProUGUI pointsLabel;

    void Start()
    {
        GameManager.Instance.OnPointUpdated.AddListener(UpdatePoints);
        GameManager.Instance.OnGameStateUpdated.AddListener(GameStateUpdated);
    }
    private void OnDestroy(){
        GameManager.Instance.OnGameStateUpdated.RemoveListener(GameStateUpdated);
        GameManager.Instance.OnPointUpdated.RemoveListener(UpdatePoints);

    }
    private void GameStateUpdated(GameManager.GameState newState)
    {
            if(newState== GameManager.GameState.GameOver){
                displayedPoints=0;
                pointsLabel.text= displayedPoints.ToString();
            }
    }

    void UpdatePoints()
    {
        
    }

    IEnumerator  UpdatePointsCoroutine()
    {
        while(displayedPoints<GameManager.Instance.Points){
                displayedPoints++;
                pointsLabel.text = displayedPoints.ToString();
                yield return new WaitForSeconds(0.1f);
        }
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
