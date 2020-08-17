using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : MonoBehaviour
{
    public float BASE_SPEED = 3f;

    Animator anim;

    private const String BOOL_IDLE = "idle";
    private const String BOOL_RUN = "run";
    private const String BOOL_SPRINT = "sprint";


    void Awake()
    {
        this.anim = this.GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Animate(Animator anim, String animation)
    {
        DisableOtherAnimations(anim, animation);
        anim.SetBool(animation, true);
    }

    void DisableOtherAnimations(Animator anim, String animation)
    {
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if(param.name != animation)
            {
                anim.SetBool(param.name, false);
            }
        }
    }

    Vector3 forward;
    Vector3 right;

    // Update is called once per frame
    void Update()
    {

        float rotSpeed = 8;
        float moveSpeed = Input.GetKey(KeyCode.LeftShift) ? 5 : 3;

        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;

        if (Input.anyKey)
        {
            Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")); // setup a direction Vector based on keyboard input. GetAxis returns a value between -1.0 and 1.0. If the A key is pressed, GetAxis(HorizontalKey) will return -1.0. If D is pressed, it will return 1.0

            Vector3 rightMovement = right * moveSpeed
                //* Time.deltaTime
                * Input.GetAxis("Horizontal"); // Our right movement is based on the right vector, movement speed, and our GetAxis command. We multiply by Time.deltaTime to make the movement smooth.
            Vector3 upMovement = forward * moveSpeed
                //* Time.deltaTime
                * Input.GetAxis("Vertical"); // Up movement uses the forward vector, movement speed, and the vertical axis inputs.Vector3 heading = Vector3.Normalize(rightMovement + upMovement); // This creates our new direction. By combining our right and forward movements and normalizing them, we create a new vector that points in the appropriate direction with a length no greater than 1.0transform.forward = heading; // Sets forward direction of our game object to whatever direction we're moving in

            Vector3 newPos = transform.position + rightMovement + upMovement;
            print(rightMovement + " " + upMovement);
            transform.position = newPos;


            if (moveSpeed == 0) Animate(anim, BOOL_IDLE);
            else if (moveSpeed == 3) Animate(anim, BOOL_RUN);
            else Animate(anim, BOOL_SPRINT);


            Vector3 heading = Vector3.Normalize(rightMovement + upMovement);
            if (heading != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(heading), Time.deltaTime * rotSpeed);
            }
        }
        else
        {
            Animate(anim, BOOL_IDLE);
        }


    }

    void Move()
    {
        
    }

    float PythDist(Collection<float> args)
    {
        if (args.Count == 0) return 0;

        double num = 0;
        foreach(float f in args)
        {
            num += Math.Pow((double) f, 2);
        }

        if (num == 0) return 0;
        else return (float) Math.Sqrt(num);
    }
    
}
