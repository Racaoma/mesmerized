/// ---------------------------------------------
/// Contact: Henry Braun
/// Brief: Defines the world environment
/// Thanks to VHLab for original implementation
/// Date: November 2017 
/// ---------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using UnityEditor.AI;
using System.Collections;
using UnityEngine.AI;

namespace Biocrowds.Core
{
    public class World : ProperSingleton<World>
    {
        public enum MovementFlowEnum
        {
            None,
            Clockwise,
            Counterclockwise
        };

        //New Vars
        public PressurePlate[] pressurePlates;
        public float roundTime = 20f;
        private float currentRoundTimer;
        private MovementFlowEnum movementFlow = MovementFlowEnum.Clockwise;

        //agent radius
        private const float AGENT_RADIUS = 1.0f;

        //radius for auxin collide
        private const float AUXIN_RADIUS = 0.1f;

        //density
        private const float AUXIN_DENSITY = 0.45f; //0.65f;

        [SerializeField]
        private Terrain _terrain;

        [SerializeField]
        private Vector2 _dimension = new Vector2(30.0f, 20.0f);
        public Vector2 Dimension
        {
            get { return _dimension; }
        }

        //number of agents in the scene
        [SerializeField]
        private int _maxAgents_Security = 6;
        [SerializeField]
        private int _maxAgents_Doctor = 6;
        [SerializeField]
        private int _maxAgents_Operator = 6;
        [SerializeField]
        private int _maxAgents_Deliveryman = 6;

        //Behaviors
        private const float _timeSpentNearDogThreshold = 3f;
        private const float _timeToCompleteAllLevelsThreshold = 2000f;
        private const float _timeSpentNearClosedDoorThreshold = 5f;
        private const int _parLeverUsesThreshold = 5;
        private const int _parButtonUsesThreshold = 8;
        private const int _machineExplosionsTriggeredThreshold = 5;

        //agent prefab
        [SerializeField]
        private Agent _agentPrefab_Security;
        [SerializeField]
        private Agent _agentPrefab_Doctor;
        [SerializeField]
        private Agent _agentPrefab_Operator;
        [SerializeField]
        private Agent _agentPrefab_Deliveryman;

        [SerializeField]
        private Cell _cellPrefab;

        [SerializeField]
        private Auxin _auxinPrefab;

        List<Agent> _agents = new List<Agent>();
        List<Cell> _cells = new List<Cell>();

        public List<Cell> Cells
        {
            get { return _cells; }
        }

        //max auxins on the ground
        private int _maxAuxins;
        private bool _isReady;

        [SerializeField] private GameObject[] _flowArrows;
        private const float _speedArrow = 100f;

        // Use this for initialization
        private void Start()
        {
            currentRoundTimer = roundTime;

            //Application.runInBackground = true;

            //change terrain size according informed
            _terrain.terrainData.size = new Vector3(_dimension.x, _terrain.terrainData.size.y, _dimension.y);

            //create all cells based on dimension
            CreateCells();

            //populate cells with auxins
            DartThrowing();

            //create our agents
            CreateAgents();

            //build the navmesh at runtime
            //NavMeshBuilder.BuildNavMesh();

            //wait a little bit to start moving
            _isReady = true;
        }

        public void InvertFlow()
        {
            if(movementFlow == MovementFlowEnum.Clockwise)
            {
                movementFlow = MovementFlowEnum.Counterclockwise;
            }
            else
            {
                movementFlow = MovementFlowEnum.Clockwise;
            }
        }

        private void CreateCells()
        {
            Transform cellPool = new GameObject("Cells").transform;

            for (int i = 0; i < _dimension.x / 2; i++) //i + agentRadius * 2
            {
                for (int j = 0; j < _dimension.y / 2; j++) // j + agentRadius * 2
                {
                    //instantiante a new cell
                    Cell newCell = Instantiate(_cellPrefab, new Vector3(1.0f + (i * 2.0f), 0.0f, 1.0f + (j * 2.0f)), Quaternion.Euler(90.0f, 0.0f, 0.0f), cellPool);

                    //change its name
                    newCell.name = "Cell [" + i + "][" + j + "]";

                    //metadata for optimization
                    newCell.X = i;
                    newCell.Z = j;

                    _cells.Add(newCell);
                }
            }
        }

