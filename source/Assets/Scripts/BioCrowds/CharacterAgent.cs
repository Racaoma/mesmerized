/// ---------------------------------------------
/// Contact: Henry Braun
/// Brief: Defines an Agent
/// Thanks to VHLab for original implementation
/// Date: November 2017 
/// ---------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

namespace Biocrowds.Core
{
    public class CharacterAgent : Agent
    {
        public Vector3 GoalPosition;

        protected override void Start()
        {
            _world = World.Instance;
            GoalPosition = this.transform.position;
            base.Start();
        }

        protected override void Update()
        {
            this.transform.LookAt(GoalPosition);

            //clear agent´s information
            ClearAgent();
            
            // Update the way to the goal every second.
            _elapsedTime += Time.deltaTime;

            if (_elapsedTime > UPDATE_NAVMESH_INTERVAL)
            {
                _elapsedTime = 0.0f;

                //calculate agent path
               bool foundPath = NavMesh.CalculatePath(transform.position, GoalPosition, NavMesh.AllAreas, _navMeshPath);

                //update its goal if path is found
                if (foundPath)
                {
                    _goalPosition = new Vector3(_navMeshPath.corners[1].x, 0f, _navMeshPath.corners[1].z);
                    _dirAgentGoal = _goalPosition - transform.position;
                }
            }

            //draw line to goal
            for (int i = 0; i < _navMeshPath.corners.Length - 1; i++)
                Debug.DrawLine(_navMeshPath.corners[i], _navMeshPath.corners[i + 1], Color.red);
        }
    }
}