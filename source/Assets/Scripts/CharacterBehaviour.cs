using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Biocrowds.Core;

public class CharacterBehaviour : MonoBehaviour
{
    //Reference Variables
    [Header("Required References")]
    public CameraBehaviour cameraBehaviour;
    public InputController inputController;
    public Animator animator;
    public InventoryCenterBehaviour inventaryCenter;
    public float rotationSpeed;
    public Transform targetToRotation;
    public GameObject specialCompleteItem;
    private Quaternion _lookRotation;

    public bool useBioCrowds = false;

    //Control Variables
    private FSMController _FSMController;
    public NavMeshAgent _navMeshAgent;
    private CharacterAgent _agent;
    private bool isIdle = false;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _agent = GetComponent<CharacterAgent>();

        if (useBioCrowds)
        {
            _agent.enabled = true;
            _navMeshAgent.enabled = false;
        }
        else
        {
            _navMeshAgent.updateRotation = true;
            _agent.enabled = false;
            _navMeshAgent.enabled = true;
        }

        animator = GetComponent<Animator>();

        if (specialCompleteItem != null)
            specialCompleteItem.SetActive(false);
    }

    //Start
    private void Start()
    {
        _FSMController = new FSMController(this);
    }

    public bool IsStoped()
    {
        if(useBioCrowds)
        {
            return Vector3.Distance(_agent.GoalPosition, _agent.transform.position) <= 0.1f;
        }
        else return transform.position == _navMeshAgent.destination;
    }

    public void ActivateSpecialItem()
    {
        specialCompleteItem.SetActive(true);
    }

    //OnDestroy
    private void OnDestroy()
    {
        _FSMController.OnDestroy();
    }

    public void Move(Vector3 position)
    {
        if (!_FSMController.LockedByInteraction)
        {
            isIdle = false;

            if(useBioCrowds)
            {
                _agent.GoalPosition = position;
            }
            else
            {
                _navMeshAgent.destination = position;
                _navMeshAgent.isStopped = false;
            }
            
            _FSMController.SetNextState(GameEnums.FSMInteractionEnum.Moving);
        } 
    }

    public void DisableNavegation()
    {
        if(useBioCrowds)
        {
            _agent.enabled = false;
        }
        else
        {
            _navMeshAgent.enabled = false;
        }
    }

    public void EnableNavegation()
    {
        if (useBioCrowds)
        {
            _agent.enabled = true;
        }
        else
        {
            _navMeshAgent.enabled = true;
        }
    }

    public void SetNavMeshStopped(bool status)
    {
        if(!useBioCrowds)
        {
            _navMeshAgent.isStopped = status;
        }
    }

    public bool CheckInventaryObjectOnSelectedPosition(string name)
    {
        bool hasItem = inventaryCenter.CheckItem(name);
        if (hasItem)
        {
            inventaryCenter.UseSelectedItem();

            // trigger event
            GameEvents.LevelEvents.UsedItem.SafeInvoke();
        }
        return hasItem;
    }


    public bool CheckIfSpecialIsActivated()
    {
        return specialCompleteItem.activeSelf;
    }

    //Update
    private void Update()
    {
        if (IsStoped() == true && isIdle == false)
        {
            StartCoroutine(WaitToStop(0.05f));
            isIdle = true;
        }
        _FSMController.UpdateFSM();


    }

   public void SetRotation(Transform target)
    {
        this.transform.LookAt(target);
        this.transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, transform.eulerAngles.z);
    }

    IEnumerator WaitToStop(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (IsStoped() == true)
        {
            _FSMController.SetNextState(GameEnums.FSMInteractionEnum.Idle);
        }
            
    }
}
