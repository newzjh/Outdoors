using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoShared;

public class LayerPanel : BasePanel
{
    private Button btoogle;
    private Transform tPanel2;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        btoogle = transform.Find("ButtonToggle").GetComponent<Button>();
        btoogle.onClick.AddListener(
            () =>
            {
                OnToggle();
            }
        );

        tPanel2 = transform.Find("LayerPanel2");
        //Button[] bs=tPanel2.GetComponentsInChildren<Button>(true);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnToggle()
    {
        tPanel2.gameObject.SetActive(!tPanel2.gameObject.activeSelf);
    }

    public void OnSelectLayer(int index)
    {
        ma.goMap.satelliteType = (GoMap.GOMap.GOSatelliteType)index;
        loc.SetLocation(loc.currentLocation);
        tPanel2.gameObject.SetActive(false);
    }
}
