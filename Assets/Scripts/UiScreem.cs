using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class UiScreem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public RectTransform containerRect;
    public CanvasGroup containerCanvas;
    public Image background;

    public GameManager.GameState visibleState;
    public float transitionTime;
    void Start()
    {
        GameManager.Instance.OnGameStateUpdated.AddListener(GameStateUpdated);
        bool initialState = GameManager.Instance.gameState == visibleState;
        background.enabled = initialState;
        containerRect.gameObject.SetActive(initialState);
    }

    private void GameStateUpdated(GameManager.GameState newState)
    {
       if(newState== visibleState){
            ShowScreen();
       }
       else{
            HideScreen();
       }
    }

    private void HideScreen()
    {
        //background animation
        var bgColor = background.color;
        bgColor.a=0;
        background.DOColor(bgColor, transitionTime*0.5f);

        //Container
        containerCanvas.alpha=1;
        containerRect.anchoredPosition=  Vector2.zero;
        containerCanvas.DOFade(0f, transitionTime*0.5f);
        containerRect.DOAnchorPos(new Vector2(0,-100), transitionTime*0.5f).onComplete=()=>{
            background.enabled=false;
            containerRect.gameObject.SetActive(false);
        };
    }

    private void ShowScreen()
    {
        //Enable elelemts
        background.enabled = true;
        containerRect.gameObject.SetActive(true);
        //background animation
        var bgColor = background.color;
        bgColor.a=0;
        background.color = bgColor;
        bgColor.a=1;
        background.DOColor(bgColor, transitionTime);
        containerCanvas.alpha=0;
        containerRect.anchoredPosition= new Vector2(0,100);
        containerCanvas.DOFade(1f, transitionTime);
        containerRect.DOAnchorPos(Vector2.zero, transitionTime);
    }

    // Update is called once per frame
}
