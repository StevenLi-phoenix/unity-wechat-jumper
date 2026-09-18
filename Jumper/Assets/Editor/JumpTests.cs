using System;
using UnityEngine;
public static class JumpTests {
    static void Check(bool v,string n){if(!v)throw new Exception("FAIL "+n);Debug.Log("PASS "+n);}
    public static void Run(){
        Check(Mathf.Approximately(JumpRules.Distance(0),0.65f),"minimum jump");
        Check(Mathf.Approximately(JumpRules.Distance(1),4.65f),"full jump");
        Check(JumpRules.Distance(10)==JumpRules.Distance(1),"charge capped");
        Check(JumpRules.Distance(-1)==JumpRules.Distance(0),"negative charge clamped");
        Check(JumpRules.Lands(new Vector3(.8f,0,.8f),Vector3.zero,2),"square corner supported");
        Check(!JumpRules.Lands(new Vector3(1.1f,0,0),Vector3.zero,2),"edge miss");
        Check(JumpRules.Perfect(new Vector3(.1f,0,.1f),Vector3.zero),"center bonus");
        Check(!JumpRules.Perfect(new Vector3(.4f,0,0),Vector3.zero),"off center normal");
        Check(JumpRules.Points(true,3)==6 && JumpRules.Points(false,3)==1,"combo scoring");
        Debug.Log("JUMPER_TESTS_PASSED: 9");
    }
}
