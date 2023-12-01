using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KerrMetric : Metric
{
    private PhysicalObject source;
    private double spin;
    private Vector3 nHat;
    private double rapidity;
    private double a;
    private double b;
    private double integralRefinement;

    public KerrMetric(PhysicalObject source, double spin) {
        this.source = source;
        this.spin = spin;
        integralRefinement = 1;
    }
    
    public void Initialize() {
        if (source is not null) {
            Vector3 relativeVelocity = source.GetVelocity();
            double relativeSpeed = relativeVelocity.magnitude;
            nHat = Vector3.Normalize(relativeVelocity);
            rapidity = Math.Atanh(relativeSpeed);
            a = Math.Cosh(rapidity);
            b = Math.Sinh(rapidity);
        } else {
            nHat = new Vector3(0,0,1);
            rapidity = 0;
            a = 1;
            b = 0;
        }
    }
    public double[,] Generate(FourVector position) {
        //https://arxiv.org/pdf/1810.06507.pdf
        double[] oblatePosition = position.GetKerrCoordinates(source.position.v3, spin);
        double u = oblatePosition[0];
        double r = oblatePosition[1];
        Debug.Log(r);
        double theta = oblatePosition[2];
        double phi = oblatePosition[3];
        double xHatDotnHat = (Math.Cos(theta)*nHat.z + Math.Sin(theta)*(Math.Cos(phi)*nHat.x + Math.Sin(phi)*nHat.y));
        double K = (a + b*xHatDotnHat);
        double Sigma = (spin * ((b+a*xHatDotnHat)/(a+b*xHatDotnHat)));
        double LIntegral = 0;
        for (int t = 0; t < (2*Math.PI/integralRefinement); t++) {
            LIntegral += LIntegrandFunc(t, phi) * integralRefinement;
        }
        double c1;
        if (rapidity == 0) {
            c1 = 0;
        } else {
            c1 = (spin/(2*b*b));
        }
        double L = ((1-Math.Cos(theta))/Math.Sin(theta))*(c1 - LIntegral);
        double annoyingFactor = 1 - (2*source.mass*r/(r*r + Sigma*Sigma));
        double irritatingFactor = (r*r + Sigma*Sigma)/(K*K);

        double[,] metric = new double[4,4];
        metric[0,0] += annoyingFactor;

        metric[0,3] += -4*L*Cot(theta/2)*annoyingFactor;
        metric[3,0] += -4*L*Cot(theta/2)*annoyingFactor;
        
        metric[3,3] += 4*L*L*Cot(theta/2)*Cot(theta/2)*annoyingFactor;
        
        metric[2,2] += -irritatingFactor;
        
        metric[3,3] += -Math.Sin(theta)*Math.Sin(theta) * irritatingFactor;
        
        metric[0,1] += 2;
        metric[1,0] += 2;
        
        metric[0,3] += 2*spin*(-nHat.z*Math.Sin(theta)*Math.Sin(theta) + (nHat.x*Math.Cos(phi) + nHat.y*Math.Sin(phi))*Math.Sin(theta)*Math.Cos(theta))/(K*K);
        metric[3,0] += 2*spin*(-nHat.z*Math.Sin(theta)*Math.Sin(theta) + (nHat.x*Math.Cos(phi) + nHat.y*Math.Sin(phi))*Math.Sin(theta)*Math.Cos(theta))/(K*K);
        
        metric[0,2] += 2*spin*(nHat.x*Math.Sin(theta) - nHat.y*Math.Cos(phi))/(K*K);
        metric[2,0] += 2*spin*(nHat.x*Math.Sin(theta) - nHat.y*Math.Cos(phi))/(K*K);
        
        metric[1,3] += -4*L*Cot(theta/2);
        metric[3,1] += -4*L*Cot(theta/2);

        metric[3,3] += -4*L*Cot(theta/2)*spin*(-nHat.z*Math.Sin(theta)*Math.Sin(theta) + (nHat.x*Math.Cos(phi) + nHat.y*Math.Sin(phi))*Math.Sin(theta)*Math.Cos(theta))/(K*K);

        metric[2,3] += -4*L*Cot(theta/2)*spin*(nHat.x*Math.Sin(theta) - nHat.y*Math.Cos(phi))/(K*K);
        metric[3,2] += -4*L*Cot(theta/2)*spin*(nHat.x*Math.Sin(theta) - nHat.y*Math.Cos(phi))/(K*K);

        
        Debug.Log("");
        Debug.Log(metric[0,0] + "," + metric[0,1] + "," + metric[0,2] + "," + metric[0,3]);
        Debug.Log(metric[1,0] + "," + metric[1,1] + "," + metric[1,2] + "," + metric[1,3]);
        Debug.Log(metric[2,0] + "," + metric[2,1] + "," + metric[2,2] + "," + metric[2,3]);
        Debug.Log(metric[3,0] + "," + metric[3,1] + "," + metric[3,2] + "," + metric[3,3]);
        

        return metric;
    }

    private double Cot(double x) {
        return 1/Math.Tan(x);
    }
    public double LIntegrandFunc(double intTheta, double phi) {
        double xHatDotnHat = (Math.Cos(intTheta)*nHat.z + Math.Sin(intTheta)*(Math.Cos(phi)*nHat.x + Math.Sin(phi)*nHat.y));
        double K = (a + b*xHatDotnHat);
        double Sigma = (spin * ((b+a*xHatDotnHat)/(a+b*xHatDotnHat)));
        return (Sigma/(K*K))*Math.Sin(intTheta);
    }

    public double LorentzDot(FourVector location, FourVector vec1, FourVector vec2) {
        double[] vec1Oblate = vec1.GetKerrCoordinates(source.position.v3, spin);
        double[] vec2Oblate = vec2.GetKerrCoordinates(source.position.v3, spin);
        double[,] metric = Generate(location);
        double product = 0;
        for (int i = 0; i < 4; i++) {
            for (int j = 0; j < 4; j++) {
                product += vec1Oblate[i]*metric[i,j]*vec2Oblate[j];
            }
        }
        return product;
    }

    public FourVector LorentzLower(FourVector location, FourVector vec) {
        double[,] metric = Generate(location);
        double[] vecOblate = vec.GetKerrCoordinates(source.position.v3, spin);
        double[] covecOblate = new double[4];
        for (int i = 0; i < 4; i++) {
            for (int j = 0; j < 4; j++) {
                covecOblate[i] += metric[i,j]*vecOblate[j];
            }
        }
        double[] covec = new double[4] {covecOblate[0], 
            Math.Sqrt(covecOblate[1]*covecOblate[1] + spin*spin)*Math.Sin(covecOblate[2])*Math.Cos(covecOblate[3]) + source.position.v3.x,
            Math.Sqrt(covecOblate[1]*covecOblate[1] + spin*spin)*Math.Sin(covecOblate[2])*Math.Sin(covecOblate[3]) + source.position.v3.y,
            covecOblate[1]*Math.Cos(covecOblate[2]) + source.position.v3.z};
        return new FourVector(covec);
    }
}
