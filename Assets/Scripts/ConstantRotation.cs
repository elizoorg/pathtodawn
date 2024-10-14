using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantRotation : MonoBehaviour
{
    public float AngularVelocityDegrees;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float halfRad = AngularVelocityDegrees * Time.deltaTime * Mathf.Deg2Rad * 0.5f;
        gameObject.transform.rotation *= new Quaternion(0.0f, Mathf.Sin(halfRad), 0.0f, Mathf.Cos(halfRad));
    }
}
