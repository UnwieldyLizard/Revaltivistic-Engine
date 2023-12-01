using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeParticleLagrangian : Lagrangian
{
    public double Calculate(double[,] metric, PhysicalObject subject, FourVector nextPosition) {
        double lagrangian = - subject.mass;
        return lagrangian;
    }
}
