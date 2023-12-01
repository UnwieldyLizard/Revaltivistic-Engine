using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Speedometer : MonoBehaviour
{
    [SerializeField] private TMP_Text displayName;
    [SerializeField] private TMP_Text velocityField;

    public void UpdateDisplay(string name, Vector3 velocity) {
        displayName.text = name;
        velocityField.text = "<"+(velocity.x.ToString("F3"))+","+(velocity.y.ToString("F3"))+","+(velocity.z.ToString("F3"))+">";
    }

    public void SetNothingTracked() {
        displayName.text = "Press R3 to lock on to a target";
        velocityField.text = "";
    }
}