        private void DartThrowing()
        {
            //lets set the qntAuxins for each cell according the density estimation
            float densityToQnt = AUXIN_DENSITY;

            Transform auxinPool = new GameObject("Auxins").transform;

            densityToQnt *= 2f / (2.0f * AUXIN_RADIUS);
            densityToQnt *= 2f / (2.0f * AUXIN_RADIUS);

            _maxAuxins = (int)Mathf.Floor(densityToQnt);

            //for each cell, we generate its auxins
            for (int c = 0; c < _cells.Count; c++)
            {
                //Dart throwing auxins
                //use this flag to break the loop if it is taking too long (maybe there is no more space)
                int flag = 0;
                for (int i = 0; i < _maxAuxins; i++)
                {
                    float x = Random.Range(_cells[c].transform.position.x - 0.99f, _cells[c].transform.position.x + 0.99f);
                    float z = Random.Range(_cells[c].transform.position.z - 0.99f, _cells[c].transform.position.z + 0.99f);

                    //see if there are auxins in this radius. if not, instantiante
                    List<Auxin> allAuxinsInCell = _cells[c].Auxins;
                    bool createAuxin = true;
                    for (int j = 0; j < allAuxinsInCell.Count; j++)
                    {
                        float distanceAASqr = (new Vector3(x, 0f, z) - allAuxinsInCell[j].Position).sqrMagnitude;

                        //if it is too near no need to add another
                        if (distanceAASqr < AUXIN_RADIUS * AUXIN_RADIUS)
                        {
                            createAuxin = false;
                            break;
                        }
                    }

                    //if i have found no auxin, i still need to check if is there obstacles on the way
                    if (createAuxin)
                    {
                        //sphere collider to try to find the obstacles
                        //NavMeshHit hit;
                        //createAuxin = NavMesh.Raycast(new Vector3(x, 2f, z), new Vector3(x, -2f, z), out hit, 1 << NavMesh.GetAreaFromName("Walkable")); //NavMesh.GetAreaFromName("Walkable")); // NavMesh.AllAreas);
                        //createAuxin = NavMesh.SamplePosition(new Vector3(x, 0.0f, z), out hit, 0.1f, 1 << NavMesh.GetAreaFromName("Walkable"));
                        //bool isBlocked = _obstacleCollider.bounds.Contains(new Vector3(x, 0.0f, z));
                        Collider[] hitColliders = Physics.OverlapSphere(new Vector3(x, 0f, z), AUXIN_RADIUS + 0.1f, 1 << LayerMask.NameToLayer("Obstacle"));
                        createAuxin = (hitColliders.Length == 0);
                    }

                    //check if auxin can be created there
                    if (createAuxin)
                    {
                        Auxin newAuxin = Instantiate(_auxinPrefab, new Vector3(x, 0.0f, z), Quaternion.identity, auxinPool);

                        //change its name
                        newAuxin.name = "Auxin [" + c + "][" + i + "]";
                        //this auxin is from this cell
                        newAuxin.Cell = _cells[c];
                        //set position
                        newAuxin.Position = new Vector3(x, 0f, z);

                        //add this auxin to this cell
                        _cells[c].Auxins.Add(newAuxin);

                        //reset the flag
                        flag = 0;
                    }
                    else
                    {
                        //else, try again
                        flag++;
                        i--;
                    }

                    //if flag is above qntAuxins (*2 to have some more), break;
                    if (flag > _maxAuxins * 2)
                    {
                        //reset the flag
                        flag = 0;
                        break;
                    }
                }
            }
        }

