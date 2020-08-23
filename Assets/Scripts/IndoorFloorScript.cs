using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndoorFloorScript : MonoBehaviour
{
    public GameObject indoorArea;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void HideCeilingAndWalls()
    {
        //print(indoorArea.name);
        ToggleIndoors(false);
    }

    void ToggleIndoors(bool active)
    {
        int childCount = indoorArea.transform.childCount;
        String output = "";

        for (int i = 0; i < childCount; i++)
        {
            GameObject go = indoorArea.transform.GetChild(i).gameObject;

            switch (go.tag)
            {
                case "IndoorCeiling":
                    {
                        //go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                        go.SetActive(active);
                    }
                    break;
                case "IndoorWall":
                    {

                        int wallCount = go.transform.childCount;
                        for(int j = 0; j < wallCount; j++)
                        {
                            go.transform.GetChild(j).GetComponent<MeshRenderer>().material
                                = active ? Global.instance.wall : Global.instance.wallSemitrans;
                        }
                    }
                    break;
            }
        }
    }
     
    // Update is called once per frame
    void FixedUpdate()
    {
       ToggleIndoors(true);
    }


}
