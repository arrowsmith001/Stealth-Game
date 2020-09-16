using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public bool displayNavMessages = false;
    public float height = 4;
    Queue<NavData> navQ = new Queue<NavData>();
    public List<GameObject> navPoints;
    public int navPointIndex = 0;

    GameObject GetNextNavPoint(){
        
        int nextIndex = (navPointIndex + 1) % navPoints.Count;
        navPointIndex = nextIndex;

        return navPoints[nextIndex];
    }

    public FOVScript fovScript;
    public BillBoard billBoard;

    private const String BOOL_IDLE = "idle";
    private const String BOOL_WALK = "walk";
    private const String BOOL_RUN = "run";
    private const String BOOL_SPRINT = "sprint";

    public float MOVESPEED_WALK = 4;
    public float MOVESPEED_RUN = 8;

    public Vector3 headTowards;

    Animator anim;
    new Rigidbody rigidbody;
    new CapsuleCollider collider;
    CharacterController controller;


    private void Awake()
    {

        this.anim = this.GetComponent<Animator>();
        this.rigidbody = this.GetComponent<Rigidbody>();
        this.collider = this.GetComponent<CapsuleCollider>();
        this.controller = this.GetComponent<CharacterController>();

        rigidbody.freezeRotation = true;

        if(navPoints.Count != 0) 
        {
            navQ.Enqueue(new NavData(new NavCommand(Commands.GO, navPoints[0].transform.position)));
        }
    }

    private void QueueNextNavPoint()
    {
        NavData nd = navPoints[navPointIndex].GetComponent<NavPointScript>().GetNavData();
        nd.AddCommand(new NavCommand(Commands.GO, GetNextNavPoint().transform.position));
        navQ.Enqueue(nd);
    }

    // Start is called before the first frame update
    void Start()
    {
        Animate(anim, BOOL_WALK);
    }

    private void OnTriggerEnter(Collider collider)
    {
        //print("TRIGGER ENTERED");

        if (collider.tag == "NavPoint")
        {
            GameObject point = collider.gameObject;
            navPointIndex = navPoints.IndexOf(point);

            QueueNextNavPoint();
        }
    }

    public void Alert()
    {
        billBoard.OnAlert();
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
            if (param.name != animation)
            {
                anim.SetBool(param.name, false);
            }
        }
    }

    NavCommand currentNavPoint;
    int g;

    Vector3? targetPosition = null;
    Vector3? targetRotation = null;

    // Update is called once per frame
    void Update()
    {
        ActOnQueue();

        MoveToTarget();
        RotateToTarget();

    }

    private void RotateToTarget()
    {
        if(targetRotation == null) return;

        float rotSpeed = 5;

        Quaternion newRot = Quaternion.Slerp(transform.rotation, Quaternion.Euler((Vector3) targetRotation), Time.deltaTime * rotSpeed);

        if (UtilsClass.Approximately(transform.rotation, newRot, 0.0005f)) { 
            Animate(anim, BOOL_IDLE);
            }
        else
        {
            Animate(anim, BOOL_WALK);
        }

        transform.rotation = newRot;
    }

    private void MoveToTarget()
    {
        if(targetPosition == null) {
            Animate(anim, BOOL_IDLE);
            return;
        }

        float rotSpeed = 8;

        Vector3 navVec = (Vector3) targetPosition - transform.position;
        if(navVec.magnitude < 0.5){
            return;
        }


        navVec = Vector3.Normalize(navVec);
        navVec = navVec * MOVESPEED_WALK * Time.deltaTime;

        controller.Move(navVec);

        Vector3 vel = controller.velocity;
        float absVel = vel.magnitude;


        Vector3 heading = Vector3.Normalize(vel);
        if (heading != Vector3.zero)
        {
            Animate(anim, BOOL_WALK);

            Quaternion newRot = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(heading), Time.deltaTime * rotSpeed);
            newRot.x = 0;
            newRot.z = 0;

            transform.rotation = newRot;
        }
        else
        {
            
            Animate(anim, BOOL_IDLE);
        }
    }

    NavData currentNavData = null;
    bool waiting = false;
    private void ActOnQueue()
    {
        if(waiting) return;

        if(currentNavData == null){
            if(navQ.Count == 0) return;
            else currentNavData = navQ.Dequeue();
        }   

        
        if(currentNavData.Next())
        {
            NavCommand command = currentNavData.GetCommand();

            if(displayNavMessages) print("Acting on command: " + command.command.ToString() + " ("+ currentNavData.GetIndex() + "/" + currentNavData.Count() + ")");

            Commands cmd = command.command;
            Vector3 args = command.args;

            switch(cmd)
            {
                case Commands.GO: // args = position vector
                
                targetRotation = null;
                targetPosition = args;

                break;
                case Commands.ROTATE: // args = euler angles

                targetRotation = args;

                break;
                case Commands.LOOK: // args = direction vector

                // TODO

                break;
                case Commands.WAIT: // args = time(? ,seconds, milliseconds)

                StartCoroutine(Wait(args));

                break;
                case Commands.REACT: // args = tbc
                
                // TODO

                break;
            }

        }
        else
        {
            currentNavData = null;
        }
        


    }

    void ClearQueue(){
        navQ.Clear();
        currentNavData = null;
        waiting = false;
        StopAllCoroutines();
    }

    private IEnumerator Wait(Vector3 args)
    {
        waiting = true;
        int secs = (int) args.x;
        yield return new WaitForSeconds(secs);
        waiting = false;
    }

    bool knockedOut = false;
    public void KnockOut()
    {
        knockedOut = true;
    }

    public void OnTargeted()
    {
        billBoard.OnTarget();
    }

    public void CancelTargeting()
    {
        billBoard.CancelTargeting();
    }

    public void SetCollision(bool enabled){
        this.GetComponent<CapsuleCollider>().enabled = enabled;
        this.GetComponent<CharacterController>().enabled = enabled;
    }
}
