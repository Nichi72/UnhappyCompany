using UnityEngine;
using System.Linq; 

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    public string saveFileName = "SaveFile.es3";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SaveGame();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            LoadGame();
        }
    }
    // 게임 사이클 종료 시 호출하여 자동 저장 처리
    public void SaveGame()
    {
        // 씬 내의 모든 MonoBehaviour 중 ISavable을 구현한 객체를 가져와 SaveState() 호출
        var savables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISavable>().ToArray();
        foreach (var savable in savables)
        {
            savable.SaveState();
        }
        Debug.Log("게임이 파일 '" + saveFileName + "'에 저장되었습니다.");
    }

    // 게임 시작 시(또는 플레이어가 저장 파일을 선택한 후) 호출하여 저장 데이터를 불러옴
    public void LoadGame()
    {
        if (ES3.FileExists(saveFileName))
        {
            var savables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISavable>().ToArray();
            foreach (var savable in savables)
            {
                savable.LoadState();
            }
            Debug.Log("파일 '" + saveFileName + "'에서 게임 데이터를 불러왔습니다.");
        }
        else
        {
            Debug.LogWarning("저장 파일 '" + saveFileName + "'을 찾을 수 없습니다.");
        }
    }
}