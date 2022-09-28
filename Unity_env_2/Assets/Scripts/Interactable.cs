using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [RequiredComponent(typeof(Rigidbody))]

public class Interactable : MonoBehaviour
{
    [HideInInspector]
    public Hand m_ActiveHand = null;
    public bool readyAsTarget = false;
    public bool arrivedAtTarget = false;
    public GameObject realCup = null;


    private void Update()
    {
        if(readyAsTarget)
        {  
            if(arrivedAtTarget)
            {
                transform.GetComponent<Renderer>().material.color = Color.green;
            }
            else
            {
                transform.GetComponent<Renderer>().material.color = Color.red;
            }
        }
        else
        {
            transform.GetComponent<Renderer>().material.color = Color.gray;
        }

    }
}
