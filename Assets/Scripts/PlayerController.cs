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
using UnityEngine.Scripting.APIUpdating;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : MonoBehaviour
{
    public bool displayCasts = false;

    public float MOVESPEED_DRAGGING = 1;
    public float MOVESPEED_WALK = 2;
    public float MOVESPEED_RUN = 4;
    public float MOVESPEED_SPRINT = 6;
    public float ANIM_TOL_WALK = 2;
    public float ANIM_TOL_RUN = 3;
    public float ANIM_TOL_SPRINT = 4;

    private bool isCrouching = false;

    Animator anim;
    new Rigidbody rigidbody;
    new CapsuleCollider collider;

    CharacterController controller;

    private const String BOOL_IDLE = "idle";
    private const String BOOL_WALK = "walk";
    private const String BOOL_RUN = "run";
    private const String BOOL_SPRINT = "sprint";
    private const String BOOL_CROUCHWALK = "crouchwalk";
    private const String BOOL_CROUCH = "crouch";


    void Awake()
    {
        this.anim = this.GetComponent<Animator>();
        this.rigidbody = this.GetComponent<Rigidbody>();
        this.collider = this.GetComponent<CapsuleCollider>();
        this.controller = this.GetComponent<CharacterController>();

        rigidbody.freezeRotation = true;

        climbWarmup = CLIMB_WARMUP_TIME;
    }

    List<Vector3> givenClimbPath = null;
    List<Vector3> givenVaultPath = null;
    public void OfferClimbPath(List<Vector3> path)
    {
        this.givenClimbPath = path;
    }

    public void RevokeClimbPath()
    {
        this.givenClimbPath = null;
    }

    public void RevokeVaultPath()
    {
        this.givenVaultPath = null;
    }

    public void OfferVaultPath(List<Vector3> path)
    {
        this.givenVaultPath = path;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    bool isColliding = false;
    bool isTouchingEnemy = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Wall")
        {
           // print("COLLIDED WITH WALL");
            isColliding = true;
        }

    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.tag == "Wall")
        {
            // print("COLLIDING WITH WALL");
            isColliding = true;
        }

        if (collision.collider.tag == "Enemy")
        {
            // print("COLLIDING WITH WALL");
            isTouchingEnemy = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.tag == "Wall")
        {
            //print("COLLIDED WITH WALL");
            isColliding = false;
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
            if(param.name != animation)
            {
                anim.SetBool(param.name, false);
            }
        }
    }

    Vector3 forward;
    Vector3 right;

    List<Vector3> savedClimbPath;
    List<Vector3> savedVaultPath;
    bool climbing = false;
    bool vaulting = false;

    public float CLIMB_WARMUP_TIME = 0.5f;
    float climbWarmup;

    int ignorePlayerMask = ~(1 << 8);

    // Update is called once per frame
    void Update()
    {

        CheckForEnemies();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleCrouching();
        }

        if (isCrouching && Input.GetKeyDown(KeyCode.LeftShift))
        {
            ToggleCrouching();
        }


       
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            if(givenClimbPath != null)
            {
                savedClimbPath = givenClimbPath;
                StartCoroutine(Climb());
            }
            else if(givenVaultPath != null)
            {
                savedVaultPath = givenVaultPath;
                StartCoroutine(Vault());
            } //Instant takedown variant
            // else if(enemyTarget != null)
            // {
            //     enemyTarget.KnockOut();
            // }
        }
        
     
        if(Input.GetKey(KeyCode.LeftShift))
        {
            if(enemyTargetHolding == null || enemyTarget != null)
            {
                //print("Setting holding target...");
                enemyTargetHolding = enemyTarget;
            }else if(enemyTargetHolding != null){
               // print("MOVE");
                enemyTargetHolding.SetCollision(false);

                // Calculate position/rotation to stick to
                Vector3 playerForward = transform.forward;
                Vector3 justInFront = transform.position + playerForward * 1;
                if(displayCasts) Debug.DrawLine(transform.position, justInFront);

                enemyTargetHolding.transform.position = justInFront;
               enemyTargetHolding.transform.rotation = transform.rotation;
               // enemyTargetHolding.transform.rotation = Quaternion.LookRotation(justInFront + 5*playerForward);
      
            }

        }
        if(Input.GetKeyUp(KeyCode.LeftShift) && enemyTargetHolding != null){
            enemyTargetHolding.SetCollision(true);
            enemyTargetHolding = null;
        }


        if (!climbing && !vaulting)
        {
            Move(enemyTargetHolding != null);
        }

        RaycastHit floorHit;
        Vector3 pos = transform.position;
        pos.y += 0.5f;
        if (Physics.Raycast(pos, transform.TransformDirection(-transform.up), out floorHit, Mathf.Infinity, ignorePlayerMask))
        {
            if(floorHit.collider.tag == "IndoorFloor")
            {
                floorHit.collider.transform.parent.GetComponent<IndoorFloorScript>().HideCeilingAndWalls();
            }
        };

    }

    public float enemyCheckDistance = 10000;
    public int enemyChecknH = 8;
    public float enemeyCheckH = 0.7f;

    private void CheckForEnemies()
    {

        Vector3 pos = transform.position;
        pos.y += 2;

        Vector3 fwd = transform.forward;
        Vector3 right = transform.right;

        float dh = (enemeyCheckH * 2) / enemyChecknH;

        EnemyScript possibleEnemy = null;

        for (int i = 0; i <= enemyChecknH; i++)
        {
            float thisH = -enemeyCheckH + i * dh;

            Vector3 vec = Vector3.Normalize(fwd + right * thisH) * enemyCheckDistance;
            Vector3 origin = pos;

            if(displayCasts) Debug.DrawRay(origin, vec, Color.magenta);


            RaycastHit hit;
            if (Physics.Raycast(origin, vec, out hit, enemyCheckDistance, ignorePlayerMask))
            {
                if (hit.collider != null)
                {

                    if (hit.collider.tag == "Enemy")
                    {
                        possibleEnemy = hit.collider.gameObject.GetComponent<EnemyScript>();
                    }
                 
                }
            }

        }

        if (possibleEnemy!=null) OfferKill(possibleEnemy);
        else RevokeKillOffer();
    }

    EnemyScript enemyTarget;
    EnemyScript enemyTargetHolding;

    private void OfferKill(EnemyScript enemyScript)
    {
        enemyTarget = enemyScript;
        enemyScript.OnTargeted();
    }

    private void RevokeKillOffer()
    {
        if(enemyTarget != null) enemyTarget.CancelTargeting();
        enemyTarget = null;
    }


    public const float STANDING_COLLIDER_HEIGHT = 4.5f;
    public const float STANDING_COLLIDER_CENTER = 2.25f;
    public const float CROUCHING_COLLIDER_HEIGHT = 2.25f;
    public const float CROUCHING_COLLIDER_CENTER = 1.1f;

    private void ToggleCrouching()
    {
        RaycastHit upHit;

        if (Physics.Raycast(transform.position, transform.up, out upHit, Mathf.Infinity, ~(1 << 8)))
        {
            print((upHit.collider.tag == "Wall") + " " + upHit.distance + " " + isCrouching);

            if (upHit.distance < STANDING_COLLIDER_HEIGHT && isCrouching)
            {
                print("STOPPED upHit: " + upHit.distance);
                return;
            }
        }

        isCrouching = !isCrouching;


        collider.height = isCrouching ? CROUCHING_COLLIDER_HEIGHT : STANDING_COLLIDER_HEIGHT;
        collider.center =
            new Vector3(collider.center.x, isCrouching ? CROUCHING_COLLIDER_CENTER : STANDING_COLLIDER_CENTER, collider.center.z);

        controller.height = collider.height;
        controller.center = collider.center;
    }

    public float climbMoveSpeed = 3;

    private IEnumerator Climb()
    {
        climbing = true;
        controller.enabled = false;
        Animate(anim, BOOL_IDLE);
    
        Vector3 target1 = savedClimbPath[0];
        Vector3 target2 = savedClimbPath[1];
        Vector3 target3 = savedClimbPath[2];

        Quaternion q = Quaternion.LookRotation(target3 - transform.position);
        q.x = 0;
        q.z = 0;
        transform.rotation = q;

        foreach(Vector3 target in savedClimbPath)
        {
            while(Vector3.Distance(transform.position, target) > 0.05)
            {
                transform.position += (target - transform.position) * climbMoveSpeed * Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        climbing = false;
        controller.enabled = true;
    }

    private IEnumerator Vault()
    {
        vaulting = true;
        controller.enabled = false;
        Animate(anim, BOOL_IDLE);

        float startY = transform.position.y;

       foreach(Vector3 target in savedVaultPath)
        {
            while(Vector3.Distance(transform.position, target) > 0.05)
            {
                transform.position += (target - transform.position) * climbMoveSpeed * Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        Vector3 finalTarget = transform.position;
        finalTarget.y = startY;

        while(Vector3.Distance(transform.position, finalTarget) > 0.05)
            {
                transform.position += (finalTarget - transform.position) * climbMoveSpeed * Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

        vaulting = false;
        controller.enabled = true;
    }

    private void Move(bool isHoldingEnemy)
    {
        // Establishes rotation speed and movespeed
        float rotSpeed = 8;
        float moveSpeed = isCrouching ? MOVESPEED_WALK : Input.GetKey(KeyCode.LeftShift) ? !isHoldingEnemy ? MOVESPEED_SPRINT : MOVESPEED_DRAGGING : MOVESPEED_RUN;
        float velocity = 0;

        // Calculates forward and right vectors relative to camera
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;

        if (Input.anyKey)
        {

            // setup a direction Vector based on keyboard input.
            // GetAxis returns a value between -1.0 and 1.0. If the A key is pressed, GetAxis(HorizontalKey) will return -1.0.
            // If D is pressed, it will return 1.0
            Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")); 

            if (direction != Vector3.zero)
            {
                // Our right movement is based on the right vector, movement speed, and our GetAxis command.
                Vector3 rightMovement = Vector3.Normalize(right * Input.GetAxis("Horizontal"))*moveSpeed*Time.deltaTime;

                // Up movement uses the forward vector, movement speed, and the vertical axis inputs.
                Vector3 upMovement = Vector3.Normalize(forward * Input.GetAxis("Vertical")) * moveSpeed * Time.deltaTime;


                // Detect walls
                Vector3 playerFacingWorld = transform.TransformDirection(Vector3.forward);
                Vector3 playerLeft = transform.TransformDirection(Vector3.left);
                Vector3 playerRight = transform.TransformDirection(Vector3.right);

              
                controller.Move(rightMovement + upMovement);
                velocity = Vector3.Magnitude(controller.velocity);


                Vector3 heading = Vector3
                    .Normalize(isHoldingEnemy ? -controller.velocity : controller.velocity);

                if (heading != Vector3.zero)
                {
                    Quaternion newRot = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(heading), Time.deltaTime * rotSpeed);
                    newRot.x = 0;
                    newRot.z = 0;
                    transform.rotation = newRot;
                }

                    

                }

            }
            else
            {
                Animate(anim, BOOL_IDLE);
            }


        if (velocity < 0.1){
            if(isCrouching) Animate(anim, BOOL_CROUCH);
            else Animate(anim, BOOL_IDLE);
        } 
        else if(isCrouching) Animate(anim, BOOL_CROUCHWALK);
        else if (velocity < ANIM_TOL_RUN) Animate(anim, BOOL_WALK);
        else if (velocity < ANIM_TOL_SPRINT) Animate(anim, BOOL_RUN);
        else Animate(anim, BOOL_SPRINT);

        //print(velocity);

        }
        //else
        //{
        //    Animate(anim, BOOL_IDLE);
        //}
    
}
