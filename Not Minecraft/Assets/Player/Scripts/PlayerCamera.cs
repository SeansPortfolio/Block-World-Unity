using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{

    public Camera Camera;

    public Vector3 MouseDragStart;

    public Vector3 MouseDragEnd;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var mousePos = Input.mousePosition;
            mousePos.z = 10;

            MouseDragStart = Camera.ScreenToWorldPoint(mousePos);
        }

        if(Input.GetMouseButton(0))
        {
            var mousePos = Input.mousePosition;
            mousePos.z = 10;

            MouseDragEnd = Camera.ScreenToWorldPoint(mousePos);

        }

    }
}
