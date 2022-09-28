using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualCupPosUpdater : MonoBehaviour
{

    public Transform original_trans;
    public GameObject selector;
    public Interactable m_CurrentInteractable = null;
    // Start is called before the first frame update
    void Start()
    {

        original_trans = transform;
        selector = transform.parent.GetChild(1).gameObject;
        m_CurrentInteractable = selector.GetComponent<Interactable>();
    }

    // Update is called once per frame
    void Update()
    {
        if(m_CurrentInteractable.arrivedAtTarget)
        {
            transform.position = m_CurrentInteractable.realCup.transform.position;
            transform.rotation = m_CurrentInteractable.realCup.transform.rotation;
        }
        else
        {
            transform.position = original_trans.position;
        }
        
    }
}
