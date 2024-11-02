using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoShared;

public class InfoPanel : BasePanel
{
    private Text coordtext = null;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        coordtext = GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        Coordinates coord = Coordinates.convertVectorToCoordinates(ma.transform.position);
        coordtext.text = string.Format("  {0:0.00000},{1:0.00000} << {2:0.00000}", coord.longitude, coord.latitude, coord.altitude);
    }
}
