using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhysicalObject : MonoBehaviour
{
    // location data
    public string displayName;
    public List<Quaternion> points;
    public Quaternion position;
    public Quaternion previousPosition;
    //Evolver
    public DynamicsEvolver dynamicsEvolver;
    private bool aboveLightCone;
    // Other params
    public double mass;
    public double charge;

    public virtual void SetName(string name) {
        this.displayName = name;
    }

    public virtual void SetMass(double mass) {
        this.mass = mass;
    }

    public virtual void SetCharge(double charge) {
        this.charge = charge;
    }

    public virtual void SetPosition(Quaternion newPosition) {
        if (position is not null) {
            previousPosition = position;
        }
        position = newPosition;
    }
    
    public virtual void StepBack(Quaternion newPreviousPosition) {
        if (previousPosition is not null) {
            position = previousPosition;
        }
        previousPosition = newPreviousPosition;
    }

    public virtual void AttachEvolver(DynamicsEvolver dynamicsEvolver) {
        this.dynamicsEvolver = dynamicsEvolver;
    }

    private Quaternion GetDisplayPosition() {
        if (previousPosition is null || position is null) {
            return null;
        }
        //Interpolates between current and previous position to find point on light cone where it should be observed, if positions don't thread the light cone returns null.
        double a = (position.v3 - previousPosition.v3).sqrMagnitude - (position.s - previousPosition.s)*(position.s - previousPosition.s);
        double b = 2*Vector3.Dot((position.v3 - previousPosition.v3), previousPosition.v3) - 2*(position.s - previousPosition.s)*previousPosition.s;
        double c = previousPosition.v3.sqrMagnitude - previousPosition.s*previousPosition.s;
        double t = (-b + Math.Sqrt(b*b - 4*a*c)) / (2*a);
        if (t >= 0 && t <= 1) {
            return previousPosition + t * (position - previousPosition);
        } else if (t < 0) {
            aboveLightCone = true;
            return null;
        } else { //t > 1
            aboveLightCone = false;
            return null;
        }
    }
    
    public virtual void Refresh() {
        Quaternion displayPosition = GetDisplayPosition();
        if (displayPosition is not null) {
            transform.localPosition = displayPosition.v3;
        } else {
            //Debug.Log(displayPosition);
            if (position is null || previousPosition is null) {
                return;
            }
            int attempts = 0;
            while (displayPosition is null) {
                if (!aboveLightCone)
                    SetPosition(dynamicsEvolver.FindNextPosition(this, !aboveLightCone));
                if (aboveLightCone)
                    StepBack(dynamicsEvolver.FindNextPosition(this, !aboveLightCone));
                displayPosition = GetDisplayPosition();
                if (attempts > 100) {
                    Debug.Log(aboveLightCone);
                    Debug.Log("something fucked");
                    break;
                }
                attempts ++;
            }
            transform.localPosition = displayPosition.v3;
        }
    }

    public virtual Vector3 GetVelocity() {
        if (previousPosition is null || position is null) {
            return new Vector3(42,42,42);
        }
        Quaternion displacement = position - previousPosition;
        Vector3 velocity = new Vector3((float)(displacement.v[0] / displacement.s), (float)(displacement.v[1] / displacement.s), (float)(displacement.v[2] / displacement.s));
        return velocity;
    }
}
