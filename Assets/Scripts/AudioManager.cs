using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static AudioManager Instance;
    private void Awake(){
        if(Instance== null){
            Instance= this;
        }
        else{
            Destroy(gameObject);
        }
    }

    public AudioClip moveSFx;
    public AudioClip missSFx;
    public AudioClip matchSFx;
    public AudioClip gameOverSFx;

    public AudioSource SfxSource;
    void Start()
    {
        GameManager.Instance.OnPointUpdated.AddListener(PointsUpdated);
        GameManager.Instance.OnGameStateUpdated.AddListener(GameStateUpdated);
    }

    private void GameStateUpdated(GameManager.GameState newState)
    {
        if(newState== GameManager.GameState.GameOver){
            SfxSource.PlayOneShot(gameOverSFx);
        }
                if(newState== GameManager.GameState.InGame){
            SfxSource.PlayOneShot(matchSFx);
        }
    }

    private void OnDestroy(){
                GameManager.Instance.OnPointUpdated.RemoveListener(PointsUpdated);
        GameManager.Instance.OnGameStateUpdated.RemoveListener(GameStateUpdated);
    }
    private void PointsUpdated()
    {     
            SfxSource.PlayOneShot(matchSFx);
    }

    public void Move(){
            SfxSource.PlayOneShot(moveSFx);
    }
    public void Miss(){
            SfxSource.PlayOneShot(missSFx);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
