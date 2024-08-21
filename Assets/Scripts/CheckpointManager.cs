using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public CarAgent DriverAgent;
    public Checkpoint nextCheckPointToReach;
    
    private int CurrentCheckpointIndex;
    public List<Checkpoint> Checkpoints;
    private Checkpoint lastCheckpoint;

    public event Action<Checkpoint> reachedCheckpoint; 

    void Start()
    {
        Checkpoints = FindObjectOfType<Checkpoints_all>().checkPoints;
        ResetCheckpoints();
    }
    public void ResetCheckpoints()
    {
        CurrentCheckpointIndex = 0;
        SetNextCheckpoint();
    }
    public void CheckPointReached(Checkpoint checkpoint)
    {     
        if (nextCheckPointToReach != checkpoint) 
        {
            //Wrong Checkpoint
            DriverAgent.AddReward(-1f);
            Debug.Log(DriverAgent.GetCumulativeReward());

            return;
        
        }
        if (CurrentCheckpointIndex == Checkpoints.Count-1)
        {
            //Completing a lap
            lastCheckpoint = Checkpoints[CurrentCheckpointIndex];
            reachedCheckpoint?.Invoke(checkpoint);
            CurrentCheckpointIndex=0;
            DriverAgent.AddReward(5f);
        }
        else
        {
            lastCheckpoint = Checkpoints[CurrentCheckpointIndex];
            reachedCheckpoint?.Invoke(checkpoint);
            CurrentCheckpointIndex++;
        }


        //Correct Checkpoint
        DriverAgent.AddReward(0.5f);
        
        Debug.Log(DriverAgent.GetCumulativeReward());
        SetNextCheckpoint();
    }
    private void SetNextCheckpoint()
    {
        if (Checkpoints.Count > 0)
        {
            nextCheckPointToReach = Checkpoints[CurrentCheckpointIndex];
        }
    }
}