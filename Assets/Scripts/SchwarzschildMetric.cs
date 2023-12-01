using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SchwarzschildMetric : Metric
{
    private PhysicalObject source;

    public SchwarzschildMetric(PhysicalObject source) {
        this.source = source;
    }
    
    public double[,] Generate(FourVector position) {
        FourVector relativePosition = position - source.position.v4;
        double spacialExpression = (1 + ((2*source.mass)/(4*relativePosition.r)));
        double temporalExpression = (1 - ((2*source.mass)/(4*relativePosition.r)));

        double[,] metric = new double[4,4];
        metric[0,0] = (temporalExpression/spacialExpression) * (temporalExpression/spacialExpression);
        metric[1,1] = - spacialExpression*spacialExpression*spacialExpression*spacialExpression;
        metric[2,2] = - spacialExpression*spacialExpression*spacialExpression*spacialExpression;
        metric[3,3] = - spacialExpression*spacialExpression*spacialExpression*spacialExpression;
        
        /*
        Debug.Log("");
        Debug.Log(metric[0,0] + "," + metric[0,1] + "," + metric[0,2] + "," + metric[0,3]);
        Debug.Log(metric[1,0] + "," + metric[1,1] + "," + metric[1,2] + "," + metric[1,3]);
        Debug.Log(metric[2,0] + "," + metric[2,1] + "," + metric[2,2] + "," + metric[2,3]);
        Debug.Log(metric[3,0] + "," + metric[3,1] + "," + metric[3,2] + "," + metric[3,3]);
        */

        return metric;
    }

    public double LorentzDot(FourVector location, FourVector vec1, FourVector vec2) {
        double[] vec1Rel = (vec1 - source.position.v4).components;
        double[] vec2Rel = (vec2 - source.position.v4).components;
        double[,] metric = Generate(location);
        double product = 0;
        for (int i = 0; i < 4; i++) {
            for (int j = 0; j < 4; j++) {
                product += vec1Rel[i]*metric[i,j]*vec2Rel[j];
            }
        }
        return product;
    }

    public FourVector LorentzLower(FourVector location, FourVector vec) {
        double[,] metric = Generate(location);
        double[] vecRel = (vec - source.position.v4).components;
        double[] covecRel = new double[4];
        for (int i = 0; i < 4; i++) {
            for (int j = 0; j < 4; j++) {
                covecRel[i] += metric[i,j]*vecRel[j];
            }
        }
        return (new FourVector(covecRel) + source.position.v4);
    }
}
