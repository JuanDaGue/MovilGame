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
    }

    void UpdatePoints()
    {
        StartCoroutine(UpdatePointsCoroutine());
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
