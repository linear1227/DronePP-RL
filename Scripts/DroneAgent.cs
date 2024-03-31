using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

namespace PA_DronePack
{
	public class DroneAgent : Agent
	{
		private PA_DroneController dcoScript;
		public GameObject goal;
		Vector3 droneInitPos;
		Quaternion droneInitRot;
		float preDist, curDist, tempDist;
		private new Rigidbody rigidbody;
		// private Vector3 initPos = new Vector3(0, 1.5f ,0);

		private void Awake()
        {
			rigidbody = this.GetComponent<Rigidbody>();
			droneInitPos = this.transform.localPosition;	//无人机初始位置
			droneInitRot = this.transform.localRotation;	//无人机初始旋转
		}
		
		public override void OnEpisodeBegin()
		{
			dcoScript = GetComponent<PA_DroneController>();
			// droneInitPos = gameObject.transform.localPosition;	//无人机恢复到初始位置
			// droneInitRot = gameObject.transform.localRotation;	//无人机恢复到初始旋转角度
			gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
			gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			// goal.transform.localPosition = new Vector3(Random.Range(-5f, 5f), Random.Range(0f, 7f), Random.Range(-5f, 5f));
			gameObject.transform.localPosition = droneInitPos;
			gameObject.transform.localRotation = droneInitRot;
			preDist = (goal.transform.position - gameObject.transform.position).magnitude;
			// preDist = (goal.transform.localPosition - gameObject.transform.localPosition).magnitude;
		}

        public override void CollectObservations(VectorSensor sensor)
        {
            // sensor.AddObservation(gameObject.transform.localPosition - goal.transform.localPosition); //3
            sensor.AddObservation(goal.transform.position - gameObject.transform.position);
            // sensor.AddObservation(goal.transform.localPosition); //3
            sensor.AddObservation(this.rigidbody.velocity); //3
            sensor.AddObservation(this.rigidbody.angularVelocity); //3
        }

        public override void OnActionReceived(ActionBuffers actions)
		{
			// old version
			// var act0 = Mathf.Clamp(vectorAction[0], -1f, 1f);
			// var act1 = Mathf.Clamp(vectorAction[1], -1f, 1f);
			// var act2 = Mathf.Clamp(vectorAction[2], -1f, 1f);

			var act0 = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
            var act1 = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
			var act2 = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f);
			var act3 = Mathf.Clamp(actions.ContinuousActions[3], -1f, 1f);

			dcoScript.LiftInput(act0);
			dcoScript.DriveInput(act1);
			dcoScript.StrafeInput(act2);
			dcoScript.TurnInput(act2);

			float DistoGoal = (goal.transform.position - gameObject.transform.position).magnitude;

            // if((goal.transform.position - gameObject.transform.position).magnitude < 1f)
            if (DistoGoal < 1f)
			{
				AddReward(10f);
				EndEpisode();
			}
			//else if ((goal.transform.position - gameObject.transform.position).magnitude > 10f)
			//else if (this.transform.localPosition.y > 1.5f)
			//{
			//	SetReward(-2f);
			//	EndEpisode();
			//}
            else
            {
                curDist = DistoGoal;
				//float reward = (preDist - curDist) * 1.5f;
				if((preDist - curDist) > 0f)
                {
					if((preDist - curDist) > 1f)
                    {
						float ExReward = (preDist - curDist) * 0.5f + 1f;
						AddReward(ExReward);
                    }
                    else
                    {
						float reward = (preDist - curDist) * 0.5f;
						AddReward(reward);
					}
                }
				else
                {
					float reward = (preDist - curDist) * 0.5f - 0.2f;
					AddReward(reward);
				}
                // AddReward(0.5f);
                preDist = curDist;
            }
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

        public void OnCollisionEnter(Collision other)
		{
			// collide with building
			if (other.gameObject.CompareTag("obstacle"))
			{
				AddReward(-10f);
				EndEpisode();
			}
		} 
	}
}