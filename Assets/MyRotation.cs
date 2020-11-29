﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyRotation : MonoBehaviour
{
    public GameObject virtual_World;
    public Transform rot_reference;
    public float rotation_gain;
    private float old_rot;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print("started");
            old_rot = other.transform.rotation.eulerAngles.y;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            float new_rot = other.transform.rotation.eulerAngles.y;
            //if (System.Math.Abs(new_rot + 360 - old_rot) > 300) new_rot = (360 - System.Math.Abs(new_rot))*(new_rot/ System.Math.Abs(new_rot));
            if (System.Math.Abs( new_rot-old_rot)>0.01)
            {
                float diff = (new_rot - old_rot);
                float world_rot = (diff * rotation_gain * -1);
                if (System.Math.Abs(diff) > 300)
                {
                    world_rot = ((diff +((System.Math.Abs(diff) - 360)*diff/System.Math.Abs(diff) * rotation_gain)) * -1);
                    print("________________-");
                    print("old "+old_rot);
                    print("new "+new_rot);
                    print("dif "+ diff);
                    print("world "+world_rot);
                }
                virtual_World.transform.RotateAround(other.transform.position, Vector3.up, world_rot);
            }
            old_rot = new_rot;
        }
        
    }
}
