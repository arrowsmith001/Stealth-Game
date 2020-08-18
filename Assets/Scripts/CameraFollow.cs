using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 2f;
    public float rotSpeed = 5f;

    public Vector3 offset;
    float currentYRotation = 45;

    private void Update()
    {

        if(Input.anyKeyDown)
        {

            if (Input.GetKeyDown(KeyCode.X))
             {
                currentYRotation = (currentYRotation + 90);

            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                currentYRotation = (currentYRotation - 90);
            }

            switch (currentYRotation % 360)
            {
                case 45:
                    offset.x = -200; offset.z = -200;
                    break;
                case -315:
                    offset.x = -200; offset.z = -200;
                    break;
                case 135:
                    offset.x = -200; offset.z = 200;
                    break;
                case -225:
                    offset.x = -200; offset.z = 200;
                    break;
                case 225:
                    offset.x = 200; offset.z = 200;
                    break;
                case -135:
                    offset.x = 200; offset.z = 200;
                    break;
                case 315:
                    offset.x = 200; offset.z = -200;
                    break;
                case -45:
                    offset.x = 200; offset.z = -200;
                    break;
            }

            //transform.rotation =  Quaternion.Euler(30, currentYRotation, 0);
            //transform.rotation = Quaternion.Lerp(transform.rotation, new Quaternion(30, currentYRotation, 0, 0), smoothSpeed * Time.deltaTime);
        }

     }

    private void FixedUpdate()
    {
        Vector3 targetPosition = target.position + offset;
        Quaternion targetRotation = Quaternion.Euler(30, currentYRotation, 0);

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);

    }
}
