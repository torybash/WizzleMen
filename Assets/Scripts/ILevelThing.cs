using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILevelThing{

    Vector3 OriginalPosition { get; set; }

    void Reset();
}
