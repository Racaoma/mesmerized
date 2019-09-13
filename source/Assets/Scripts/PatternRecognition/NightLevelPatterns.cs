using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightLevelPatterns : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        SaveManager.currentProgress.timeToCompleteAllLevels += Time.deltaTime;
    }
}
