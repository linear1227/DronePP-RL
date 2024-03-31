using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using PA_DronePack;
using UnityEngine;

namespace DroneA
{
    public class DroneAgentPath : Agent
    {
        public int NextCheckpointIndex { get; set; }

        [Header("TrainParameters")]
        [Tooltip("Steps to time out after a checkpoint")]
        public int stepTimeout = 2000;
        public float steps = 1000000;

        private PA_DroneController DroneMove;
        private DroneArea area;
        new private Rigidbody rigidbody;
        Vector3 droneInitPos;
        Quaternion droneInitRot;
        private TrailRenderer trail;

        private float nextStepTimeout;
        public override void Initialize()
        {
            area = GetComponentInParent<DroneArea>();
            rigidbody = GetComponent<Rigidbody>();
            trail = GetComponent<TrailRenderer>();

            // Maxstep 5000 default
            // MaxStep = 1000000;
            //droneInitPos = this.transform.localPosition;    //无人机初始位置
            //droneInitRot = this.transform.localRotation;	//无人机初始旋转
        }

        public override void OnEpisodeBegin()
        {
            DroneMove = GetComponent<PA_DroneController>();

            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;

            //gameObject.transform.localPosition = droneInitPos;
            //gameObject.transform.localRotation = droneInitRot;
            area.ResetAgentPosition(agent: this, randomize: true);

            nextStepTimeout = StepCount + stepTimeout;
        }

        /// <summary>
        /// Total observations = 3 + 3 + 3 = 9
        /// </summary>
        /// <param name="sensor"></param>
        public override void CollectObservations(VectorSensor sensor)
        {
            // Observe drone velovity(a vector3 = 3 value)
            sensor.AddObservation(transform.InverseTransformDirection(rigidbody.velocity));
            // Get the position of the next checkpoint(a vector3 = 3 value)
            sensor.AddObservation(VectorToNextCheckpoint());
            // Orientation of the next checkpoint(a vector3 = 3 value)
            Vector3 nextCheckpointForward = area.Checkpoints[NextCheckpointIndex].transform.forward;
            sensor.AddObservation(transform.InverseTransformDirection(nextCheckpointForward));
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            // DroneMove = GetComponent<PA_DroneController>();

            //Lift movement value
            var act0 = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
            //Boost movement value
            var act1 = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
            //Translation movement value
            var act2 = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f);
            //Trun movement value
            //var act3 = Mathf.Clamp(actions.ContinuousActions[3], -1f, 1f);


            DroneMove.LiftInput(act0);
            DroneMove.DriveInput(act1);
            //DroneMove.StrafeInput(act2);
            DroneMove.TurnInput(act2);

            AddReward(-1f / steps);

            if (StepCount > nextStepTimeout)
            {
                AddReward(-.5f);
                EndEpisode();
            }

            Vector3 localCheckpointDir = VectorToNextCheckpoint();
            if (localCheckpointDir.magnitude < Academy.Instance.EnvironmentParameters.GetWithDefault("checkpoint_radius", 0f))
            {
                GotCheckpoint();
            }
        }

        /// <summary>
        /// Called when the agent flies through the correct checkpoint
        /// </summary>
        private void GotCheckpoint()
        {
            NextCheckpointIndex = (NextCheckpointIndex + 1) % area.Checkpoints.Count;

            AddReward(.5f);
            nextStepTimeout = StepCount + stepTimeout;
        }

        /// <summary>
        /// Gets a vector from agent to the next checkpoint
        /// </summary>
        /// <returns>A local space vector</returns>
        private Vector3 VectorToNextCheckpoint()
        {
            Vector3 nextCheckpointDir = area.Checkpoints[NextCheckpointIndex].transform.position - transform.position;
            Vector3 localCheckpointDir = transform.InverseTransformDirection(nextCheckpointDir);
            return localCheckpointDir;
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;

            if (Input.GetKey(KeyCode.UpArrow)) continuousActionsOut[0] = 1;

            if (Input.GetKey(KeyCode.DownArrow)) continuousActionsOut[0] = -1;

            if (Input.GetKey(KeyCode.W)) continuousActionsOut[1] = 1;

            if (Input.GetKey(KeyCode.S)) continuousActionsOut[1] = -1;

            if (Input.GetKey(KeyCode.D)) continuousActionsOut[2] = 1;

            if (Input.GetKey(KeyCode.A)) continuousActionsOut[2] = -1;

            if (Input.GetKey(KeyCode.RightArrow)) continuousActionsOut[3] = 1;

            if (Input.GetKey(KeyCode.LeftArrow)) continuousActionsOut[3] = -1;
        }

        /// <summary>
        /// React to enter a checkpoint
        /// </summary>
        /// <param name="other">The checkpoint collider</param>
        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.CompareTag("checkpoint") &&
                other.gameObject == area.Checkpoints[NextCheckpointIndex])
            {
                GotCheckpoint();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Untagged"))
            {
                AddReward(-1f);
                EndEpisode();
            }
        }
    }
}