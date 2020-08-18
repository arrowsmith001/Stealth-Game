using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable] public class AlertEvent : UnityEvent { }

public class EventsScript : MonoBehaviour
{
    public static EventsScript instance;
    public AlertEvent alertEvent = new AlertEvent();

    private void Awake()
    {
        if(instance != null) if (instance != this) Destroy(instance.gameObject);
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


