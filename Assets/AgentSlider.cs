using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgentSlider : MonoBehaviour
{
    [SerializeField] private Slider agentSliderHandle;

    [SerializeField] private Text agentText;

    // Start is called before the first frame update
    void Start()
    {
        if (agentSliderHandle != null)
        {
            agentSliderHandle.onValueChanged.AddListener((v)=>{
                agentText.text = v.ToString("0");
                UpdatePolygon();
            });              
        }
    }

    void UpdatePolygon()
    {
        GameObject.FindGameObjectWithTag("polygon");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
