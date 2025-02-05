using UnityEngine;

public class AutoStartActive : MonoBehaviour
{
    public bool isActive = false;
    void Awake()
    {
        gameObject.SetActive(isActive);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
