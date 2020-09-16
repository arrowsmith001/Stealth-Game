using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbBarScript : MonoBehaviour
{
    public Transform lower;
    public Transform middle;
    public Transform upper;


    bool triggerEntered = false;
    new Collider collider;

    private void OnTriggerEnter(Collider other)
    {
       print("PLAYER COLLISION");

        this.collider = other;
        triggerEntered = true;

    }

    private void OnTriggerExit(Collider other)
    {
        print("PLAYER COLLISION ENDED");

        triggerEntered = false;

        other.gameObject.GetComponent<PlayerController>().RevokeClimbPath();

    }

    public float VAULT_FORWARD_FACTOR = 4;

    // Update is called once per frame
    void Update()
    {
        // Vector3 vec = climbable.transform.position - transform.position;
        // vec.y = 0;
        // Debug.DrawRay(transform.position, vec, Color.white);

        if(triggerEntered)
        {
            Vector3 playerPos = collider.transform.position;

            Vector3 target1 = playerPos;
            target1.y = lower.transform.position.y;

            Vector3 target2 = target1;
            target2.y += middle.transform.position.y - lower.transform.position.y; 

            Vector3 target3 = target2 + (upper.transform.position - middle.transform.position);

            List<Vector3> path = new List<Vector3>(){target1, target2, target3};

            collider.gameObject.GetComponent<PlayerController>().OfferClimbPath(path);
        }

    }
}
