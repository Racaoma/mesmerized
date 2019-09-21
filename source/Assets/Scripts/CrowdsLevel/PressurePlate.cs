using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public bool isEmpty
    {
        get
        {
            return _agents.Count == 0 && !_isCharacterStanding;
        }
    }

    [SerializeField] private Material _greenMaterial;
    [SerializeField] private Material _redMaterial;
    [SerializeField] private GameObject _line;
    [SerializeField] private Animator _doorAnimator;
    [SerializeField] private Transform _doorExitPoint;

    private MeshRenderer _meshRenderer;
    private List<Biocrowds.Core.Agent> _agents = new List<Biocrowds.Core.Agent>();
    private bool _isCharacterStanding = false;

    private void Start()
    {
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    public Vector3 GetRandomPositionInside(float range)
    {
        Vector3 randomPosition = Random.insideUnitSphere * range;
        return new Vector3(this.transform.position.x + randomPosition.x, this.transform.position.y, this.transform.position.z + randomPosition.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        Biocrowds.Core.Agent agent = other.GetComponent<Biocrowds.Core.Agent>();
        if (agent != null)
        {
            agent.CheckStop(this);
            _agents.Add(agent);
        }
        else if(other.GetComponent<CharacterBehaviour>())
        {
            _isCharacterStanding = true;
        }

        UpdateMaterial(isEmpty);
    }

    private void OnTriggerExit(Collider other)
    {
        Biocrowds.Core.Agent agent = other.GetComponent<Biocrowds.Core.Agent>();
        if (agent != null)
        {
            _agents.Remove(agent);
        }
        else if (other.GetComponent<CharacterBehaviour>())
        {
            _isCharacterStanding = false;
        }

        UpdateMaterial(isEmpty);
    }

    private void UpdateMaterial(bool isEmpty)
    {
        if(isEmpty)
        {
            _meshRenderer.material = _greenMaterial;

            for (int i = 0; i < _line.transform.childCount; i++)
            {
                _line.transform.GetChild(i).GetComponentInChildren<MeshRenderer>().material = _greenMaterial;
            }
        }
        else
        {
            _meshRenderer.material = _redMaterial;

            for (int i = 0; i < _line.transform.childCount; i++)
            {
                _line.transform.GetChild(i).GetComponentInChildren<MeshRenderer>().material = _redMaterial;
            }
        }
    }

    public void SetDoorState(bool state)
    {
        if(state)
        {
            foreach(Biocrowds.Core.Agent agent in _agents)
            {
                agent.SetExitGoal(_doorExitPoint.position);
            }
        }

        _doorAnimator.SetBool("Open", state);
    }
}
