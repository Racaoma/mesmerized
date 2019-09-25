using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pedestal : InteractableItemBehaviour
{
    protected override void ExecuteAction(CharacterBehaviour character)
    {
        Debug.Log("Pedestal");
    }
}
