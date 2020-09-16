using System;
using System.Collections.Generic;
using UnityEngine;

public class NavData
{
    private int index = -1;

    List<NavCommand> commands = new List<NavCommand>();

    public NavData(){}

    public NavData(NavCommand navCommand)
    {
        commands.Add(navCommand);
    }

    public void AddCommand(NavCommand cmd){
        commands.Add(cmd);
    }

    public void First(){
        index = 0;
    }

    public bool Next()
    {
        index++;
        return index < commands.Count;
    }

    public NavCommand GetCommand(){
        return commands[index];
    }

    public int Count(){
        return commands.Count;
    }

    public int GetIndex(){
        return index;
    }



}

public enum Commands{
    GO, ROTATE, LOOK, WAIT, REACT
}

public class NavCommand
{
    public Commands command;
    public Vector3 args;




    public NavCommand(Commands command, Vector3 args)
    {
        this.command = command;
        this.args = args;


    }
}