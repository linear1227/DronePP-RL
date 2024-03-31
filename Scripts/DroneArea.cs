using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DroneA
{
    public class DroneArea : MonoBehaviour
    {
        [Tooltip("Racing path")]
        public CinemachineSmoothPath path;

        [Tooltip("Checkpoint prefab")]
        public GameObject checkpointPrefab;

        [Tooltip("If true, enable training mode")]
        // public bool training;

        public List<DroneAgentPath> DroneAgents { get; private set; }
        public List<GameObject> Checkpoints { get; private set; }

        private void Awake()
        {
            // Find drone agents
            DroneAgents = transform.GetComponentsInChildren<DroneAgentPath>().ToList();
            Debug.Assert(DroneAgents.Count > 0, "No DroneAgentPath Found");
        }

        private void Start()
        {
            //Creat checkpoints along the race path
            Debug.Assert(path != null, "Race Path Was Not Set");
            Checkpoints = new List<GameObject>();
            int NumofCheckpoints = (int)path.MaxUnit(CinemachinePathBase.PositionUnits.PathUnits);

            for (int i = 0; i < NumofCheckpoints; i++)
            {
                // Instantiate either a checkpoint or finish line checkpoint
                GameObject checkpoint;
                checkpoint = Instantiate<GameObject>(checkpointPrefab);

                // Set the parent, position and rotation
                checkpoint.transform.SetParent(path.transform);
                checkpoint.transform.localPosition = path.m_Waypoints[i].position;
                checkpoint.transform.rotation = path.EvaluateOrientationAtUnit(i, CinemachinePathBase.PositionUnits.PathUnits);

                // Add the checkpoint to the list
                Checkpoints.Add(checkpoint);
            }
        }

        /// <summary>
        /// Reset the position of an agent using its current NextCheckpoint,
        /// unless randomize is true
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="randomize"></param>
        public void ResetAgentPosition(DroneAgentPath agent, bool randomize = false)
        {
            if (randomize)
            {
                agent.NextCheckpointIndex = Random.Range(0, Checkpoints.Count);
            }
            
            // Set start position to the previous checkpoint
            int previousCheckpointIndex = agent.NextCheckpointIndex - 1;
            if (previousCheckpointIndex == -1) previousCheckpointIndex = Checkpoints.Count - 1;

            float startPosition = path.FromPathNativeUnits(previousCheckpointIndex, CinemachinePathBase.PositionUnits.PathUnits);

            // Convert the position on the race path to a position in 3D space 
            Vector3 basePosition = path.EvaluatePosition(startPosition);

            // Get the orientation at current position on the race path
            Quaternion orientation = path.EvaluateOrientation(startPosition);

            // Set the position and rotation of a drone
            agent.transform.position = basePosition + orientation * Vector3.forward;
            agent.transform.rotation = orientation;
        }
    }
}