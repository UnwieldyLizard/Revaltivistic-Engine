using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinkowskiMetric : Metric
{
    public double[,] Generate(FourVector position) {
        double[,] metric = new double[4,4];
        metric[0,0] = 1;
        metric[1,1] = -1;
        metric[2,2] = -1;
        metric[3,3] = -1;
        return metric;
    }

    public double LorentzDot(FourVector location, FourVector vec1, FourVector vec2) {
        double[,] metric = Generate(location);
        double product = 0;
        for (int i = 0; i < 4; i++) {
            for (int j = 0; j < 4; j++) {
                product += vec1.components[i]*metric[i,j]*vec2.components[j];
            }
        }
        return product;
    }

    public FourVector LorentzLower(FourVector location, FourVector vec) {
        double[,] metric = Generate(location);
        double[] covec = new double[4];
        for (int i = 0; i < 4; i++) {
            for (int j = 0; j < 4; j++) {
                covec[i] += metric[i,j]*vec.components[j];
            }
        }
        return new FourVector(covec);
    }
}
