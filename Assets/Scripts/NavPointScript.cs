using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavPointScript : MonoBehaviour
{
    // List of times (in seconds) to go through
    public Commands[] commands;

    // List of angles (relative to the sentry facing) to go through
    public Vector3[] args;

    public NavData GetNavData()
    {
        NavData nd = new NavData();
        
        if(commands.Length != args.Length) throw new Exception("NavData args unequal lengths");

        for(int i = 0; i < commands.Length; i++)
        {
            nd.AddCommand(new NavCommand(commands[i], args[i]));
        }

        return nd;
    }




}
