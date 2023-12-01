using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Lagrangian
{
    public double Calculate(double[,] metric, PhysicalObject subject, FourVector nextPosition);
}
