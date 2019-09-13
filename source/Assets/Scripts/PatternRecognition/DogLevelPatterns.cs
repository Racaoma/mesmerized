using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogLevelPatterns : MonoBehaviour
{
    public GameObject character;
    public GameObject dog;

    // Update is called once per frame
    void Update()
    {
        SaveManager.currentProgress.timeToCompleteAllLevels += Time.deltaTime;

        if(Vector3.Distance(character.transform.position, dog.transform.position) <= 1f)
        {
            SaveManager.currentProgress.timeSpentNearDog += Time.deltaTime;
        }
    }
}
