using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerProgress
{
    public string playerName;
    public bool completedTutorial = false;
    public List<LevelProgress> levels = new List<LevelProgress>();

    //Patterns
    public float timeSpentNearDog;
    public float timeToCompleteAllLevels;
    public int gatesPuzzleLeverUses;
    public int buttonPuzzleButtonUses;
    public int machineExplosionsTriggered;

    public PlayerProgress(string name)
    {
        playerName = name;
        completedTutorial = false;
        levels = new List<LevelProgress>();

        timeSpentNearDog = 0f;
        timeToCompleteAllLevels = 0f;
        gatesPuzzleLeverUses = 0;
        buttonPuzzleButtonUses = 0;
        machineExplosionsTriggered = 0;
    }
}

[System.Serializable]
public class LevelProgress
{   
    public GameEnums.LevelName levelName;
    public bool levelConcluded;
    public bool patientLeftBed;
    public bool attemptedToTalk;

    public LevelProgress(GameEnums.LevelName name)
    {
        levelName = name;
        levelConcluded = false;
        patientLeftBed = false;
        attemptedToTalk = false;
    }
}
