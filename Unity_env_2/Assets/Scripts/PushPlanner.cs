using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushPlanner : MonoBehaviour
{

    public GameObject robot_target_controller = null;
    public GameObject cup_obj = null;
    public GameObject target_marker = null;

    private Vector3 robot_pos;
    private Vector3 cup_pos;
    private Vector3 target_pos;
    private Vector3 robot_prePush_pos;
    private Vector3 robot_Push_pos;
    private Vector3 robot_pullBack_pos;
    
    
    // [HideInInspector]
    public bool prePush_reached = false; 
    // [HideInInspector]
    public bool target_reached = false;
    // [HideInInspector]
    public bool pullback_reached = false;


    public float target_change_dist_tol = 0.02f;
    private bool target_init = false;
    
    [HideInInspector]
    public RobotMover m_CurrentRobotMover = null;

    public float prePush_offset = 0.1f; 
    public float Push_offset = 0.01f;
    public float PullBack_offset = 0.05f;

    [HideInInspector]
    public Interactable m_CurrentInteractable = null;

    // Start is called before the first frame update
    void Start()
    {
        // get robot controller to enable setting of target.
        m_CurrentRobotMover = robot_target_controller.GetComponent<RobotMover>();
        // m_CurrentInteractable = target_marker.GetComponent<Interactable>();
    }

    // Update is called once per frame
    void Update()
    { 
        if(!m_CurrentInteractable)
        {
            return;
        }

        // if target marker is not ready, don't moverobot
        if(!m_CurrentInteractable.readyAsTarget)
        {
            m_CurrentRobotMover.target_set = false;
            return;
        }

        robot_pos = m_CurrentRobotMover.robot_pos;
        cup_pos = cup_obj.transform.position;

        // initialise a value for target position if haven't already:
        if (!target_init)
        {
            target_pos = m_CurrentInteractable.transform.position;
            target_init = true;
            m_CurrentRobotMover.target_set = false;
            calcTargets();
        }

        // check if target position has changed significantly:
        float target_diff =  (m_CurrentInteractable.transform.position - target_pos).magnitude;

        if (target_diff > target_change_dist_tol)
        {   
            prePush_reached = false;
            target_pos = m_CurrentInteractable.transform.position;
            m_CurrentRobotMover.target_set = false;
            calcTargets();
        }
        
        // go to pre push position:
        if(!prePush_reached)
        {
            goToPrePush();
        }
        // push to target position: 
        else if(!target_reached)
        {
            pushToTarget();
        }
        // go to pull back position:
        else if(!pullback_reached)
        {
            pullbackFromTarget();
        }
        // else
        // {
        //     m_CurrentRobotMover.target_set = false;
        //     m_CurrentRobotMover.target_reached = false;
        //     m_CurrentRobotMover.reverse = false;

        // }
        
    }

    void calcTargets()
    {
        // calculate Target
        Vector3 toVect;

        toVect  =  cup_pos - target_pos;

        Vector2 toVect2; 
        toVect2 = new Vector2 (toVect.x, toVect.z);
        toVect2 *= prePush_offset/toVect2.magnitude;

        robot_prePush_pos= cup_pos + new Vector3(toVect2.x, 0, toVect2.y);

        toVect2 *= Push_offset/toVect2.magnitude;

        robot_Push_pos = target_pos + new Vector3(toVect2.x, 0, toVect2.y);

        resetMovementFlags();
    }

    void resetMovementFlags()
    {
        prePush_reached = false;
        target_reached = false;
        pullback_reached = false;
        m_CurrentInteractable.arrivedAtTarget = false;
    }

    void goToPrePush()
    {   
        if (!m_CurrentRobotMover.target_set)
        {
            m_CurrentRobotMover.target_Position = robot_prePush_pos;
            m_CurrentRobotMover.target_reached = false;
            m_CurrentRobotMover.reverse = false;
            m_CurrentRobotMover.target_set = true;
        }

        if(m_CurrentRobotMover.target_reached)
        {
            prePush_reached = true;
            m_CurrentRobotMover.target_set = false;
        }
    }

    void pushToTarget()
    {
        if (!m_CurrentRobotMover.target_set)
        {
            m_CurrentRobotMover.target_Position = robot_Push_pos;
            m_CurrentRobotMover.target_reached = false;
            m_CurrentRobotMover.reverse =false;
            m_CurrentRobotMover.target_set = true;
        }

        if(m_CurrentRobotMover.target_reached)
        {
            target_reached = true;
            m_CurrentInteractable.arrivedAtTarget = true;
            m_CurrentRobotMover.target_set = false;
        }


    }
    void pullbackFromTarget()
    {
        if (!m_CurrentRobotMover.target_set)
        {
            // calculate vector where the robot is facing:
            Vector2 toVect2; 
            float robot_yaw = m_CurrentRobotMover.robot_yaw*Mathf.Deg2Rad;
            
            toVect2 = new Vector2(Mathf.Sin(robot_yaw), Mathf.Cos(robot_yaw));
            // toVect2 = new Vector2(Mathf.Cos(robot_yaw), Mathf.Sin(robot_yaw));                        
            toVect2 *= PullBack_offset/toVect2.magnitude;

            // set target to be behind where the robot is facing:
            robot_pullBack_pos = m_CurrentRobotMover.robot_pos - new Vector3(toVect2.x, 0, toVect2.y);

            m_CurrentRobotMover.target_Position = robot_pullBack_pos;
            m_CurrentRobotMover.target_reached = false;
            m_CurrentRobotMover.reverse = true;
            m_CurrentRobotMover.target_set = true;

        }

        if(m_CurrentRobotMover.target_reached)
        {
            pullback_reached = true;
            m_CurrentRobotMover.target_set = false;
            m_CurrentRobotMover.reverse = false;
            m_CurrentRobotMover.target_reached = false;
        }

    }
    
}
