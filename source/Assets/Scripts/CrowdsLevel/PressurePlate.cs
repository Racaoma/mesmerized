using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public bool isEmpty = true;

    [SerializeField] private Material _greenMaterial;
    [SerializeField] private Material _redMaterial;
    [SerializeField] private GameObject _line;
    [SerializeField] private GameObject _buttonFrame;

    private MeshRenderer _meshRenderer;
    private int _contacts = 0;

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
        _contacts++;
        UpdateMaterial(isEmpty = _contacts == 0);
    }

    private void OnTriggerExit(Collider other)
    {
        _contacts--;
        UpdateMaterial(isEmpty = _contacts == 0);
    }

    private void UpdateMaterial(bool isEmpty)
    {
        if(isEmpty)
        {
            _meshRenderer.material = _greenMaterial;
            _buttonFrame.GetComponentInChildren<MeshRenderer>().material = _greenMaterial;

            for (int i = 0; i < _line.transform.childCount; i++)
            {
                _line.transform.GetChild(i).GetComponentInChildren<MeshRenderer>().material = _greenMaterial;
            }
        }
        else
        {
            _meshRenderer.material = _redMaterial;
            _buttonFrame.GetComponentInChildren<MeshRenderer>().material = _redMaterial;

            for (int i = 0; i < _line.transform.childCount; i++)
            {
                _line.transform.GetChild(i).GetComponentInChildren<MeshRenderer>().material = _redMaterial;
            }
        }
    }
}
