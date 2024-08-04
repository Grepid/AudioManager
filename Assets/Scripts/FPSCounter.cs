using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    private float elapsedTime;
    private float elapsedTime2;
    private float lowest;
    private List<float> fpss = new List<float>();
    TextMeshProUGUI txt;
    void Start()
    {
        txt = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        elapsedTime2 += Time.deltaTime;
        fpss.Add(Time.deltaTime);
        if (elapsedTime2 > 0.5f)
        {
            lowest = fpss.Max();
            fpss.Clear();
            elapsedTime2 = 0;
        }
        

        if (elapsedTime > 0.1f)
        {
            txt.text = Mathf.Round((1 / Time.smoothDeltaTime)).ToString() + "\n Lowest in 1s: " + Mathf.Round((1 / lowest));
            elapsedTime = 0;
        }
    }
}
