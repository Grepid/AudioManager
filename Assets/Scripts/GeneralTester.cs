using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralTester : MonoBehaviour
{
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
}
