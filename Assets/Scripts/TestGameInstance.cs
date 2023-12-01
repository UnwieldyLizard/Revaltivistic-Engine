using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

public class TestGameInstance : MonoBehaviour
{
    private Camera cam;
    private CameraProjectionCache cameraProjectionCache;
    private bool timePaused;
    private bool manualThrustEnabled;
    private double mouseRotationScaling;
    private double stickRotationScaling;
    private double stickBoostScaling;
    private List<PhysicalObject> physicalObjects;
    private int lockOnTargetIndex;
    [SerializeField] private GameObject planetPrefab;
    [SerializeField] private GameObject speedometerPrefab;
    [SerializeField] private GameObject lockOnPrefab;
    [SerializeField] private GameObject blackHolePrefab;
    private KerrMetric blackHoleGrav;
    //private SchwarzschildMetric blackHoleGrav;
    private Speedometer speedometer;
    private LockOn lockOn;
    private SpacetimeManager spacetimeManager;
    private Quaternion transformer; //the quaternion to be used for frame transformations
    private DynamicsEvolver dynamicsEvolver;
    private Vector2 lastRightClickPos;
    private Vector2 centerOfScreen;

    void Start()
    {
        cam = GetComponent<Camera>();
        cameraProjectionCache = new CameraProjectionCache(cam);
        centerOfScreen = new Vector2(cam.pixelWidth/2, cam.pixelHeight/2);

        mouseRotationScaling = Math.PI / cam.pixelWidth;
        stickRotationScaling = Math.PI / 1000;
        stickBoostScaling = 0.0001;
        physicalObjects = new List<PhysicalObject>();
        transformer = new Quaternion();

        double depth = 0;
        double scale = 1.5;
        double properTimeStep = 10;
        
        GameObject blackHoleObj = Instantiate(blackHolePrefab, transform);
        BlackHole blackHole = blackHoleObj.GetComponent<BlackHole>();
        blackHole.SetCharge(0);
        blackHole.SetMass(1);
        blackHole.SetName("Black Hole");
        blackHole.SetPosition(new Quaternion( new double[] {-scale*(depth+50) - properTimeStep,0,0,scale*(depth+50)}));
        blackHole.SetPosition(new Quaternion( new double[] {-scale*(depth+50),0,0,scale*(depth+50)}));
        blackHole.AttachEvolver(new DynamicsEvolver(new MinkowskiMetric(), new FreeParticleLagrangian(), new double[] {double.Parse("1e-6"), double.Parse("1e-6")}));
        //blackHole.InitDisk();
        blackHole.Refresh();
        physicalObjects.Add(blackHole);

        blackHoleGrav = new KerrMetric(blackHole, 0);
        //blackHoleGrav.Initialize();
        //blackHoleGrav = new SchwarzschildMetric(blackHole);

        Planet[] planets = new Planet[8];
        for (int i = 0; i < planets.Length; i++){
            GameObject planet = Instantiate(planetPrefab, transform);
            
            planets[i] = planet.GetComponent<Planet>();
            planets[i].SetMass(1);
            planets[i].SetCharge(0);
            string name = "Planet Number: " + i;
            planets[i].SetName(name);
            planets[i].AttachEvolver(new DynamicsEvolver(new MinkowskiMetric(), new FreeParticleLagrangian(), new double[] {double.Parse("1e-6"), double.Parse("1e-6")}));
            planets[i].Refresh();
            physicalObjects.Add(planets[i]);
        }

        planets[0].SetPosition(new Quaternion(new double[] {-Math.Sqrt(scale*scale*2 + (scale+depth)*(scale+depth)) - properTimeStep, scale,scale,scale+depth}));
        planets[0].SetPosition(new Quaternion(new double[] {-Math.Sqrt(scale*scale*2 + (scale+depth)*(scale+depth)), scale,scale,scale+depth}));
        planets[1].SetPosition(new Quaternion(new double[] {-Math.Sqrt(scale*scale*2 + (scale+depth)*(scale+depth)) - properTimeStep, -scale,scale,scale+depth}));
        planets[1].SetPosition(new Quaternion(new double[] {-Math.Sqrt(scale*scale*2 + (scale+depth)*(scale+depth)), -scale,scale,scale+depth}));
        planets[2].SetPosition(new Quaternion(new double[] {-Math.Sqrt(scale*scale*2 + (scale+depth)*(scale+depth)) - properTimeStep, scale,-scale,scale+depth}));
        planets[2].SetPosition(new Quaternion(new double[] {-Math.Sqrt(scale*scale*2 + (scale+depth)*(scale+depth)), scale,-scale,scale+depth}));
        planets[3].SetPosition(new Quaternion(new double[] {-Math.Sqrt(scale*scale*2 + (scale+depth)*(scale+depth)) - properTimeStep, scale,scale,-scale+depth}));
        planets[3].SetPosition(new Quaternion(new double[] {-Math.Sqrt(scale*scale*2 + (scale+depth)*(scale+depth)), scale,scale,-scale+depth}));
        planets[4].SetPosition(new Quaternion(new double[] {-Math.Sqrt(scale*scale*2 + (scale+depth)*(scale+depth)) - properTimeStep, -scale,-scale,scale+depth}));
        planets[4].SetPosition(new Quaternion(new double[] {-Math.Sqrt(scale*scale*2 + (scale+depth)*(scale+depth)), -scale,-scale,scale+depth}));
        planets[5].SetPosition(new Quaternion(new double[] {-Math.Sqrt(scale*scale*2 + (scale+depth)*(scale+depth)) - properTimeStep, -scale,scale,-scale+depth}));
        planets[5].SetPosition(new Quaternion(new double[] {-Math.Sqrt(scale*scale*2 + (scale+depth)*(scale+depth)), -scale,scale,-scale+depth}));
        planets[6].SetPosition(new Quaternion(new double[] {-Math.Sqrt(scale*scale*2 + (scale+depth)*(scale+depth)) - properTimeStep, scale,-scale,-scale+depth}));
        planets[6].SetPosition(new Quaternion(new double[] {-Math.Sqrt(scale*scale*2 + (scale+depth)*(scale+depth)), scale,-scale,-scale+depth}));
        planets[7].SetPosition(new Quaternion(new double[] {-Math.Sqrt(scale*scale*2 + (scale+depth)*(scale+depth)) - properTimeStep, -scale,-scale,-scale+depth}));
        planets[7].SetPosition(new Quaternion(new double[] {-Math.Sqrt(scale*scale*2 + (scale+depth)*(scale+depth)), -scale,-scale,-scale+depth}));


        spacetimeManager = new SpacetimeManager(physicalObjects);

        timePaused = true;

        GameObject speedometerObj = Instantiate(speedometerPrefab, transform);
        speedometer = speedometerObj.GetComponent<Speedometer>();
        lockOnTargetIndex = -1;
        speedometer.SetNothingTracked();

        GameObject lockOnObj = Instantiate(lockOnPrefab, transform);
        lockOn = lockOnObj.GetComponent<LockOn>();
        lockOn.SetMarkerPosition(centerOfScreen);
        lockOn.SetNotTracking();
    }

