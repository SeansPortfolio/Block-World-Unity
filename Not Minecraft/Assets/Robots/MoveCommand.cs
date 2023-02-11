using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCommand
{

    public void Execute(Robot robot)
    {
        robot.Position.x += 1;
    }



}
