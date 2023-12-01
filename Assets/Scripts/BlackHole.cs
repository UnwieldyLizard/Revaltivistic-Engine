using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHole : PhysicalObject
{
    [SerializeField] GameObject AccretionRingPrefab;
    [SerializeField] GameObject blackHole;
    private AccretionRing[] diskRings;
    
    void Awake(){
        this.points = new List<Quaternion>(new Quaternion[0]);
    }

    public override void SetMass(double mass) {
        this.mass = mass;
        blackHole.transform.localScale = new Vector3((float)(2*mass), (float)(2*mass), (float)(2*mass));
    }

    public void InitDisk() {
        int diskNum = 12;
        double diskThickness = mass/4;
        diskRings = new AccretionRing[diskNum];
        for (int i = 0; i < diskRings.Length; i++){
            GameObject ring = Instantiate(AccretionRingPrefab, transform);
            
            diskRings[i] = ring.GetComponent<AccretionRing>();
            diskRings[i].SetMass(1);
            diskRings[i].SetCharge(0);
            string name = "Accretion disk of " + this.displayName;
            diskRings[i].SetName(name);
            diskRings[i].AttachEvolver(this.dynamicsEvolver);
            diskRings[i].Initialize(this, 6*mass+(i)*2*diskThickness, diskThickness);
            diskRings[i].Refresh();
        }
    }


}
