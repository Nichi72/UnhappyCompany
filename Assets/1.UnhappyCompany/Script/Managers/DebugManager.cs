using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance { get; private set; }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("DebugManager 이미 있음");
            Destroy(gameObject);
        }
    }

    public static void Log(string message, bool isRampageDebug = false)
    {
        if (Instance == null) return;
        
        if (isRampageDebug == false) return;
        
        Debug.Log(message);
    }
} 