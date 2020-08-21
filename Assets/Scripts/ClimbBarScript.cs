using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbBarScript : MonoBehaviour
{
    public Transform climbable;

    // Start is called before the first frame update
    void Start()
    {
        
    }


    bool triggerEntered = false;
    new Collider collider;

    private void OnTriggerEnter(Collider other)
    {
       Debug.Log("PLAYER COLLISION");

        this.collider = other;
        triggerEntered = true;

    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("PLAYER COLLISION ENDED");

        triggerEntered = false;

        other.gameObject.GetComponent<PlayerController>().RevokeVaultPos();

    }

    public float VAULT_FORWARD_FACTOR = 4;

    // Update is called once per frame
    void Update()
    {
        Vector3 vec = climbable.transform.position - transform.position;
        vec.y = 0;
        Debug.DrawRay(transform.position, vec, Color.white);

        if(triggerEntered)
        {
            Vector3 playerPos = collider.transform.position;

            float height = climbable.localScale.y;
         //   print("HEIGHT: " + height);

            playerPos.y = climbable.position.y + height/2 + 5;

            Debug.DrawRay(playerPos, vec);

            collider.gameObject.GetComponent<PlayerController>().OfferVaultPos(playerPos + VAULT_FORWARD_FACTOR*vec);
        }

    }
}
