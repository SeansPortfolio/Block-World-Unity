using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    public Robot CurrentRobot;

    public MoveCommand Move;


    // Start is called before the first frame update
    void Start()
    {
        CurrentRobot = new Robot();


        Move = new MoveCommand();

        StartCoroutine(Loop());


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Loop()
    {
        while(true)
        {
            yield return new WaitForSeconds(1);

            Move.Execute(CurrentRobot);

            transform.position = CurrentRobot.Position.ToVector3();
        }
    }
}

public static class Extensions
{
    public static Vector3 ToVector3(this int3 vec)
    {
        return new Vector3 { x = vec.x, y = vec.y, z = vec.z };
    }
}