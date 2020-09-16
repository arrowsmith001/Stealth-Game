using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VaultBarScript : MonoBehaviour
{
    public Transform bar1;
    public Transform bar2;


    bool triggerEntered = false;
    new Collider collider;

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.tag != "Player") return;

        this.collider = other;

        triggerEntered = true;

    }

    private void OnTriggerExit(Collider other)
    {
        if(other.transform.tag != "Player") return;

        triggerEntered = false;

        other.gameObject.GetComponent<PlayerController>().RevokeVaultPath();

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

            // Which bar is nearest?
            float distTo1 = Vector3.Distance(playerPos, bar1.transform.position);
            float distTo2 = Vector3.Distance(playerPos, bar2.transform.position); 
            Vector3 thisBar1 = distTo1 < distTo2 ? bar1.transform.position : bar2.transform.position;
            Vector3 thisBar2 = distTo1 < distTo2 ? bar2.transform.position : bar1.transform.position;


            Vector3 target1 = playerPos;
            target1.y = thisBar1.y;

            Vector3 target2 = target1;
            target2 += thisBar2 - thisBar1; 


            List<Vector3> path = new List<Vector3>(){target1, target2, };

            collider.gameObject.GetComponent<PlayerController>().OfferVaultPath(path);
        }

    }
}
