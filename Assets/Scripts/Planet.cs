using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : PhysicalObject
{

    void Awake(){
        this.points = new List<Quaternion>(new Quaternion[0]);
    }
}
