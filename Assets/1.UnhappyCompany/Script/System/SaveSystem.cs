using UnityEngine;
using System.Linq; 

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    public const string SAVE_FILE_NAME = "SaveFile.es3";

    // 새로 만든 ItemLoader 컴포넌트를 에디터에서 Assign
    public ItemLoader itemLoader;
    public QuickSlotSystem quickSlotSystem; 


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
    void Start()
    {
        // quickSlotSystem = QuickSlotSystem.in
    }
    void Update()
    {
       
    }

    // 게임 사이클 종료 시 호출하여 자동 저장 처리
    public void SaveGame()
    {
        // 1) 월드 아이템 저장
        if(itemLoader != null)
        {
            itemLoader.SaveItemsToSaveFile(SAVE_FILE_NAME);
        }
        Debug.Log("게임이 파일 '" + SAVE_FILE_NAME + "'에 저장되었습니다.");

        // 2) 퀵슬롯 시스템 상태 저장
        if(quickSlotSystem != null)
        {
            // SerializeSystem()을 통해 QuickSlotSystemState 생성
            QuickSlotSystem.QuickSlotSystemState slotSystemState = quickSlotSystem.SerializeSystem();
            // Easy Save로 파일에 기록
            ES3.Save("QuickSlotSystemState", slotSystemState, SAVE_FILE_NAME);
            Debug.Log("퀵슬롯 시스템 상태 저장 완료");
        }
    }

    // 게임 시작 시(또는 플레이어가 저장 파일을 선택한 후) 호출하여 저장 데이터를 불러옴
    public void LoadGame()
    {
        if (ES3.FileExists(SAVE_FILE_NAME))
        {
            // 1) 월드 아이템 로드
            if(itemLoader != null)
            {
                itemLoader.LoadItemsFromSaveFile(SAVE_FILE_NAME);
            }

            // 2) 퀵슬롯 시스템 로드
            if(quickSlotSystem != null)
            {
                if(ES3.KeyExists("QuickSlotSystemState", SAVE_FILE_NAME))
                {
                    QuickSlotSystem.QuickSlotSystemState slotSystemState = ES3.Load<QuickSlotSystem.QuickSlotSystemState>("QuickSlotSystemState", SAVE_FILE_NAME);
                    quickSlotSystem.DeserializeSystem(slotSystemState);
                    Debug.Log("퀵슬롯 시스템 상태 로드 완료");
                }
                else
                {
                    Debug.Log("퀵슬롯 시스템 상태가 저장된 적이 없습니다.");
                }
            }
            Debug.Log($"파일 '{SAVE_FILE_NAME}'에서 게임 데이터를 불러왔습니다.");
        }
        else
        {
            Debug.LogWarning($"저장 파일 '{SAVE_FILE_NAME}'을 찾을 수 없습니다.");
        }
    }
}