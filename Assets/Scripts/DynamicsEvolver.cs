using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicsEvolver
{
    public Metric metric;
    private Lagrangian lagrangian;
    private PhysicalObject subject;
    //Granularity params
    private double variationStep;
    private double tolerance;
    
    public DynamicsEvolver(Metric metric, Lagrangian lagrangian, double[] granularity) {
        this.metric = metric;
        this.lagrangian = lagrangian;
        this.variationStep = granularity[0];
        this.tolerance = granularity[1];
    }
    public void SetSubject(PhysicalObject subject) {
        this.subject = subject;
    }
    private double GetL_d(FourVector pos1, FourVector pos2) {
        //using the delta tau squared gimmick hehe
        //Debug.Log(pos1 + "," + pos2);
        double delta_tau_squared = metric.LorentzDot((pos1 + pos2)/2, pos2 - pos1, pos2 - pos1);
        double L_d = lagrangian.Calculate(metric.Generate((pos1 + pos2)/2), subject, pos2) * delta_tau_squared;
        //Debug.Log(lagrangian.Calculate(metric.Generate((pos1 + pos2)/2), subject, pos2) + "*" + delta_tau_squared + "=" + L_d);
        return L_d;
    }
    private double GetL_dDerivative(FourVector fixedPoint, FourVector variablePoint, int variableIndex) {
        double[] variablePointPlus = new double[4];
        double[] variablePointMinus = new double[4];
        variablePoint.components.CopyTo(variablePointPlus, 0);
        variablePoint.components.CopyTo(variablePointMinus, 0);
        variablePointPlus[variableIndex] += variationStep;
        variablePointMinus[variableIndex] -= variationStep;
        double L_dDerivative = (GetL_d(fixedPoint, new FourVector(variablePointPlus)) - GetL_d(fixedPoint, new FourVector(variablePointMinus)))/(2*variationStep);
        //Debug.Log("varidx: "+variableIndex + " varstep: " + variationStep);
        //Debug.Log("varpoint: " + variablePoint);
        //Debug.Log(new FourVector(variablePointPlus) + "-" + new FourVector(variablePointMinus) + "=" + (new FourVector(variablePointPlus) - new FourVector(variablePointMinus)) + " != 0");
        //Debug.Log(GetL_d(fixedPoint, new FourVector(variablePointPlus)) + "-" + GetL_d(fixedPoint, new FourVector(variablePointMinus)) + "=" + L_dDerivative*2*variationStep);
        return L_dDerivative;
    }
    private FourVector GetVariationSpaceGradient(FourVector knownPoint, FourVector testPoint, FourVector target) {
        double[] gradient = new double[4];
        for (int k = 0; k < 4; k++) {
            double[] testPointPlus = new double[4];
            double[] testPointMinus = new double[4];
            testPoint.components.CopyTo(testPointPlus, 0);
            testPoint.components.CopyTo(testPointMinus, 0);            
            testPointPlus[k] += variationStep;
            testPointMinus[k] -= variationStep;
            gradient[k] = (GetL_dDerivative(new FourVector(testPointPlus), knownPoint, k) - GetL_dDerivative(new FourVector(testPointMinus), knownPoint, k))/(2*variationStep);
            gradient[k] += target.components[k];
        }
        return new FourVector(gradient);
    }
    private bool CheckGuess(FourVector guess) {
        bool isIncorrect = false;
        for (int d = 0; d < 4; d++) {
            if (Math.Abs(guess.components[d]) > tolerance) {
                isIncorrect = true;
            }
        }
        return isIncorrect;
    }
    public Quaternion FindNextPosition(PhysicalObject subject, bool forward) {
        this.subject = subject;
        FourVector p0 = new FourVector();
        FourVector p1 = new FourVector();
        if (forward) {
            p0 = subject.previousPosition.v4;
            p1 = subject.position.v4;
        } else {
            p1 = subject.previousPosition.v4;
            p0 = subject.position.v4;
        }

        FourVector p2Guess = p1 + (p1 - p0);
        FourVector dLdp01 = new FourVector(new double[4]);
        FourVector dLdp12 = new FourVector(new double[4]);
        for (int d = 0; d<4; d++) {
            dLdp01.components[d] = GetL_dDerivative(p0, p1, d);
            dLdp12.components[d] = GetL_dDerivative(p2Guess, p1, d);
        }
        FourVector gradient = GetVariationSpaceGradient(p1, p2Guess, dLdp01);
        FourVector difference = dLdp01 + dLdp12;

        int i = 0;
        while (CheckGuess(difference)) {
            p2Guess -= (difference/gradient)*(Math.Pow(variationStep, (i/500)));
            gradient = GetVariationSpaceGradient(p1, p2Guess, dLdp01);
            for (int d = 0; d<4; d++) {
                dLdp12.components[d] = GetL_dDerivative(p2Guess, p1, d);
            }
            difference = dLdp01 + dLdp12;
            i ++;
            if (i > 1000) {
                Debug.Log("failed to converge");
                break;
            }
        }
        //Debug.Log(dLdp01 +"+"+dLdp12+"="+difference);
        return new Quaternion(p2Guess);
    }
    
}