    void Update()
    {
        manualThrustEnabled = true;
        //Time Pausing
        if (Gamepad.current is not null && Gamepad.current.buttonSouth.wasPressedThisFrame) {
            timePaused = !timePaused;
        }
        if (Input.GetKeyDown("space")) {
            timePaused = !timePaused;
        }

        //Speedometer && Flight Assist
        if(lockOnTargetIndex >= 0 && physicalObjects[lockOnTargetIndex] is not null) {
            Vector2 screenPoint = cameraProjectionCache.WorldToScreenPoint(physicalObjects[lockOnTargetIndex].transform.position);
            if (!float.IsNaN(screenPoint.x) && !float.IsNaN(screenPoint.y) && physicalObjects[lockOnTargetIndex].position.v3.z > 0) {
                lockOn.SetMarkerPosition(screenPoint);
            }
            Vector3 targetVelocity = physicalObjects[lockOnTargetIndex].GetVelocity();
            speedometer.UpdateDisplay(physicalObjects[lockOnTargetIndex].displayName, targetVelocity);
            
            if (!timePaused && Gamepad.current is not null && Gamepad.current.dpad.down.isPressed) {
                manualThrustEnabled = false;
                targetVelocity = Vector3.Normalize(targetVelocity);
                transformer.InitHyperbolic(stickBoostScaling, new double[] {targetVelocity.x, targetVelocity.y, targetVelocity.z});
                spacetimeManager.BoostFrame(transformer);
            }

            if (Gamepad.current is not null && Gamepad.current.rightStickButton.wasPressedThisFrame) {
                lockOnTargetIndex = -1;
                lockOn.SetMarkerPosition(centerOfScreen);
                lockOn.SetNotTracking();
                speedometer.SetNothingTracked();
            }
        } else if (lockOnTargetIndex == -1) {
            if (Gamepad.current is not null && Gamepad.current.rightStickButton.wasPressedThisFrame) {
                double lockOnClickCutoff = 0.05 * cam.pixelWidth;
                double clickError;
                for (int k = 0; k < physicalObjects.Count; k++) {
                    clickError = (cameraProjectionCache.WorldToScreenPoint(physicalObjects[k].transform.position) - centerOfScreen).magnitude;
                    if (clickError < lockOnClickCutoff && physicalObjects[k].position.v[2] > 0) {
                        lockOnClickCutoff = clickError;
                        lockOnTargetIndex = k;
                    }
                }
                if (lockOnTargetIndex >= 0) {
                    lockOn.SetTracking();
                }
            }
        }

        //Rotation Controller
        if (Gamepad.current is not null) {   
            Vector2 rightStickDrag = Gamepad.current.rightStick.ReadValue();
            if (rightStickDrag != new Vector2(0,0)) {
                double[] rotAxis = new double[3] {rightStickDrag.y/rightStickDrag.magnitude, -rightStickDrag.x/rightStickDrag.magnitude, 0};
                transformer.InitPolar(1, rightStickDrag.magnitude * stickRotationScaling, rotAxis);
                spacetimeManager.RotateFrame(transformer);
            }
            if (Gamepad.current.leftShoulder.isPressed) {
                transformer.InitPolar(1, -1 * stickRotationScaling, new double[] {0, 0, 1});
                spacetimeManager.RotateFrame(transformer);
            }
            if (Gamepad.current.rightShoulder.isPressed) {
                transformer.InitPolar(1, stickRotationScaling, new double[] {0, 0, 1});
                spacetimeManager.RotateFrame(transformer);
            }
        }

        //Rotation Mouse-keyBoard
        if (Input.GetMouseButtonDown(1)) {
            lastRightClickPos = Input.mousePosition;
        }
        if (Input.GetMouseButton(1) && ((Vector2)Input.mousePosition != lastRightClickPos)) {
            Vector2 drag = (Vector2)Input.mousePosition - lastRightClickPos;
            double[] rotAxis = new double[3] {-drag.y/drag.magnitude, drag.x/drag.magnitude, 0};
            transformer.InitPolar(1, drag.magnitude * mouseRotationScaling, rotAxis);
            spacetimeManager.RotateFrame(transformer);
            lastRightClickPos = Input.mousePosition;
        }

        if (!timePaused) {
            //Boost Controller
            if (Gamepad.current is not null && manualThrustEnabled) {
                Vector2 leftStickDrag = Gamepad.current.leftStick.ReadValue();
                if (leftStickDrag != new Vector2(0,0)) {
                    Vector3 boost = Vector3.Normalize(new Vector3(leftStickDrag.x, 0, leftStickDrag.y));
                    transformer.InitHyperbolic(leftStickDrag.magnitude * stickBoostScaling, new double[] {boost.x, boost.y, boost.z});
                    spacetimeManager.BoostFrame(transformer);
                }
                if (Gamepad.current.leftTrigger.isPressed) {
                    transformer.InitHyperbolic(stickBoostScaling, new double[] {0, -1, 0});
                    spacetimeManager.BoostFrame(transformer);
                }
                if (Gamepad.current.rightTrigger.isPressed) {
                    transformer.InitHyperbolic(stickBoostScaling, new double[] {0, 1, 0});
                    spacetimeManager.BoostFrame(transformer);
                }
            }

            //Boost Mouse-Keyboard
            if (Input.GetKeyDown("b") && manualThrustEnabled) {
                Quaternion booster = new Quaternion();
                booster.InitHyperbolic((Math.PI/10), new double[] {0, 0, 1});
                spacetimeManager.BoostFrame(booster);
            }

            //Time Evolution
            transformer.Init(new double[] {0.05,0,0,0});
            //transformer.Init(new double[] {1,0,0,0});
            //blackHoleGrav.Initialize();
            spacetimeManager.TranslateFrame(transformer);
        }
    }
}
