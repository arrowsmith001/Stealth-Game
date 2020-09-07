using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public float height = 4;
    public List<GameObject> navPoints;
    public int navPointIndex;

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

        if(navPoints.Count != 0) navPointIndex = 0;
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
            NavPointData navData = collider.gameObject.GetComponent<NavPointScript>().GetNavData();

            //if(navPoints[navPointIndex] != collider.gameObject) return;

            if(navData.angles.Length == 0 && navData.times.Length == 0
                || navData.angles.Length != navData.times.Length)
            {
                navPointIndex = (navPointIndex + 1) % navPoints.Count;
            }
            else
            {
                StartCoroutine(ActOnNavData(navData));
            }
        }
    }

    public void Alert()
    {
        billBoard.OnAlert();
    }

    public bool isActingOnNavData = false;
    private IEnumerator ActOnNavData(NavPointData navData)
    {
        //print("ActOnNavData CALLED");

        isActingOnNavData = true;
        Animate(anim, BOOL_IDLE);

        // Initialise direction
        //transform.rotation = navData.direction;

        for (int i = 0; i < navData.angles.Length; i++)
        {
            currentNavPoint = navData.GetNavPoint(i);
            yield return new WaitForSeconds(navData.times[i]);
        }

        //print("navpointbefore: "+navPointIndex);
        navPointIndex = (navPointIndex + 1) % navPoints.Count;
        //print("navpointafter: "+navPointIndex);

        isActingOnNavData = false;
        Animate(anim, BOOL_WALK);

        if(navPoints.Count == 1){ 
            StartCoroutine(ActOnNavData(navData));
        }
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

    NavPointSingle currentNavPoint;

    // Update is called once per frame
    void Update()
    {
        if(!knockedOut)
        {
            if (navPoints.Count > 0 && !isActingOnNavData)
            {
                float rotSpeed = 8;

                GameObject nextNavPoint = navPoints[navPointIndex];
                Vector3 navVec = nextNavPoint.transform.position - transform.position;

                navVec = Vector3.Normalize(navVec);
                navVec = navVec * MOVESPEED_WALK * Time.deltaTime;

                controller.Move(navVec);

                Vector3 heading = Vector3.Normalize(controller.velocity);
                if (heading != Vector3.zero)
                {
                    Quaternion newRot = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(heading), Time.deltaTime * rotSpeed);
                    newRot.x = 0;
                    newRot.z = 0;
                    transform.rotation = newRot;
                }
            }



            if (isActingOnNavData)
            {
                float rotSpeed = 5;

                Quaternion newRot = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, currentNavPoint.angle, 0), Time.deltaTime * rotSpeed);
                newRot.x = 0;
                newRot.z = 0;

                if (UtilsClass.Approximately(transform.rotation, newRot, 0.0005f)) { Animate(anim, BOOL_IDLE); }
                else
                {
                    Animate(anim, BOOL_WALK);
                }

                transform.rotation = newRot;
            }
        }
            
        


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
