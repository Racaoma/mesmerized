using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowInverterLever : InteractableItemBehaviour
{
    private bool active = true;

    protected override void ExecuteAction(CharacterBehaviour character)
    {
        if(active)
        {
            active = false;
            Biocrowds.Core.World.Instance.InvertFlow();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        active = true;
    }
}
