using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BillBoard : MonoBehaviour
{
    public EnemyScript enemy;

    public Image board;
    public Sprite[] images;

    Sprite alertImage;
    Sprite targetImage;

    private void Awake()
    {
        alertImage = images[0];
        targetImage = images[1];

        board = transform.GetComponent<Image>();
        board.gameObject.SetActive(false);

    }

    public void OnAlert()
    {
        StopAllCoroutines();

        board.gameObject.SetActive(true);
        StartCoroutine(Alert());
    }

    IEnumerator Alert()
    {
        board.sprite = alertImage;
        yield return new WaitForSeconds(2);
        board.sprite = null;
        board.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
            Camera.main.transform.rotation * Vector3.up);
    }

    public void OnTarget()
    {
        StopAllCoroutines();

        board.gameObject.SetActive(true);

        board.sprite = targetImage;
    }

 

    public void CancelTargeting()
    {
        board.sprite = null;
        board.gameObject.SetActive(false);
    }
}
