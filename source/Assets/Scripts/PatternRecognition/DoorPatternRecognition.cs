using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorPatternRecognition : InteractableItemBehaviour
{
    private void Start()
    {
        SetActive(true);
    }

    protected override void ExecuteAction(CharacterBehaviour character)
    {
        SaveManager.currentProgress.timeSpentNearClosedDoor += Time.deltaTime;
    }
}
