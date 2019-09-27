using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateItem : MonoBehaviour
{
    public Vector3 rotationAxis;
    private float speed = 50.0f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotationAxis * Time.deltaTime * speed);
    }
}
