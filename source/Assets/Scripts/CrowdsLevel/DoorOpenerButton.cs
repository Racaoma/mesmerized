using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpenerButton : InteractableItemBehaviour
{
    [SerializeField] private GameObject _buttonFrame;
    [SerializeField] private Material _greenMaterial;
    [SerializeField] private Material _redMaterial;
    [SerializeField] private PressurePlate _triggerPressurePlate;
    [SerializeField] private PressurePlate _targetPressurePlate;

    public bool isPressed = false;

    protected override void ExecuteAction(CharacterBehaviour character)
    {
        _buttonFrame.GetComponentInChildren<MeshRenderer>().material = _greenMaterial;
        isPressed = true;
    }

    private void OnTriggerExit(Collider other)
    {
        _buttonFrame.GetComponentInChildren<MeshRenderer>().material = _redMaterial;
        isPressed = false;
    }

    private void Update()
    {
        _targetPressurePlate.SetDoorState(_triggerPressurePlate.isEmpty && isPressed);
    }
}
