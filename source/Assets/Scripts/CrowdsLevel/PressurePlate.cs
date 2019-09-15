using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public Vector3 GetRandomPositionInside(float range)
    {
        Vector3 randomPosition = Random.insideUnitSphere * range;
        return new Vector3(this.transform.position.x + randomPosition.x, this.transform.position.y, this.transform.position.z + randomPosition.z);
    }
}
