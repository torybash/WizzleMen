using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTile : MonoBehaviour, ILevelThing
{
	public Vector3 OriginalPosition{
		get; private set;
	}

	public Level Level{
		get; private set;
	}

    void Awake()
    {
        OriginalPosition = transform.localPosition;
    }


    public void Reset()
    {
        if (!Application.isPlaying) return;

        transform.localPosition = OriginalPosition;
    }


}
