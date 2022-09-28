using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

public class RobotMover : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "cmd_vel";
    public GameObject robot_tracker_obj;
    
    // [HideInInspector]
    public Vector3 target_Position;
    // [HideInInspector]
    public bool target_set = false;
    // [HideInInspector]
    public bool target_reached = false;
    // [HideInInspector]
    public bool reverse = false;



    // Publish the cube's position and rotation every N seconds
    public float publishMessageFrequency = 0.001f;

    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;
    private TwistMsg cmd_vel_msg  = new TwistMsg();

    // [HideInInspector]
    public Vector3 robot_pos; 
    // [HideInInspector]
    public float yaw_desired, robot_yaw; 
    private Vector2 toVector;

    void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TwistMsg>(topicName);
    }

    private void Update()
    {

        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            if(target_set)
            {
                moveRobot();
                
            }
            timeElapsed = 0;
        }
    }

    private void moveRobot()
    {
            robot_pos = robot_tracker_obj.transform.position;
            robot_yaw = robot_tracker_obj.transform.localEulerAngles.y;

            // clamp angle between (-180, 180]
            if (robot_yaw > 180)
            {
                robot_yaw -=360;
            } 

            toVector = new Vector2 (target_Position.x - robot_pos.x, target_Position.z - robot_pos.z);
            if (reverse)
                toVector = (-1)*toVector;

            yaw_desired =  Mathf.Atan2(toVector.x, toVector.y) * Mathf.Rad2Deg;

            cmd_vel_msg  = new TwistMsg();
            float angle_error = 1.0f; 
            float pos_error = 0.02f;
            float speed_scaler = 0.5f;
            float angular_speed_scaler = 0.5f;

            // early exit if at desire location
            if (toVector.magnitude <= pos_error)
            {
                ros.Publish(topicName, cmd_vel_msg);
                target_reached = true;
                return;
            }

            target_reached = false;

            if (Mathf.Abs(yaw_desired- robot_yaw) > angle_error)
            {
                float min_speed_angular = 0.1f;

                cmd_vel_msg.angular.z = -(yaw_desired - robot_yaw)*Mathf.Deg2Rad*angular_speed_scaler;

                if (Mathf.Abs((float) cmd_vel_msg.angular.z) < min_speed_angular)
                    cmd_vel_msg.angular.z = Mathf.Sign((float) cmd_vel_msg.angular.z)*min_speed_angular;
                // Debug.Log("Desired yaw: " + yaw_desired);
                // Debug.Log("Current yaw: " + robot_yaw);


            }
            else if (toVector.magnitude > pos_error)
            {
                float min_speed_lin = 0.1f;
                cmd_vel_msg.linear.x = toVector.magnitude*speed_scaler;
                if (cmd_vel_msg.linear.x < min_speed_lin)
                    cmd_vel_msg.linear.x = min_speed_lin;
                
                if(reverse)
                    cmd_vel_msg.linear.x = (-1)*cmd_vel_msg.linear.x;
            }
             
            // Finally send the message to server_endpoint.py running in ROS
            ros.Publish(topicName, cmd_vel_msg);

    }
}
