using UnityEngine;
using FMODUnity;
using System.Collections.Generic;


public class FMODEvents : MonoBehaviour
{
    public static FMODEvents instance;
    [field: Header("Damage")]
    [field: SerializeField] public EventReference missDamage { get; private set; }
    [Header("TEST")]
    [field: SerializeField] public EventReference TEST { get; private set; }

    [field: SerializeField] public EventReference damage { get; private set; }
    [field: SerializeField] public EventReference rspWin { get; private set; }
    [field: SerializeField] public EventReference rspLose { get; private set; }
    [field: SerializeField] public List<EventReference> rspStack { get; private set; }
    [field: SerializeField] public EventReference TestBeep { get; private set; }



    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }   
}
