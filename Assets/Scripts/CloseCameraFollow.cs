using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CloseCameraFollow : MonoBehaviour
{
    public bool fixHeight;
    public bool fixYTilt = false;
    public float fixedHeightOffset = 1;
    public float X_TILT = 10;
    public float Y_TILT = 0;
    public float Z_TILT = 100;
    public GameObject target;
    public float followDistance = 50;
    public float CAMERA_INPUT_MOVE_SPEED = 50;
    public float CAMERA_INPUT_STEP_SIZE = 10;
    public float CAMERA_MOVE_SPEED = 10;

    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        // Camera forward
        Vector3 forward = transform.forward;
        Vector3 up = transform.up;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        Vector3 right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;            
        
        Vector3 direction = new Vector3();
        if(Input.GetKey(KeyCode.LeftControl)) direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")); 

        Vector3 targetPos = new Vector3();

        
        Vector3 rightCameraMovement = Vector3.Normalize(right * direction.x);//* CAMERA_INPUT_MOVE_SPEED *Time.deltaTime;
        Vector3 upCameraMovement = Vector3.Normalize(up * direction.z);// * CAMERA_INPUT_MOVE_SPEED * Time.deltaTime;
        //transform.position = Vector3.Slerp(transform.position, targetPos, CAMERA_INPUT_MOVE_SPEED * Time.deltaTime);
        Vector3 cameraInputMovement =  CAMERA_INPUT_STEP_SIZE * (rightCameraMovement + upCameraMovement).normalized; 

        // Away from player vector
        Vector3 towardTarget = target.transform.position - transform.position;
        Vector3 awayFromTarget = -towardTarget;
        

        // Correct distance
        targetPos = target.transform.position + (awayFromTarget.normalized)*followDistance + cameraInputMovement;
        Vector3 towardsTargetPosition = targetPos - transform.position;
        transform.position = Vector3.Slerp(transform.position, targetPos, CAMERA_MOVE_SPEED * Time.deltaTime);
    

        if(fixHeight)
        {
           Vector3 pos = transform.position;
           pos.y = fixedHeightOffset;
           transform.position = pos;
        }

        // Rotation towards player
        if(!fixYTilt)
        {
            Quaternion q = Quaternion.FromToRotation(transform.forward, towardTarget) * transform.rotation;
            q = Quaternion.Slerp(transform.rotation, q, CAMERA_MOVE_SPEED * Time.deltaTime);

            transform.rotation = Quaternion.Euler(new Vector3(X_TILT, q.eulerAngles.y, Z_TILT));
        }
        else
        {
            transform.rotation = Quaternion.Euler(new Vector3(X_TILT, Y_TILT, Z_TILT));
        }
        

        
    }
}
