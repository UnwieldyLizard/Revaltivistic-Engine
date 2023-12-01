using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpacetimeManager
{
    private List<PhysicalObject> physicalObjects;

    public SpacetimeManager(List<PhysicalObject> physicalObjects) {
        this.physicalObjects = physicalObjects;
    }

    public void RotateFrame(Quaternion rotator) {
        foreach(PhysicalObject physicalObject in physicalObjects) {
            if (physicalObject.previousPosition is not null)
                physicalObject.previousPosition = rotator.Rotate(physicalObject.previousPosition);
            if (physicalObject.position is not null)
                physicalObject.position = rotator.Rotate(physicalObject.position);
            for (int k = 0; k < physicalObject.points.Count; k++) {
                physicalObject.points[k] = rotator.Rotate(physicalObject.points[k]);
            }
            physicalObject.Refresh();
        }
    }

    public void BoostFrame(Quaternion booster) {
        foreach(PhysicalObject physicalObject in physicalObjects) {
            if (physicalObject.previousPosition is not null)
                physicalObject.previousPosition = booster.Boost(physicalObject.previousPosition);
            if (physicalObject.position is not null)
                physicalObject.position = booster.Boost(physicalObject.position);
            for (int k = 0; k < physicalObject.points.Count; k++) {
                physicalObject.points[k] = booster.Boost(physicalObject.points[k]);
            }
            physicalObject.Refresh();
        }
    }

    public void TranslateFrame(Quaternion translator) {
        foreach(PhysicalObject physicalObject in physicalObjects) {
            if (physicalObject.previousPosition is not null)
                physicalObject.previousPosition -= translator;
            if (physicalObject.position is not null)
                physicalObject.position -= translator;
            for (int k = 0; k < physicalObject.points.Count; k++) {
                physicalObject.points[k] -= translator;
            }
            physicalObject.Refresh();
        }
    }
}
