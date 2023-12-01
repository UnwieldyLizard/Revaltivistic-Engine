using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quaternion
{
    public double s;
    public double[] v;
    public Vector3 v3 {
        get {
            return new Vector3((float)v[0], (float)v[1], (float)v[2]);
        }
    }
    public FourVector v4 {
        get {
            return new FourVector(new double[] {s, v[0], v[1], v[2]});
        }
    }
    
    public Quaternion() {
        v = new double[3];
    }
    public Quaternion(FourVector vec) {
        v = new double[3];
        Init(vec.components);
    }
    public Quaternion(double[] components) {
        v = new double[3];
        Init(components);
    }

    public void Init(double[] components) {
        s = components[0];
        for (int o = 0; o < v.Length; o++) {
            v[o] = components[o+1];
        }
    }

    public void InitPolar(double magnitude, double argument, double[] orienter) {
        s = magnitude * Math.Cos(argument);
        for(int o = 0; o < v.Length; o++) {
            v[o] = magnitude * Math.Sin(argument) * orienter[o];
        }
    }

    public void InitHyperbolic(double argument, double[] orienter) {
        s = Math.Cosh(argument);
        for(int o = 0; o < v.Length; o++) {
            v[o] = Math.Sinh(argument) * orienter[o];
        }
    }

    // Multiplication
    public static Quaternion operator *(Quaternion leftFactor, Quaternion rightFactor) {
        double[] productComponents = new double[4];
        productComponents[0] = leftFactor.s * rightFactor.s - leftFactor.v[0] * rightFactor.v[0] - leftFactor.v[1] * rightFactor.v[1] - leftFactor.v[2] * rightFactor.v[2]; 
        productComponents[1] = leftFactor.s * rightFactor.v[0] + leftFactor.v[0] * rightFactor.s + leftFactor.v[1] * rightFactor.v[2] - leftFactor.v[2] * rightFactor.v[1];
        productComponents[2] = leftFactor.s * rightFactor.v[1] + leftFactor.v[1] * rightFactor.s + leftFactor.v[2] * rightFactor.v[0] - leftFactor.v[0] * rightFactor.v[2];
        productComponents[3] = leftFactor.s * rightFactor.v[2] + leftFactor.v[2] * rightFactor.s + leftFactor.v[0] * rightFactor.v[1] - leftFactor.v[1] * rightFactor.v[0];
        return new Quaternion(productComponents);
    }

    public static Quaternion operator *(double scalar, Quaternion quaternion) {
        double[] productComponents = new double[4];
        productComponents[0] = quaternion.s * scalar;
        for (int o = 1; o <= 3; o++) {
            productComponents[o] = quaternion.v[o-1] * scalar;
        }
        return new Quaternion(productComponents);
    }

    public static Quaternion operator *(Quaternion quaternion, double scalar) {
        double[] productComponents = new double[4];
        productComponents[0] = quaternion.s * scalar;
        for (int o = 1; o <= 3; o++) {
            productComponents[o] = quaternion.v[o-1] * scalar;
        }
        return new Quaternion(productComponents);
    }

    //Addtition & subtraction
    public static Quaternion operator +(Quaternion leftAddend, Quaternion rightAddend) {
        double[] sumComponents = new double[4];
        sumComponents[0] = leftAddend.s + rightAddend.s;
        for (int o = 1; o <= 3; o++) {
            sumComponents[o] = leftAddend.v[o-1] + rightAddend.v[o-1];
        }
        return new Quaternion(sumComponents);
    }

    public static Quaternion operator -(Quaternion minuend, Quaternion subtrahend) {
        double[] differenceComponents = new double[4];
        differenceComponents[0] = minuend.s - subtrahend.s;
        for (int o = 1; o <= 3; o++) {
            differenceComponents[o] = minuend.v[o-1] - subtrahend.v[o-1];
        }
        return new Quaternion(differenceComponents);
    }

    //Printing
    public override string ToString() {
        return "("+s+"+"+v[0]+"i+"+v[1]+"j+"+v[2]+"k)";
    }

    public Quaternion Conjugate() {
        double[] conjugateComponents = new double[4];
        conjugateComponents[0] = this.s;
        for (int o = 0; o < v.Length; o++) {
            conjugateComponents[o+1] = -1 * this.v[o];
        }
        return new Quaternion(conjugateComponents);
    }

    public double Magnitude() {
        double squareMagnitude = this.s * this.s;
        for (int o = 0; o < v.Length; o++) {
            squareMagnitude += (this.v[o] * this.v[o]);
        }
        double magnitude = Math.Sqrt(squareMagnitude); 
        return magnitude;
    }

    public Quaternion Rotate(Quaternion rotationVictim) {
        // Does NOT check if this is a unit quaternion
        rotationVictim = this * rotationVictim * this.Conjugate();
        return rotationVictim;
    }

    public Quaternion Boost(Quaternion boostVictim) {
        // Does NOT check if this is a valid boost
        boostVictim = this * boostVictim * this.Conjugate() + (0.5) * boostVictim.Conjugate() * (this.Conjugate() * this.Conjugate() - this * this);
        return boostVictim; 
    }
}