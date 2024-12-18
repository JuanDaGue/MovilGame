using UnityEngine;

public class UIStartScreen : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void StartBtnClicked(){
        GameManager.Instance.StartGame();
    }
}
