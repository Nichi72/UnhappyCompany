using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CheatSystem : MonoBehaviour
{
    public static CheatSystem instance;
    public GameObject cheatPanel;

    [SerializeField] private Button btnInitEgg;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }   
    void Start()
    {
        btnInitEgg.onClick.AddListener(InitEgg);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            EnemyManager.instance.SpawnEnemy();
        }
        if(Input.GetKeyDown(KeyCode.F2))
        {
            RegionManager.instance.InitDefaultSchedule();
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            TimeManager.instance.FastForwardToNearestSchedule();
        }

       
    }

    private void InitEgg()
    {
        Debug.Log("InitEgg");
        EnemyManager.instance.SpawnEgg();
    }

    private void InitEnemy()
    {
        Debug.Log("InitEnemy");
        EnemyManager.instance.SpawnEnemy();
    }
    // 아이템 세이브 & 로드 관련
    private void SaveAndLoad()
    {
        // if (Input.GetKeyDown(KeyCode.F1))
        // {
        //     SaveManager.Instance.SaveGame();
        // }
        // if (Input.GetKeyDown(KeyCode.F2))
        // {
        //     SaveManager.Instance.LoadGame();
        // }
        // if (Input.GetKeyDown(KeyCode.F3))
        // {
        //     ItemLoader.Instance.ClearSceneItems();
        // }
    }


}
