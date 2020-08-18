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

    public float MOVESPEED_WALK = 2;
    public float MOVESPEED_RUN = 4;
    public float MOVESPEED_SPRINT = 6;

    private bool isCrouching = false;

    Animator anim;
    new Rigidbody rigidbody;
    new CapsuleCollider collider;

    CharacterController controller;

    private const String BOOL_IDLE = "idle";
    private const String BOOL_WALK = "walk";
    private const String BOOL_RUN = "run";
    private const String BOOL_SPRINT = "sprint";


    void Awake()
    {
        this.anim = this.GetComponent<Animator>();
        this.rigidbody = this.GetComponent<Rigidbody>();
        this.collider = this.GetComponent<CapsuleCollider>();
        this.controller = this.GetComponent<CharacterController>();

        rigidbody.freezeRotation = true;

        vaultWarmup = VAULT_WARMUP_TIME;
    }

    Vector3? givenVaultPos = null;
    public void OfferVaultPos(Vector3 vector3)
    {
        this.givenVaultPos = vector3;
    }

    public void RevokeVaultPos()
    {
        this.givenVaultPos = null;
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

    Vector3? savedVaultPos;
    bool vaulting = false;

    public float VAULT_WARMUP_TIME = 0.5f;
    float vaultWarmup;

    int ignorePlayerMask = ~(1 << 8);

    // Update is called once per frame
    void Update()
    {

        RaycastHit upHit;

        if (Physics.Raycast(transform.position, transform.up, out upHit, Mathf.Infinity, ignorePlayerMask))
        {
            Debug.DrawLine(transform.position, upHit.point, Color.white);
        }
        RaycastHit fwdHit;
        if (Physics.Raycast(transform.position, transform.up, out fwdHit, Mathf.Infinity, ignorePlayerMask))
        {
            Debug.DrawLine(transform.position, fwdHit.point, Color.red);
        }

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
            // Attack!
            if(enemyTarget != null)
            {
                print("Killed");

            } // Then resolve vaulting
            else if(givenVaultPos != null)
            {

            savedVaultPos = givenVaultPos;
            vaulting = true;
            }
        }

        if(vaulting)
        {
            if (vaultWarmup > 0) vaultWarmup -= Time.deltaTime;
            else
            {
                Vault();
            }
            
        }


        if (!vaulting)
        {
            Move();
        }
    }

    public float enemyCheckDistance = 10000;
    public int enemyChecknH = 8;
    public float enemeyCheckH = 0.7f;

    private void CheckForEnemies()
    {

        Vector3 pos = transform.position;
        pos.y += 100;

        Vector3 fwd = transform.forward;
        Vector3 right = transform.right;

        float dh = (enemeyCheckH * 2) / enemyChecknH;

        EnemyScript possibleEnemy = null;

        for (int i = 0; i <= enemyChecknH; i++)
        {
            float thisH = -enemeyCheckH + i * dh;

            Vector3 vec = Vector3.Normalize(fwd + right * thisH) * enemyCheckDistance;
            Vector3 origin = pos;

            Debug.DrawRay(origin, vec, Color.magenta);


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


    private void ToggleCrouching()
    {
        RaycastHit upHit;

        if (Physics.Raycast(transform.position, transform.up, out upHit, Mathf.Infinity, ~(1 << 8)))
        {
            //print((upHit.collider.tag == "Wall") + " " + upHit.distance + " " + isCrouching);

            if (upHit.distance < 120 && isCrouching)
            {
                //print("STOPPED upHit: " + upHit.distance);
                return;
            }
        }

        isCrouching = !isCrouching;

        collider.height = isCrouching ? 100 : 200;
        collider.center = new Vector3(collider.center.x, isCrouching ? 50 : 100, collider.center.z);

        controller.height = collider.height;
        controller.center = collider.center;
    }


    public float vaultMoveSpeed = 50;
    private void Vault()
    {
        Debug.DrawRay(savedVaultPos.Value, Vector3.up * 1000, Color.magenta);

        if (Vector3.Distance(savedVaultPos.Value, transform.position) > 10)
        {
            print("MOVE ACROSS");
            controller.Move((savedVaultPos.Value - transform.position) * vaultMoveSpeed);
            Debug.DrawRay(transform.position, (savedVaultPos.Value - transform.position) * vaultMoveSpeed);
            Debug.Log(Vector3.Distance(savedVaultPos.Value, transform.position));
        }
        else
        {
            vaultWarmup = VAULT_WARMUP_TIME;
            vaulting = false;
        }
    }

    private void Move()
    {
        // Establishes rotation speed and movespeed
        float rotSpeed = 8;
        float moveSpeed = isCrouching ? MOVESPEED_WALK : Input.GetKey(KeyCode.LeftShift) ? MOVESPEED_SPRINT : MOVESPEED_RUN;
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
                Vector3 rightMovement = right * moveSpeed * Input.GetAxis("Horizontal");

                // Up movement uses the forward vector, movement speed, and the vertical axis inputs.
                Vector3 upMovement = forward * moveSpeed * Input.GetAxis("Vertical");


                // Detect walls
                Vector3 playerFacingWorld = transform.TransformDirection(Vector3.forward);
                Vector3 playerLeft = transform.TransformDirection(Vector3.left);
                Vector3 playerRight = transform.TransformDirection(Vector3.right);

              
                controller.Move(rightMovement + upMovement);
                velocity = Vector3.Magnitude(controller.velocity);


                Vector3 heading = Vector3.Normalize(controller.velocity);

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


            if (velocity <= 0.5) Animate(anim, BOOL_IDLE);
        else if (moveSpeed == MOVESPEED_WALK) Animate(anim, BOOL_WALK);
        else if (moveSpeed == MOVESPEED_RUN) Animate(anim, BOOL_RUN);
        else if (moveSpeed == MOVESPEED_SPRINT) Animate(anim, BOOL_SPRINT);

        //print(velocity);

        }
        //else
        //{
        //    Animate(anim, BOOL_IDLE);
        //}
    
}
