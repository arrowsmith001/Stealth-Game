using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviour
{
    public PlayerController player;

    public Material wall;
    public Material wallSemitrans;

    public static Global instance;
    
    private void Awake()
    {
        if (instance != null) if (instance != this) Destroy(instance.gameObject);
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
