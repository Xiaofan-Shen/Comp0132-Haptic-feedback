using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Hand : MonoBehaviour
{
    public SteamVR_Action_Boolean m_GrabAction = null;
    public GameObject Push_Planner = null; 
    [HideInInspector]
    public PushPlanner m_CurrentPushPlanner = null;

    private SteamVR_Behaviour_Pose m_Pose = null;
    private FixedJoint m_Joint = null;

    public Interactable m_CurrentInteractable = null;
    // private Interactable m_PrevInteractable = null;

    private List<Interactable> m_ContactInteractables = new List<Interactable>();

    private Vector3 prev_hand_pos, delta_pos; 
    private void Awake() 
    {
        m_Pose = GetComponent<SteamVR_Behaviour_Pose>();
        m_Joint = GetComponent<FixedJoint>();
        m_CurrentPushPlanner = Push_Planner.GetComponent<PushPlanner>();
    }


    // Update is called once per frame
    private void Update()
    {
        // Down
        if(m_GrabAction.GetStateDown(m_Pose.inputSource))
        {
            print(m_Pose.inputSource + " Trigger Down");
            Pickup();
        }

        // Up
        if(m_GrabAction.GetStateUp(m_Pose.inputSource))
        {
            print(m_Pose.inputSource + " Trigger Up");
            Drop();
        }



    }

    private void OnTriggerEnter(Collider other) 
    {
        if(!other.gameObject.CompareTag("Interactable"))
            return;
        m_ContactInteractables.Add(other.gameObject.GetComponent<Interactable>());
    }

    private void OnTriggerExit(Collider other)
    {

        if(!other.gameObject.CompareTag("Interactable"))
            return;

        m_ContactInteractables.Remove(other.gameObject.GetComponent<Interactable>());

    }

    public void Pickup()
    {

        // remove last target if exist:
        if (m_CurrentInteractable)
        {
            m_CurrentInteractable.readyAsTarget = false;
            m_CurrentInteractable.arrivedAtTarget = false;
        }

        // Get nearest
        m_CurrentInteractable = GetNearestInteractable();

        
        // Null check
        if (!m_CurrentInteractable)
            return;

        // Already held, check
        if (m_CurrentInteractable.m_ActiveHand)
        {
            m_CurrentInteractable.m_ActiveHand.Drop();
        }
        
        // Set active hand 
        m_CurrentInteractable.m_ActiveHand = this;

        // Set target flag to false:
        m_CurrentInteractable.readyAsTarget = false;
        m_CurrentInteractable.arrivedAtTarget = false;


    }

    public void Drop()
    {
        // Null check
        if (!m_CurrentInteractable)
            return;

        // link the fake cup to the real cup game obj
        m_CurrentInteractable.realCup = m_CurrentPushPlanner.cup_obj;

        // Set target flag to true:
        m_CurrentInteractable.readyAsTarget = true;

        // Set target to be the current marker:
        m_CurrentPushPlanner.m_CurrentInteractable = m_CurrentInteractable;
    }

    private Interactable GetNearestInteractable()
    {
        Interactable nearest = null;
        float minDistance = float.MaxValue;
        float distance = 0.0f;

        foreach(Interactable interactable in m_ContactInteractables)
        {
            distance = (interactable.transform.position - transform.position).sqrMagnitude;

            if(distance < minDistance)
            {
                minDistance = distance;
                nearest = interactable;
            }
        }

        return nearest;
    }

}
