using System.Collections;
using UnityEngine;

public class RSPSystem : MonoBehaviour
{
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public RSPResult RSPCalculate(RSPChoice playerChoice, RSPChoice enemyChoice)
    {
        if (playerChoice == enemyChoice)
        {
            return RSPResult.Draw;
        }

        if ((playerChoice == RSPChoice.Rock && enemyChoice == RSPChoice.Scissors) ||
            (playerChoice == RSPChoice.Scissors && enemyChoice == RSPChoice.Paper) ||
            (playerChoice == RSPChoice.Paper && enemyChoice == RSPChoice.Rock))
        {
            return RSPResult.Win;
        }
        return RSPResult.Lose;
    }

    public RSPChoice GetRandomRSPChoice()
    {
        int randomValue = Random.Range(0, 3);
        switch (randomValue)
        {
            case 0:
                return RSPChoice.Rock;
            case 1:
                return RSPChoice.Scissors;
            case 2:
                return RSPChoice.Paper;
            default:
                return RSPChoice.Rock; // 기본값으로 Rock 반환
        }
    }

    public IEnumerator PlayRSP()
    {
        Debug.Log("RSP: 가위바위보 게임 시작");
        RSPResult result = RSPResult.Draw;
        while(true)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                Debug.Log("사용자가 바위를 선택했습니다.");
                result = RSPCalculate(RSPChoice.Rock, GetRandomRSPChoice());
                break;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("사용자가 가위를 선택했습니다.");
                result = RSPCalculate(RSPChoice.Scissors, GetRandomRSPChoice());
                break;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("사용자가 보를 선택했습니다.");
                result = RSPCalculate(RSPChoice.Paper, GetRandomRSPChoice());
                break;
            }
            yield return null;
        }

        
        yield return result;
    }
}
