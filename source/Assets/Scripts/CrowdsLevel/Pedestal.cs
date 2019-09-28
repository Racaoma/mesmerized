using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pedestal : InteractableItemBehaviour
{
    [SerializeField] private InventoryCenterBehaviour inventoryCenter;
    [SerializeField] private GameObject robotGameObject;
    [SerializeField] private GameObject machineGameObject;
    [SerializeField] private GameObject dogGameObject;
    [SerializeField] private GameObject lampGameObject;
    [Space]
    [SerializeField] private GameObject robotItemPrefab;
    [SerializeField] private GameObject machineItemPrefab;
    [SerializeField] private GameObject dogItemPrefab;
    [SerializeField] private GameObject lampItemPrefab;
    private bool isActive = true;

    public enum PressurePlateActiveItemEnum
    {
        None,
        Dog,
        Machine,
        Robot,
        Lamp
    }

    protected override void ExecuteAction(CharacterBehaviour character)
    {
        if(isActive)
        {
            isActive = false;

            if (robotGameObject.activeInHierarchy)
            {
                robotGameObject.SetActive(false);
                GameObject obj = Instantiate(robotItemPrefab, character.transform.position, Quaternion.identity);
                obj.GetComponent<InventoryObjectBehaviour>().inventaryCenter = inventoryCenter;
            }
            else if(machineGameObject.activeInHierarchy)
            {
                machineGameObject.SetActive(false);
                GameObject obj = Instantiate(machineItemPrefab, character.transform.position, Quaternion.identity);
                obj.GetComponent<InventoryObjectBehaviour>().inventaryCenter = inventoryCenter;
            }
            else if(dogGameObject.activeInHierarchy)
            {
                dogGameObject.SetActive(false);
                GameObject obj = Instantiate(dogItemPrefab, character.transform.position, Quaternion.identity);
                obj.GetComponent<InventoryObjectBehaviour>().inventaryCenter = inventoryCenter;
            }
            else if(lampGameObject.activeInHierarchy)
            {
                lampGameObject.SetActive(false);
                GameObject obj = Instantiate(lampItemPrefab, character.transform.position, Quaternion.identity);
                obj.GetComponent<InventoryObjectBehaviour>().inventaryCenter = inventoryCenter;
            }

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
        }
    }

    public PressurePlateActiveItemEnum GetActiveItem()
    {
        if(robotGameObject.activeInHierarchy)
        {
            return PressurePlateActiveItemEnum.Robot;
        }
        else if (machineGameObject.activeInHierarchy)
        {
            return PressurePlateActiveItemEnum.Machine;
        }
        else if (dogGameObject.activeInHierarchy)
        {
            return PressurePlateActiveItemEnum.Dog;
        }
        else if (lampGameObject.activeInHierarchy)
        {
            return PressurePlateActiveItemEnum.Lamp;
        }
        else
        {
            return PressurePlateActiveItemEnum.None;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        isActive = true;
    }
}
