using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccretionRing : PhysicalObject
{
    [SerializeField] private GameObject ring;
    private Torus torus;
    private Material shader;

    void Awake() {
        torus = ring.GetComponent<Torus>();
        shader = ring.GetComponent<Renderer>().material;
    }

    public void Initialize(PhysicalObject host, double radius, double thickness) {
        torus.radius = (float)radius;
        torus.thickness = (float)thickness;
        Vector2 colorVec = new Vector2((float)(host.mass/radius), (float)(host.mass/(radius*radius)));
        colorVec.Normalize();
        Color ringLightColor = new Vector4(colorVec.x, colorVec.y, 0f, 1f);
        Color ringColor = new Vector4(colorVec.x, colorVec.y, 0f, 0.5f);
        shader.SetColor("_Color", ringColor);
        shader.SetColor("_EmissionColor", ringLightColor);
        torus.NewMesh();
    }

    public override void Refresh() {
        transform.localPosition = new Vector3(0,0,0);
    }
}
