using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Metric
{
    public double[,] Generate(FourVector position);
    public double LorentzDot(FourVector location, FourVector vec1, FourVector vec2);
    public FourVector LorentzLower(FourVector location, FourVector vec);
}
