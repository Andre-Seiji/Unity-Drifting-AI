using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoints_all : MonoBehaviour
{
    public List<Checkpoint> checkPoints;
    
    private void Awake()
    {
        checkPoints = new List<Checkpoint>(GetComponentsInChildren<Checkpoint>());
    }
}