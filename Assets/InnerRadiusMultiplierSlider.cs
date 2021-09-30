using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InnerRadiusMultiplierSlider : MonoBehaviour
{
    [SerializeField] private Slider agentSliderHandle;

    [SerializeField] private Text agentText;

    // Start is called before the first frame update
    void Start()
    {
        if (agentSliderHandle != null)
        {
            agentSliderHandle.onValueChanged.AddListener((v)=>{
                agentText.text = v.ToString("0.0");
            });              
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
