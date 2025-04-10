using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance { get; private set; }

    [Header("Debug Settings")]
    public bool enableRampageDebug = true;

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

    public static void Log(string message, bool isRampageDebug = false)
    {
        if (Instance == null) return;
        
        if (isRampageDebug && !Instance.enableRampageDebug) return;
        
        Debug.Log(message);
    }
} 