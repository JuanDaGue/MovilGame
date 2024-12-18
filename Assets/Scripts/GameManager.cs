using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public static GameManager Instance;

    public float timeToMatch = 10f;
    public float currentTimeToMatch=0;

    public int Points =0;
    public UnityEvent OnPointUpdated;

    public UnityEvent<GameState> OnGameStateUpdated;
    public enum GameState{
        Idle,
        InGame,
        GameOver
    }

    public GameState gameState;
    private void Awake(){
        if(Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
        }

    }
    private void Update(){
        if(gameState== GameState.InGame){
            currentTimeToMatch += Time.deltaTime;
            if(currentTimeToMatch> timeToMatch){
                gameState=GameState.GameOver;
                OnGameStateUpdated?.Invoke(gameState);
            }
        }
    }
    public void AddPoints(int newPoints){
        Points+= newPoints;
        OnPointUpdated?.Invoke();
        currentTimeToMatch = 0;
    }
    public void RestatGame(){
        Points =0;
        gameState = GameState.InGame;
        OnGameStateUpdated?.Invoke(gameState);
        currentTimeToMatch=0;
    }
    public void StartGame(){
        Points=0;
        gameState=GameState.InGame;
        OnGameStateUpdated?.Invoke(gameState);
        currentTimeToMatch=0;
    }

    public void ExitGame(){
        Points=0;
        gameState = GameState.Idle;
        OnGameStateUpdated?.Invoke(gameState);
    }
}
