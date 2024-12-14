using UnityEngine;
using FMODUnity;


public class FMODEvents : MonoBehaviour
{
    public static FMODEvents instance;
    [field: Header("Damage")]
    [field: SerializeField] public EventReference missDamage { get; private set; }

    [field: SerializeField] public EventReference damage { get; private set; }
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
