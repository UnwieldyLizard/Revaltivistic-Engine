using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourVector
{
    public double[] components;
    public double t {
        get {
            return components[0];
        }
    }
    public double x {
        get {
            return components[1];
        }
    }
    public double y {
        get {
            return components[2];
        }
    }
    public double z {
        get {
            return components[3];
        }
    }
    public double r {
        get {
            return Math.Sqrt(x*x+y*y+z*z);
        }
    }
    public double phi {
        get {
            return Math.Atan2(y,x);
        }
    }
    public double theta {
        get {
            return Math.Acos(z/r);
        }
    }
    public FourVector() {
        return;
    }
    public FourVector(double[] components) {
        this.components = components;
    }
    
    //Oblate coordinates
    public double[] GetKerrCoordinates(Vector3 center, double a) {
        double x_ = x - center.x;
        double y_ = y - center.y;
        double z_ = z - center.z;
        double t_ = t - center.magnitude;
        double oblateR = Math.Sqrt(Math.Sqrt(x_*x_*x_*x_ + y_*y_*y_*y_ + z_*z_*z_*z_ + a*a*a*a + 2*x_*x_*y_*y_ + 2*y_*y_*z_*z_ + 2*x_*x_*z_*z_ + 2*a*a*z_*z_ - 2*a*a*x_*x_ - 2*a*a*y_*y_) + x_*x_ + y_*y_ + z_*z_ - a*a)/Math.Sqrt(2);
        double oblateTheta = Math.Acos(z_/oblateR);
        return new double[] {t_, oblateR, oblateTheta, phi};
    }
    //Addition & Subtraction
    public static FourVector operator +(FourVector leftAddend, FourVector rightAddend) {
        double[] sum = new double[4];
        for (int d = 0; d<4; d++) {
            sum[d] = leftAddend.components[d] + rightAddend.components[d];
        }
        return new FourVector(sum);
    }
    public static FourVector operator -(FourVector minuend, FourVector subtrahend) {
        double[] difference = new double[4];
        for (int d = 0; d<4; d++) {
            difference[d] = minuend.components[d] - subtrahend.components[d];
        }
        return new FourVector(difference);
    }
    //Multiplication & Division
    public static FourVector operator *(FourVector vec1, FourVector vec2) {
        double[] product = new double[4];
        for (int d = 0; d<4; d++) {
            product[d] = vec1.components[d] * vec2.components[d];
        }
        return new FourVector(product);
    }
    public static FourVector operator *(double scalar, FourVector vec) {
        double[] product = new double[4];
        for (int d = 0; d<4; d++) {
            product[d] = scalar * vec.components[d];
        }
        return new FourVector(product);
    }
    public static FourVector operator *(FourVector vec, double scalar) {
        double[] product = new double[4];
        for (int d = 0; d<4; d++) {
            product[d] = scalar * vec.components[d];
        }
        return new FourVector(product);
    }
    public static FourVector operator /(FourVector vec1, FourVector vec2) {
        double[] quotient = new double[4];
        for (int d = 0; d<4; d++) {
            quotient[d] = vec1.components[d] / vec2.components[d];
        }
        return new FourVector(quotient);
    }
    public static FourVector operator /(FourVector vec, double scalar) {
        double[] quotient = new double[4];
        for (int d = 0; d<4; d++) {
            quotient[d] = vec.components[d] / scalar;
        }
        return new FourVector(quotient);
    }

    public override string ToString() {
        return "<"+t+","+x+","+y+","+z+">";
    }
}
