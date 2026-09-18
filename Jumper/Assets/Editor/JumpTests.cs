using System;
using UnityEngine;
public static class JumpTests {
    static int checks;
    static void Check(bool v,string n){if(!v)throw new Exception("FAIL "+n);checks++;Debug.Log("PASS "+n);}
    public static void Run(){
        checks=0;
        float lastGap=2.6f;
        for(int i=0;i<100;i++){
            float gap=JumpRules.PlatformGap(lastGap,(i*37%101)/100f);
            Check(gap>=2.4f&&gap<=4.3f,"route gap within reachable range");
            Check(Mathf.Abs(gap-lastGap)>=.65f,"successive gaps need different timing");
            lastGap=gap;
        }
        float near=2.4f,far=4.3f;
        Check(!JumpRules.Lands(Vector3.right*near,Vector3.right*far,2.2f),"near timing undershoots far platform");
        Check(!JumpRules.Lands(Vector3.right*far,Vector3.right*near,2.2f),"far timing overshoots near platform");
        Check(Mathf.Approximately(JumpRules.Distance(0),.65f),"minimum jump");
        Check(Mathf.Approximately(JumpRules.Distance(1),4.65f),"full jump");
        Check(JumpRules.Distance(10)==JumpRules.Distance(1),"charge capped");
        Check(JumpRules.Distance(-1)==JumpRules.Distance(0),"negative charge clamped");
        Check(JumpRules.Lands(new Vector3(.8f,0,.8f),Vector3.zero,2),"square corner supported");
        Check(!JumpRules.Lands(new Vector3(1.1f,0,0),Vector3.zero,2),"edge miss");
        Check(JumpRules.Perfect(new Vector3(.1f,0,.1f),Vector3.zero),"center bonus");
        Check(!JumpRules.Perfect(new Vector3(.4f,0,0),Vector3.zero),"off center normal");
        Check(JumpRules.Points(true,3)==6&&JumpRules.Points(false,3)==1,"combo scoring");
        var s=new JumpSession();
        Check(s.Phase==JumpPhase.Title,"starts at title");
        s.BeginCharge();Check(s.Phase==JumpPhase.Title,"title cannot charge");
        s.StartRun();Check(s.Phase==JumpPhase.Ready&&s.Score==0&&s.Hops==0,"new run");
        s.Release();Check(s.Phase==JumpPhase.Ready,"release without press ignored");
        s.BeginCharge();s.TickCharge(.475f);Check(Mathf.Abs(s.Charge-.5f)<.001f,"charge uses seconds");
        s.TickCharge(10);Check(s.Charge==1,"overcharge clamp");
        s.Pause();Check(s.Phase==JumpPhase.Paused&&s.Charge==0,"pause cancels charge");
        s.Resume();Check(s.Phase==JumpPhase.Ready,"resume does not auto jump");
        s.BeginCharge();s.TickCharge(.6f);s.Release();Check(s.Phase==JumpPhase.Flight,"release launches");
        s.Land(true,true);Check(s.Score==2&&s.Combo==1&&s.Coins==1&&s.Hops==1,"perfect landing and coin");
        s.Land(true,true);Check(s.Score==2&&s.Hops==1,"duplicate landing ignored");
        Hop(s,true,true);Check(s.Score==6&&s.Combo==2,"second perfect stacks");
        Hop(s,true,true);Check(s.Score==12&&s.BestCombo==3,"third perfect stacks");
        Hop(s,false,false);Check(s.Score==13&&s.Combo==0&&s.Coins==3,"ordinary landing breaks combo");
        s.BeginCharge();s.Release();s.LandBack();Check(s.Phase==JumpPhase.Ready&&s.Hops==4,"same platform does not score");
        s.BeginCharge();s.Release();s.Miss();Check(s.Phase==JumpPhase.Falling,"miss enters falling");
        s.Finish();Check(s.Phase==JumpPhase.Result,"fall finishes run");
        s.StartRun();Check(s.Score==0&&s.Combo==0&&s.Coins==0&&s.Hops==0,"retry resets run stats");
        s.BeginCharge();s.TickCharge(-1);Check(s.Charge==0,"negative time ignored");
        s.CancelCharge();Check(s.Phase==JumpPhase.Ready,"cancel on focus loss");
        for(int i=0;i<12;i++)Hop(s,true,true);
        Check(s.District==1,"new district after twelve hops");
        Check(s.Combo==12&&s.LastAward==12,"bonus capped to keep scores bounded");
        Check(JumpRules.Lands(Vector3.right,Vector3.zero,2),"exact edge boundary");
        Check(!JumpRules.Lands(Vector3.right*1.001f,Vector3.zero,2),"outside edge boundary");
        var tone=JumpAudio.SynthesizeTone(260,.18f,750);
                Check(tone.Length==Mathf.CeilToInt(22050*.18f),"jump sound duration");
                Check(Mathf.Abs(tone[0])<.001f&&Mathf.Abs(tone[tone.Length-1])<.001f,"sound envelope has no edge click");
                float peak=0;foreach(float value in tone){CheckSample(value);peak=Mathf.Max(peak,Mathf.Abs(value));}
                Check(peak>.05f&&peak<.95f,"sound is audible without clipping");
                var music=JumpAudio.SynthesizeMusic();peak=0;foreach(float value in music){CheckSample(value);peak=Mathf.Max(peak,Mathf.Abs(value));}
                Check(music.Length>22050*8&&music.Length<22050*9,"music loop duration");
                Check(peak>.05f&&peak<.95f,"music has safe headroom");
                Debug.Log("JUMPER_TESTS_PASSED: "+checks);
    }
    static void CheckSample(float value){if(float.IsNaN(value)||float.IsInfinity(value))throw new Exception("Non-finite audio sample");}
    static void Hop(JumpSession s,bool perfect,bool coin){s.BeginCharge();s.Release();s.Land(perfect,coin);}
}
