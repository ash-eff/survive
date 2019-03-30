using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public enum State { Rest, Activity }
    public State state = State.Rest;

    public Vector2 currentPosition;

    public int energy = 3000;
    public int baseEnergy = 45;

    public void ReduceEnergy(int val) 
    {
        energy -= val;
    }
}
