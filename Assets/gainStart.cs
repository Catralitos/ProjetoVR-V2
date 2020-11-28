﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gainStart : MonoBehaviour
{
    public VRTK.VRTK_StepMultiplier step;
    private void Start()
    {
        step.additionalMovementMultiplier = 0f;
        step.enabled = false;
    }
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            step.enabled = true;
            step.additionalMovementMultiplier = 0.5f;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            step.enabled = true;
            step.additionalMovementMultiplier = 0.5f;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            
            step.additionalMovementMultiplier = 0f;
            step.enabled = false;
        }
    }
}
