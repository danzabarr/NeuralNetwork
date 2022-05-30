using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestAction : MonoBehaviour
{
    public float maxMovementSpeed;
    public float maxRotationSpeed;

    public void MoveForward(float amount)
    {
        amount = Mathf.Clamp(amount, 0, maxMovementSpeed);
        transform.position += transform.forward * amount * Time.deltaTime;
    }

    public void MoveBack(float amount)
    {
        amount = Mathf.Clamp(amount, 0, maxMovementSpeed);
        transform.position -= transform.forward * amount * Time.deltaTime;
    }

    public void MoveRight(float amount)
    {
        amount = Mathf.Clamp(amount, 0, maxMovementSpeed);
        transform.position += transform.right * amount * Time.deltaTime;
    }

    public void MoveLeft(float amount)
    {
        amount = Mathf.Clamp(amount, 0, maxMovementSpeed);
        transform.position -= transform.right * amount * Time.deltaTime;
    }

    public void Rotate(float amount)
    {
        amount *= 180;
        amount = Mathf.Clamp(amount, -maxRotationSpeed, maxRotationSpeed);
        transform.Rotate(Vector3.up, amount * Time.deltaTime);
    }

    
}
