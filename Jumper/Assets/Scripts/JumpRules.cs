using UnityEngine;
public static class JumpRules {
    public static float Distance(float charge)=>.65f+4*Mathf.Clamp01(charge);
    public static bool Lands(Vector3 p,Vector3 c,float size)=>Mathf.Abs(p.x-c.x)<=size*.5f && Mathf.Abs(p.z-c.z)<=size*.5f;
    public static bool Perfect(Vector3 p,Vector3 c)=>new Vector2(p.x-c.x,p.z-c.z).magnitude<.25f;
    public static int Points(bool perfect,int combo)=>perfect?2*Mathf.Max(1,combo):1;
}
