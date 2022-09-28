using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

/// <summary>
/// 
/// </summary>
public class RobotFollowerController : MonoBehaviour
{
    ROSConnection ros;
    // public Transform origin;
    public string topicName = "cmd_vel";
    public GameObject robot_tracker_obj;
    public GameObject object_tracker_obj ;


    // Publish the cube's position and rotation every N seconds
    public float publishMessageFrequency = 0.001f;

    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;
    private TwistMsg cmd_vel_msg  = new TwistMsg();


    public Vector3 robot_pos, obj_pos; 
    private float yaw_desired, robot_yaw; 
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
            // calculate dersired direction
            // robot_tran.transform.position = robot_tracker_obj.transform.position;
            // robot_tran.transform.rotation = robot_tracker_obj.transform.rotation;

            robot_pos = robot_tracker_obj.transform.position;
            obj_pos = object_tracker_obj.transform.position;
            robot_yaw = robot_tracker_obj.transform.localEulerAngles.y;

            // clamp angle between (-180, 180]
            if (robot_yaw > 180)
            {
                robot_yaw -=360;
            } 

            toVector = new Vector2 (obj_pos.x - robot_pos.x, obj_pos.z - robot_pos.z);
            
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
                return;
            }

            if (Mathf.Abs(yaw_desired- robot_yaw) > angle_error)
            {
                float min_speed_angular = 0.05f;

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
            }
             
            // Finally send the message to server_endpoint.py running in ROS
            ros.Publish(topicName, cmd_vel_msg);

            timeElapsed = 0;
        }
    }
}
