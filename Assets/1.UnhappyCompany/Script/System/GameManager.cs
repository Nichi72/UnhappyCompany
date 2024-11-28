using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public PlayerStatus playerStatus;
    public Player currentPlayer;
    public int totalGold = 10;

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

    private void GameStart()
    {

    }

    private void Update()
    {
        UIManager.instance.UpdateGold(totalGold);
    }
    private IEnumerator CheckGameOver()
    {
        while (true)
        {
            if(CentralBatterySystem.Instance.totalBatteryLevel <= 0 )
            {
                UIManager.instance.gameOverImage.SetActive(true);
            }

            if(playerStatus.CurrentHealth <= 0)
            {
                UIManager.instance.gameOverImage.SetActive(true);
            }
            yield return null;
        }
    }

    public bool BuyItemWithGold(ItemData itemData)
    {
        var temp = totalGold - itemData.BuyPrice;
        if(temp <= 0)
        {
            Debug.Log("���źҰ�");
            return false;
        }
        totalGold -= itemData.BuyPrice;
        return true;
    }
}
