using System.Collections;
using System.Collections.Generic;
using GoShared;
using UnityEngine;
using UnityEngine.UI;

public class NavPanel : BasePanel
{
    private GOOrbit goOrbit;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        goOrbit = Camera.main.GetComponent<GOOrbit>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnButton1()
    {
        LocationManager loc = GameObject.FindAnyObjectByType<LocationManager>(FindObjectsInactive.Include);
        loc.motionMode = LocationManagerEnums.MotionMode.GPS;
        loc.SetOrigin(loc.worldOrigin);
        goOrbit.currentAngle = 0.0f;
        goOrbit.updateOrbit(true);
        
    }

    public void OnButton2()
    {
        goOrbit.toggle2D = !goOrbit.toggle2D;
        goOrbit.updateOrbit(false);
    }

    public void OnButton3()
    {
        goOrbit.distance += 100;
        goOrbit.updateOrbit(false);
    }

    public void OnButton4()
    {
        goOrbit.distance -= 100;
        goOrbit.updateOrbit(false);
    }
}
