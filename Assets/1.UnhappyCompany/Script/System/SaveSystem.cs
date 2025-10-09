using UnityEngine;
using System.Linq; 

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    public const string SAVE_FILE_NAME = "SaveFile.es3";

    // 새로 만든 ItemLoader 컴포넌트를 에디터에서 Assign
    public ItemLoader itemLoader;
    public QuickSlotSystem quickSlotSystem;
    public RoomGenerator roomGenerator; 


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
        roomGenerator = RoomManager.Instance.roomGenerator;
        
        // 세이브 파일이 있고 방 데이터가 있다면, RoomGenerator가 새로운 방을 생성하지 않도록 미리 플래그 설정
        if (ES3.FileExists(SAVE_FILE_NAME) && ES3.KeyExists("RoomSystemData", SAVE_FILE_NAME))
        {
            if (roomGenerator != null)
            {
                roomGenerator.isRoomDataLoaded = true;
                Debug.Log("[SaveManager] 저장된 방 데이터 발견, RoomGenerator의 자동 생성을 비활성화합니다.");
            }
        }
    }
    
    void Start()
    {
        // 게임 시작 시 자동으로 저장 파일 로드 시도
        if (ES3.FileExists(SAVE_FILE_NAME))
        {
            Debug.Log("[SaveManager] 저장 파일 발견, 자동 로드를 시도합니다.");
            LoadGame();
        }
        else
        {
            Debug.Log("[SaveManager] 저장 파일이 없습니다. 새 게임을 시작합니다.");
        }
    }
    void Update()
    {
        // 디버깅용 수동 저장/로드 단축키
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveGameManual();
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadGameManual();
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            DeleteRoomDataManual();
        }
        if (Input.GetKeyDown(KeyCode.F8))
        {
            DeleteAllSaveDataManual();
        }
        #endif
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

        // 3) 방 시스템 저장
        if(roomGenerator != null)
        {
            RoomSystemSaveData roomData = roomGenerator.SerializeRooms();
            ES3.Save("RoomSystemData", roomData, SAVE_FILE_NAME);
            Debug.Log($"[SaveManager] 방 시스템 저장 완료: {roomData.allRooms.Count}개 방");
        }
        else
        {
            Debug.LogWarning("[SaveManager] RoomGenerator가 설정되지 않아 방 데이터를 저장할 수 없습니다.");
        }

        // 4) 적 시스템 저장 (전역)
        if(roomGenerator != null)
        {
            EnemySystemSaveData enemyData = roomGenerator.SerializeEnemies();
            ES3.Save("EnemySystemData", enemyData, SAVE_FILE_NAME);
            Debug.Log($"[SaveManager] 적 시스템 저장 완료: {enemyData.allEnemies.Count}개 적");
        }
    }

    // 게임 시작 시(또는 플레이어가 저장 파일을 선택한 후) 호출하여 저장 데이터를 불러옴
    public void LoadGame()
    {
        if (ES3.FileExists(SAVE_FILE_NAME))
        {
            // 1) 방 시스템 로드 (다른 시스템보다 먼저 로드)
            if(roomGenerator != null && ES3.KeyExists("RoomSystemData", SAVE_FILE_NAME))
            {
                RoomSystemSaveData roomData = ES3.Load<RoomSystemSaveData>("RoomSystemData", SAVE_FILE_NAME);
                roomGenerator.DeserializeRooms(roomData);
                Debug.Log($"[SaveManager] 방 시스템 로드 완료: {roomData.allRooms.Count}개 방");
            }
            else if(roomGenerator == null)
            {
                Debug.LogWarning("[SaveManager] RoomGenerator가 설정되지 않아 방 데이터를 로드할 수 없습니다.");
            }
            else
            {
                Debug.Log("[SaveManager] 저장된 방 데이터가 없습니다. 새로 생성됩니다.");
            }

            // 2) 적 시스템 로드 (전역)
            if(roomGenerator != null && ES3.KeyExists("EnemySystemData", SAVE_FILE_NAME))
            {
                EnemySystemSaveData enemyData = ES3.Load<EnemySystemSaveData>("EnemySystemData", SAVE_FILE_NAME);
                roomGenerator.DeserializeEnemies(enemyData);
                Debug.Log($"[SaveManager] 적 시스템 로드 완료: {enemyData.allEnemies.Count}개 적");
            }
            else if(ES3.KeyExists("EnemySystemData", SAVE_FILE_NAME) == false)
            {
                Debug.Log("[SaveManager] 저장된 적 데이터가 없습니다.");
            }

            // 3) 월드 아이템 로드
            if(itemLoader != null)
            {
                itemLoader.LoadItemsFromSaveFile(SAVE_FILE_NAME);
            }

            // 4) 퀵슬롯 시스템 로드
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

    /// <summary>
    /// 디버깅용 수동 저장 (단축키나 디버그 메뉴에서 호출)
    /// </summary>
    public void SaveGameManual()
    {
        Debug.Log("[SaveManager] 수동 저장 시작...");
        SaveGame();
        Debug.Log("[SaveManager] 수동 저장 완료!");
    }

    /// <summary>
    /// 디버깅용 수동 로드 (단축키나 디버그 메뉴에서 호출)
    /// </summary>
    public void LoadGameManual()
    {
        Debug.Log("[SaveManager] 수동 로드 시작...");
        LoadGame();
        Debug.Log("[SaveManager] 수동 로드 완료!");
    }

    /// <summary>
    /// 방 세이브 데이터만 삭제 (다른 데이터는 유지)
    /// </summary>
    public void DeleteRoomData()
    {
        if (ES3.FileExists(SAVE_FILE_NAME))
        {
            bool deletedAny = false;
            
            if (ES3.KeyExists("RoomSystemData", SAVE_FILE_NAME))
            {
                ES3.DeleteKey("RoomSystemData", SAVE_FILE_NAME);
                deletedAny = true;
                Debug.Log("[SaveManager] 방 세이브 데이터가 삭제되었습니다.");
            }
            
            if (ES3.KeyExists("EnemySystemData", SAVE_FILE_NAME))
            {
                ES3.DeleteKey("EnemySystemData", SAVE_FILE_NAME);
                deletedAny = true;
                Debug.Log("[SaveManager] 적 세이브 데이터가 삭제되었습니다.");
            }
            
            // RoomGenerator의 로드 플래그 리셋
            if(roomGenerator != null)
            {
                roomGenerator.isRoomDataLoaded = false;
            }
            
            if (!deletedAny)
            {
                Debug.LogWarning("[SaveManager] 삭제할 방/적 세이브 데이터가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning($"[SaveManager] 저장 파일 '{SAVE_FILE_NAME}'이 존재하지 않습니다.");
        }
    }

    /// <summary>
    /// 디버깅용 방 세이브 데이터 수동 삭제 (F6 키)
    /// </summary>
    public void DeleteRoomDataManual()
    {
        Debug.Log("[SaveManager] 방 세이브 데이터 삭제 시작...");
        DeleteRoomData();
        Debug.Log("[SaveManager] 방 세이브 데이터 삭제 완료! 씬을 다시 로드하면 새로운 방이 생성됩니다.");
    }

    /// <summary>
    /// 전체 세이브 파일 삭제 (모든 데이터 초기화)
    /// </summary>
    public void DeleteAllSaveData()
    {
        if (ES3.FileExists(SAVE_FILE_NAME))
        {
            ES3.DeleteFile(SAVE_FILE_NAME);
            
            // RoomGenerator의 로드 플래그 리셋
            if(roomGenerator != null)
            {
                roomGenerator.isRoomDataLoaded = false;
            }
            
            Debug.Log($"[SaveManager] 전체 저장 파일 '{SAVE_FILE_NAME}'이 삭제되었습니다.");
        }
        else
        {
            Debug.LogWarning($"[SaveManager] 삭제할 저장 파일 '{SAVE_FILE_NAME}'이 존재하지 않습니다.");
        }
    }

    /// <summary>
    /// 디버깅용 전체 세이브 파일 수동 삭제 (F8 키)
    /// </summary>
    public void DeleteAllSaveDataManual()
    {
        Debug.Log("[SaveManager] 전체 세이브 파일 삭제 시작...");
        Debug.LogWarning("[SaveManager] 경고: 모든 저장 데이터(방, 아이템, 퀵슬롯 등)가 삭제됩니다!");
        DeleteAllSaveData();
        Debug.Log("[SaveManager] 전체 세이브 파일 삭제 완료! 씬을 다시 로드하면 처음부터 시작됩니다.");
    }
}