using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance = null;
    public PlayerStatus playerStatus;
    public Player currentPlayer;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    private void Start()
    {
        StartCoroutine(CheckGameOver());
    }
    private IEnumerator CheckGameOver()
    {
        while (true)
        {
            if(CentralBatterySystem.Instance.totalBatteryLevel <= 0 )
            {
                UIManager.Instance.gameOverImage.SetActive(true);
            }

            if(playerStatus.CurrentHealth <= 0)
            {
                UIManager.Instance.gameOverImage.SetActive(true);
            }
            yield return null;
        }
    }
}
