using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTile : MonoBehaviour, ILevelThing
{
    public Vector3 OriginalPosition
    {
        get; set;
    }


    void Awake()
    {
        OriginalPosition = transform.localPosition;
    }


    public void Reset()
    {
        transform.localPosition = OriginalPosition;
    }


}
