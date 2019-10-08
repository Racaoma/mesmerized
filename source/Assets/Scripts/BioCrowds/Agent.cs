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
    public class Agent : MonoBehaviour
    {
        //New Vars
        public Vector3 agentOffset;

        protected const float UPDATE_NAVMESH_INTERVAL = 1.0f;

        //agent radius
        public float agentRadius;
        //agent speed
        public Vector3 _velocity;
        //max speed
        [SerializeField]
        protected float _maxSpeed = 1.5f;

        //goal
        private PressurePlate Goal;
        private bool _isLeavingLevel = false;

        //list with all auxins in his personal space
        protected List<Auxin> _auxins = new List<Auxin>();
        public List<Auxin> Auxins
        {
            get { return _auxins; }
            set { _auxins = value; }
        }

        //agent cell
        protected Cell _currentCell;
        public Cell CurrentCell
        {
            get { return _currentCell; }
            set { _currentCell = value; }
        }

        protected World _world;
        public World World
        {
            get { return _world; }
            set { _world = value; }
        }

        protected int _totalX;
        protected int _totalZ;

        protected NavMeshPath _navMeshPath;

        //time elapsed (to calculate path just between an interval of time)
        protected float _elapsedTime;
        //auxins distance vector from agent
        public List<Vector3> _distAuxin;

        /*-----------Paravisis' model-----------*/
        protected bool _isDenW = false; //  avoid recalculation
        protected float _denW;    //  avoid recalculation
        protected Vector3 _rotation; //orientation vector (movement)
        protected Vector3 _goalPosition; //goal position
        protected Vector3 _dirAgentGoal; //diff between goal and agent

        //Chance to stop
        public float baseChanceToStop = 0.5f;
        public float extraChanceToStop = 0f;

        private const float _extraChanceToStopAfterNotStopping = 0.5f;
        private const float _distanceToStopMovementAnimation = 0.005f;

        //Patterns
        public bool dogFear = false;
        public bool attractedToComputer = false;
        public bool attractedToMachine = false;
        public bool attractedToLamp = false;
        public bool fearRobot = false;
        public float speedMultiplier = 1f;
        public float maxDelayToWalk = 2f;
        private float _rangeRandomGoal = 1f;

        //Other Variables
        private Animator _animator;
        private Vector3 _lastPosition;

        protected virtual void Start()
        {
            _animator = GetComponentInChildren<Animator>();
            _navMeshPath = new NavMeshPath();
            _lastPosition = this.transform.position;

            //cache world info
            _totalX = Mathf.FloorToInt(_world.Dimension.x / 2.0f) - 1;
            _totalZ = Mathf.FloorToInt(_world.Dimension.y / 2.0f);
        }

        private void OnDestroy()
        {
            if(World.Instance != null)
            {
                World.Instance.RemoveAgentFromList(this);
            }
        }

        public void CheckStop(PressurePlate pressurePlate)
        {
            if(pressurePlate == Goal)
            {
                Pedestal.PressurePlateActiveItemEnum activeItem = pressurePlate.GetPressurePlateActiveItem();
                if (activeItem == Pedestal.PressurePlateActiveItemEnum.Dog && dogFear)
                {
                    SetNextGoal(World.Instance.GetNextPressurePlate(Goal));
                    extraChanceToStop += 1f - baseChanceToStop;
                }
                else if(activeItem == Pedestal.PressurePlateActiveItemEnum.Lamp && attractedToLamp)
                {
                    return;
                }
                else if (activeItem == Pedestal.PressurePlateActiveItemEnum.Machine && attractedToMachine)
                {
                    return;
                }
                else if (activeItem == Pedestal.PressurePlateActiveItemEnum.Robot && fearRobot)
                {
                    SetNextGoal(World.Instance.GetNextPressurePlate(Goal));
                    extraChanceToStop += 1f - baseChanceToStop;
                }

                if (Random.Range(extraChanceToStop, 1f) <= baseChanceToStop)
                {
                    SetNextGoal(World.Instance.GetNextPressurePlate(Goal));
                    extraChanceToStop += _extraChanceToStopAfterNotStopping;
                }
            }
        }

        public void SetNextGoal(PressurePlate pressurePlate)
        {
            _isLeavingLevel = false;
            StartCoroutine(ChangeGoalCoroutine(pressurePlate));
        }

        public void SetExitGoal(Vector3 position)
        {
            Goal.OnDoorClose += CancelExitGoal;
            _isLeavingLevel = true;
            _goalPosition = position;
        }

        private void CancelExitGoal()
        {
            Goal.OnDoorClose -= CancelExitGoal;
            _isLeavingLevel = false;
            _goalPosition = Goal.GetRandomPositionInside(_rangeRandomGoal);
        }

        public PressurePlate GetCurrentPressurePlate()
        {
            return Goal;
        }

        private IEnumerator ChangeGoalCoroutine(PressurePlate pressurePlate)
        {
            float timer = Random.Range(0f, maxDelayToWalk);
            while(timer > 0f)
            {
                timer -= Time.deltaTime;
                yield return null;
            }

            Goal = pressurePlate;
            _goalPosition = Goal.GetRandomPositionInside(_rangeRandomGoal) + agentOffset;
            _dirAgentGoal = this.transform.position - _goalPosition;
        }

        protected virtual void Update()
        {
            //clear agent´s information
            ClearAgent();

            if (Vector3.Distance(this.transform.position, _lastPosition) > _distanceToStopMovementAnimation)
            {
                _animator.SetBool("isWalking", true);
            }
            else
            {
                _animator.SetBool("isWalking", false);
            }

            _lastPosition = this.transform.position;
            this.transform.LookAt(_goalPosition);

            // Update the way to the goal every second.
            _elapsedTime += Time.deltaTime;

            if (_elapsedTime > UPDATE_NAVMESH_INTERVAL)
            {
                _elapsedTime = 0.0f;

                //calculate agent path
               bool foundPath = NavMesh.CalculatePath(transform.position, _goalPosition, NavMesh.AllAreas, _navMeshPath);

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

            //Check Distance to Goal
            if(_isLeavingLevel && Vector3.Distance(this.transform.position, _goalPosition) <= 0.2f)
            {
                Goal.OnDoorClose -= CancelExitGoal;
                Destroy(this.gameObject);
            }
        }

        //clear agent´s informations
        protected void ClearAgent()
        {
            //re-set inicial values
            _denW = 0;
            _distAuxin.Clear();
            _isDenW = false;
            _rotation = new Vector3(0f, 0f, 0f);
            _dirAgentGoal = _goalPosition - transform.position;
        }

        //walk
        public void Step()
        {
            if (_velocity.sqrMagnitude > 0.0f)
                transform.Translate(_velocity * Time.deltaTime, Space.World);
        }

        //The calculation formula starts here
        //the ideia is to find m=SUM[k=1 to n](Wk*Dk)
        //where k iterates between 1 and n (number of auxins), Dk is the vector to the k auxin and Wk is the weight of k auxin
        //the weight (Wk) is based on the degree resulting between the goal vector and the auxin vector (Dk), and the
        //distance of the auxin from the agent
        public void CalculateDirection()
        {
            //for each agent´s auxin
            for (int k = 0; k < _distAuxin.Count; k++)
            {
                //calculate W
                float valorW = CalculaW(k);
                if (_denW < 0.0001f)
                    valorW = 0.0f;

                //sum the resulting vector * weight (Wk*Dk)
                _rotation += valorW * _distAuxin[k] * _maxSpeed;
            }
        }

        //calculate W
        protected float CalculaW(int indiceRelacao)
        {
            //calculate F (F is part of weight formula)
            float fVal = GetF(indiceRelacao);

            if (!_isDenW)
            {
                _denW = 0f;

                //for each agent´s auxin
                for (int k = 0; k < _distAuxin.Count; k++)
                {
                    //calculate F for this k index, and sum up
                    _denW += GetF(k);
                }
                _isDenW = true;
            }

            return fVal / _denW;
        }

        //calculate F (F is part of weight formula)
        protected float GetF(int pRelationIndex)
        {
            //distance between auxin´s distance and origin 
            float Ymodule = Vector3.Distance(_distAuxin[pRelationIndex], Vector3.zero);
            //distance between goal vector and origin
            float Xmodule = _dirAgentGoal.normalized.magnitude;

            float dot = Vector3.Dot(_distAuxin[pRelationIndex], _dirAgentGoal.normalized);

            if (Ymodule < 0.00001f)
                return 0.0f;

            //return the formula, defined in thesis
            return (float)((1.0 / (1.0 + Ymodule)) * (1.0 + ((dot) / (Xmodule * Ymodule))));
        }

        //calculate speed vector    
        public void CalculateVelocity()
        {
            //distance between movement vector and origin
            float moduleM = Vector3.Distance(_rotation, Vector3.zero);

            //multiply for PI
            float s = moduleM * Mathf.PI;

            //if it is bigger than maxSpeed, use maxSpeed instead
            if (s > _maxSpeed)
                s = _maxSpeed;

            //Debug.Log("vetor M: " + m + " -- modulo M: " + s);
            if (moduleM > 0.0001f)
            {
                //calculate speed vector
                _velocity = s * (_rotation / moduleM);
            }
            else
            {
                //else, go idle
                _velocity = Vector3.zero;
            }
        }

        //find all auxins near him (Voronoi Diagram)
        //call this method from game controller, to make it sequential for each agent
        public void FindNearAuxins()
        {
            //clear them all, for obvious reasons
            _auxins.Clear();

            //get all auxins on my cell
            List<Auxin> cellAuxins = _currentCell.Auxins;

            //iterate all cell auxins to check distance between auxins and agent
            for (int i = 0; i < cellAuxins.Count; i++)
            {
                //see if the distance between this agent and this auxin is smaller than the actual value, and inside agent radius
                float distanceSqr = (transform.position - cellAuxins[i].Position).sqrMagnitude;
                if (distanceSqr < cellAuxins[i].MinDistance && distanceSqr <= agentRadius * agentRadius)
                {
                    //take the auxin!
                    //if this auxin already was taken, need to remove it from the agent who had it
                    if (cellAuxins[i].IsTaken)
                        cellAuxins[i].Agent.Auxins.Remove(cellAuxins[i]);

                    //auxin is taken
                    cellAuxins[i].IsTaken = true;

                    //auxin has agent
                    cellAuxins[i].Agent = this;
                    //update min distance
                    cellAuxins[i].MinDistance = distanceSqr;
                    //update my auxins
                    _auxins.Add(cellAuxins[i]);
                }
            }

            FindCell();
        }

        protected void FindCell()
        {
            //distance from agent to cell, to define agent new cell
            float distanceToCellSqr = (transform.position - _currentCell.transform.position).sqrMagnitude; //Vector3.Distance(transform.position, _currentCell.transform.position);

            //cap the limits
            //[ ][ ][ ]
            //[ ][X][ ]
            //[ ][ ][ ]
            if (_currentCell.X > 0)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X - 1) * _totalZ + (_currentCell.Z + 0)]);

            if (_currentCell.X > 0 && _currentCell.Z < _totalZ - 1)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X - 1) * _totalZ + (_currentCell.Z + 1)]);

            if (_currentCell.X > 0 && _currentCell.Z > 0)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X - 1) * _totalZ + (_currentCell.Z - 1)]);

            if (_currentCell.Z < _totalZ - 1)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X + 0) * _totalZ + (_currentCell.Z + 1)]);

            if (_currentCell.X < _totalX && _currentCell.Z < _totalZ - 1)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X + 1) * _totalZ + (_currentCell.Z + 1)]);

            if (_currentCell.X < _totalX)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X + 1) * _totalZ + (_currentCell.Z + 0)]);

            if (_currentCell.X < _totalX && _currentCell.Z > 0)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X + 1) * _totalZ + (_currentCell.Z - 1)]);

            if (_currentCell.Z > 0)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X + 0) * _totalZ + (_currentCell.Z - 1)]);

        }

        protected void CheckAuxins(ref float pDistToCellSqr, Cell pCell)
        {
            //get all auxins on neighbourcell
            List<Auxin> cellAuxins = pCell.Auxins;

            //iterate all cell auxins to check distance between auxins and agent
            for (int c = 0; c < cellAuxins.Count; c++)
            {
                //see if the distance between this agent and this auxin is smaller than the actual value, and smaller than agent radius
                float distanceSqr = (transform.position - cellAuxins[c].Position).sqrMagnitude;
                if (distanceSqr < cellAuxins[c].MinDistance && distanceSqr <= agentRadius * agentRadius)
                {
                    //take the auxin
                    //if this auxin already was taken, need to remove it from the agent who had it
                    if (cellAuxins[c].IsTaken)
                        cellAuxins[c].Agent.Auxins.Remove(cellAuxins[c]);

                    //auxin is taken
                    cellAuxins[c].IsTaken = true;
                    //auxin has agent
                    cellAuxins[c].Agent = this;
                    //update min distance
                    cellAuxins[c].MinDistance = distanceSqr;
                    //update my auxins
                    _auxins.Add(cellAuxins[c]);
                }
            }

            //see distance to this cell
            float distanceToNeighbourCell = (transform.position - pCell.transform.position).sqrMagnitude; 
            if (distanceToNeighbourCell < pDistToCellSqr)
            {
                pDistToCellSqr = distanceToNeighbourCell;
                _currentCell = pCell;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(_goalPosition, 0.1f);
        }
    }
}