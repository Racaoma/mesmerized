using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pedestal : InteractableItemBehaviour
{
    [SerializeField] private GameObject robotGameObject;
    [SerializeField] private GameObject machineGameObject;
    [SerializeField] private GameObject dogGameObject;
    [SerializeField] private GameObject lampGameObject;

    private GameObject _currentPlaceGameObject;
    private bool isActive = true;

    protected override void ExecuteAction(CharacterBehaviour character)
    {
        if(isActive)
        {
            isActive = false;
            if(character && character.CheckInventaryObjectOnSelectedPosition("Robot"))
            {
                robotGameObject.SetActive(true);
            }
            else if (character && character.CheckInventaryObjectOnSelectedPosition("Machine"))
            {
                machineGameObject.SetActive(true);
            }
            else if (character && character.CheckInventaryObjectOnSelectedPosition("Lamp"))
            {
                lampGameObject.SetActive(true);
            }
            else if (character && character.CheckInventaryObjectOnSelectedPosition("Dog"))
            {
                dogGameObject.SetActive(true);
            }
            else
            {
                Debug.Log("None");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        isActive = true;
    }
}