        private void CreateAgents()
        {
            Transform agentPool = new GameObject("Agents").transform;

            //Instantiate Agents
            for (int i = 0; i < _maxAgents_Deliveryman; i++)
            {
                int pressurePlateIndex = Random.Range(0, pressurePlates.Length);
                Agent newAgent = Instantiate(_agentPrefab_Deliveryman, pressurePlates[pressurePlateIndex].GetRandomPositionInside(1f), Quaternion.identity, agentPool);
                newAgent.transform.position += newAgent.agentOffset;

                newAgent.dogFear = true;
                newAgent.attractedToComputer = false;
                newAgent.attractedToComputer = false;
                newAgent.attractedToMachine = false;
                newAgent.attractedToLamp = false;
                newAgent.speedMultiplier = 1f;
                newAgent.baseChanceToStop = 0.5f;
                newAgent.maxDelayToWalk = 2f;

                newAgent.name = "Deliveryman Agent [" + i + "]";  //name
                newAgent.CurrentCell = _cells[i];  //agent cell
                newAgent.agentRadius = AGENT_RADIUS;  //agent radius

                newAgent.SetNextGoal(pressurePlates[pressurePlateIndex]);  //agent goal
                newAgent.World = this;

                _agents.Add(newAgent);
            }

            for (int i = 0; i < _maxAgents_Security; i++)
            {
                int pressurePlateIndex = Random.Range(0, pressurePlates.Length);
                Agent newAgent = Instantiate(_agentPrefab_Security, pressurePlates[pressurePlateIndex].GetRandomPositionInside(1f), Quaternion.identity, agentPool);
                newAgent.transform.position += newAgent.agentOffset;

                newAgent.dogFear = false;
                newAgent.attractedToComputer = false;
                newAgent.attractedToComputer = false;
                newAgent.attractedToMachine = false;
                newAgent.attractedToLamp = true;
                newAgent.speedMultiplier = 1f;
                newAgent.baseChanceToStop = 0.5f;
                newAgent.maxDelayToWalk = 2f;

                newAgent.name = "Security Agent [" + i + "]";  //name
                newAgent.CurrentCell = _cells[i];  //agent cell
                newAgent.agentRadius = AGENT_RADIUS;  //agent radius

                newAgent.SetNextGoal(pressurePlates[pressurePlateIndex]);  //agent goal
                newAgent.World = this;

                _agents.Add(newAgent);
            }

            for (int i = 0; i < _maxAgents_Operator; i++)
            {
                int pressurePlateIndex = Random.Range(0, pressurePlates.Length);
                Agent newAgent = Instantiate(_agentPrefab_Operator, pressurePlates[pressurePlateIndex].GetRandomPositionInside(1f), Quaternion.identity, agentPool);
                newAgent.transform.position += newAgent.agentOffset;

                newAgent.dogFear = false;
                newAgent.attractedToComputer = false;
                newAgent.attractedToComputer = true;
                newAgent.attractedToMachine = true;
                newAgent.attractedToLamp = false;
                newAgent.speedMultiplier = 1f;
                newAgent.baseChanceToStop = 0.5f;
                newAgent.maxDelayToWalk = 2f;

                newAgent.name = "Operator Agent [" + i + "]";  //name
                newAgent.CurrentCell = _cells[i];  //agent cell
                newAgent.agentRadius = AGENT_RADIUS;  //agent radius

                newAgent.SetNextGoal(pressurePlates[pressurePlateIndex]);  //agent goal
                newAgent.World = this;

                _agents.Add(newAgent);
            }

            for (int i = 0; i < _maxAgents_Doctor; i++)
            {
                int pressurePlateIndex = Random.Range(0, pressurePlates.Length);
                Agent newAgent = Instantiate(_agentPrefab_Doctor, pressurePlates[pressurePlateIndex].GetRandomPositionInside(1f), Quaternion.identity, agentPool);
                newAgent.transform.position += newAgent.agentOffset;

                SetDoctorBehaviors(ref newAgent);

                newAgent.name = "Doctor Agent [" + i + "]";  //name
                newAgent.CurrentCell = _cells[i];  //agent cell
                newAgent.agentRadius = AGENT_RADIUS;  //agent radius

                newAgent.SetNextGoal(pressurePlates[pressurePlateIndex]);  //agent goal
                newAgent.World = this;

                _agents.Add(newAgent);
            }
        }

        public void RemoveAgentFromList(Agent agentDestroyed)
        {
            _agents.Remove(agentDestroyed);
        }

