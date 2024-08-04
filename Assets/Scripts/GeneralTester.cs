using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GeneralTester : MonoBehaviour
{
    public UnityEvent TriggerEnterEvent;
    private string m_testStr;
    public string TestStrProp
    {
        get;
        private set;
    }
    public string toSetTest;


    [ContextMenu("Print Test")]
    public void PrintTest()
    {
        print(TestStrProp);
       
    }

    [ContextMenu("Set Test")]
    public void SetTest()
    {
        TestStrProp = toSetTest;
    }

    private void OnTriggerEnter(Collider other)
    {
        TriggerEnterEvent.Invoke();
        gameObject.transform.parent = Camera.main.transform;
    }
}
