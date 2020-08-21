using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform playerTransform;
    new public Camera camera;

    public float angleIncrements = 90;
    public float upwardsTilt = 15;

    public float smoothSpeed = 2f;
    public float rotSpeed = 5f;

    public Vector3 offset;
    public float offsetFactor = 2;
    float currentYRotation = 45;


    private void Update()
    {

        if (Input.anyKeyDown)
        {

            if (Input.GetKeyDown(KeyCode.X))
            {
                currentYRotation = (currentYRotation + angleIncrements);

            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                currentYRotation = (currentYRotation - angleIncrements);
            }

            //switch (currentYRotation % 360)
            //{
            //    case 45:
            //        offset.x = -offsetFactor; offset.z = -offsetFactor;
            //        break;
            //    case -315:
            //        offset.x = -offsetFactor; offset.z = -offsetFactor;
            //        break;
            //    case 135:
            //        offset.x = -offsetFactor; offset.z = offsetFactor;
            //        break;
            //    case -225:
            //        offset.x = -offsetFactor; offset.z = offsetFactor;
            //        break;
            //    case 225:
            //        offset.x = offsetFactor; offset.z = offsetFactor;
            //        break;
            //    case -135:
            //        offset.x = offsetFactor; offset.z = offsetFactor;
            //        break;
            //    case 315:
            //        offset.x = offsetFactor; offset.z = -offsetFactor;
            //        break;
            //    case -45:
            //        offset.x = 2; offset.z = -2;
            //        break;
            //}

            //transform.rotation =  Quaternion.Euler(30, currentYRotation, 0);
            //transform.rotation = Quaternion.Lerp(transform.rotation, new Quaternion(30, currentYRotation, 0, 0), smoothSpeed * Time.deltaTime);
        }

    }

    private void FixedUpdate()
    {
        Vector3 targetPosition = playerTransform.position + offset;
        Quaternion targetRotation = Quaternion.Euler(upwardsTilt, currentYRotation, 0);

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);

    }

}