        private void SetDoctorBehaviors(ref Agent newAgent)
        {
            if (SaveManager.currentProgress.timeSpentNearDog <= _timeSpentNearDogThreshold)
            {
                newAgent.dogFear = false;
            }
            else
            {
                newAgent.dogFear = true;
            }

            if (SaveManager.currentProgress.timeToCompleteAllLevels <= _timeToCompleteAllLevelsThreshold)
            {
                newAgent.speedMultiplier = 2f;
            }
            else if(SaveManager.currentProgress.timeToCompleteAllLevels <= _timeToCompleteAllLevelsThreshold * 2f)
            {
                newAgent.speedMultiplier = 1f;
            }
            else
            {
                newAgent.speedMultiplier = 0.5f;
            }

            if (SaveManager.currentProgress.timeSpentNearClosedDoor <= _timeSpentNearClosedDoorThreshold)
            {
                newAgent.baseChanceToStop = 0.5f;
            }
            else
            {
                newAgent.baseChanceToStop = 0.65f;
            }

            if(SaveManager.currentProgress.gatesPuzzleLeverUses <= _parLeverUsesThreshold)
            {
                newAgent.maxDelayToWalk = 2f;
            }
            else
            {
                newAgent.maxDelayToWalk = 1f;
            }

            if (SaveManager.currentProgress.buttonPuzzleButtonUses <= _parButtonUsesThreshold)
            {
                newAgent.attractedToComputer = true;
                newAgent.fearComputer = false;
            }
            else
            {
                newAgent.attractedToComputer = false;
                newAgent.fearComputer = false;
            }

            if (SaveManager.currentProgress.machineExplosionsTriggered <= _machineExplosionsTriggeredThreshold)
            {
                newAgent.attractedToMachine = true;
            }
            else
            {
                newAgent.attractedToMachine = false;
            }

            newAgent.attractedToLamp = false;
        }

        public PressurePlate GetNextPressurePlate(PressurePlate currentPressurePlate)
        {
            int i = 0;
            for (; i < pressurePlates.Length; i++)
            {
                if(pressurePlates[i] == currentPressurePlate)
                {
                    break;
                }
            }

            if(movementFlow == MovementFlowEnum.Clockwise)
            {
                return pressurePlates[(i + 1) % pressurePlates.Length];
            }
            else if(movementFlow == MovementFlowEnum.Counterclockwise)
            {
                i--;
                if(i < 0)
                {
                    i = pressurePlates.Length - 1;
                }

                return pressurePlates[i];
            }
            else
            {
                return null;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!_isReady)
                return;

            currentRoundTimer -= Time.deltaTime;

            if (currentRoundTimer <= 0f)
            {
                for(int i = 0; i < _agents.Count; i++)
                {
                    _agents[i].extraChanceToStop = 0f;
                    _agents[i].SetNextGoal(GetNextPressurePlate(_agents[i].GetCurrentPressurePlate()));
                }

                currentRoundTimer = roundTime;
            }

            //Rotate
            Vector3 targetRotation;
            if (movementFlow == MovementFlowEnum.Clockwise)
            {
                targetRotation = Vector3.zero;
            }
            else
            {
                targetRotation = new Vector3(0, 0, 180);
            }

            foreach (GameObject arrow in _flowArrows)
            {
                arrow.transform.localEulerAngles = Vector3.MoveTowards(arrow.transform.localEulerAngles, targetRotation, Time.deltaTime * _speedArrow);
            }

            //reset auxins
            for (int i = 0; i < _cells.Count; i++)
                for (int j = 0; j < _cells[i].Auxins.Count; j++)
                    _cells[i].Auxins[j].ResetAuxin();

            //find nearest auxins for each agent
            for (int i = 0; i < _agents.Count; i++)
                _agents[i].FindNearAuxins();

            /*
             * to find where the agent must move, we need to get the vectors from the agent to each auxin he has, and compare with 
             * the vector from agent to goal, generating a angle which must lie between 0 (best case) and 180 (worst case)
             * The calculation formula was taken from the Bicho´s master thesis and from Paravisi OSG implementation.
            */
            /*for each agent:
            1 - for each auxin near him, find the distance vector between it and the agent
            2 - calculate the movement vector 
            3 - calculate speed vector 
            4 - step
            */

            for (int i = 0; i < _agents.Count; i++)
            {
                //find the agent
                List<Auxin> agentAuxins = _agents[i].Auxins;

                //vector for each auxin
                for (int j = 0; j < agentAuxins.Count; j++)
                {
                    //add the distance vector between it and the agent
                    _agents[i]._distAuxin.Add(agentAuxins[j].Position - _agents[i].transform.position);

                    //just draw the lines to each auxin
                    Debug.DrawLine(agentAuxins[j].Position, _agents[i].transform.position, Color.green);
                }

                //calculate the movement vector
                _agents[i].CalculateDirection();
                //calculate speed vector
                _agents[i].CalculateVelocity();
                //step
                _agents[i].Step();
            }
        }
    }
}