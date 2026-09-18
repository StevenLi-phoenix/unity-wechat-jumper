using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class JumpGame : MonoBehaviour {
    readonly JumpSession session=new JumpSession();
    JumpWorld world;JumpHud hud;JumpAudio sound;Camera cam;
    Vector3 focus,start,end;float flight,fall,shake,elapsed;int best,initialBest;bool guide=true,qa;
    void Start(){
        Application.targetFrameRate=60;
#if !UNITY_WEBGL
        qa=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--qa")>=0;
#endif
        Application.runInBackground=qa;
        cam=new GameObject("Postcard camera").AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=5.5f;cam.nearClipPlane=.1f;cam.farClipPlane=180;
        cam.backgroundColor=JumpWorld.Blue;cam.clearFlags=CameraClearFlags.SolidColor;cam.gameObject.AddComponent<AudioListener>();
        var sun=new GameObject("Afternoon sunshine").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=.82f;sun.color=new Color(1,.94f,.84f);sun.shadows=LightShadows.Soft;sun.shadowStrength=.55f;sun.transform.rotation=Quaternion.Euler(48,-30,0);
        RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.32f,.37f,.46f);
        RenderSettings.fog=true;RenderSettings.fogColor=JumpWorld.Blue;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=25;RenderSettings.fogEndDistance=95;
        world=new GameObject("Toybox rooftops").AddComponent<JumpWorld>();world.Initialize();
        hud=new GameObject("Postcard HUD").AddComponent<JumpHud>();hud.Initialize();
        sound=gameObject.AddComponent<JumpAudio>();sound.Initialize();
        best=qa?0:PlayerPrefs.GetInt("JumperBest",0);guide=PlayerPrefs.GetInt("JumperGuide",1)==1;
        hud.StartClicked=StartRun;hud.PauseClicked=TogglePause;hud.ResumeClicked=TogglePause;
        hud.MuteClicked=ToggleMute;hud.AssistClicked=()=>{guide=!guide;if(!qa){PlayerPrefs.SetInt("JumperGuide",guide?1:0);PlayerPrefs.Save();}hud.Settings(sound.Muted,guide);};
        hud.Settings(sound.Muted,guide);
        focus=(world.CurrentPad.Position+world.Next.Position)*.5f;
        UpdateCamera(1);Debug.Log("JUMPER_READY v1.1");
#if !UNITY_WEBGL
        if(qa)StartCoroutine(QA());
#endif
    }
    void StartRun(){
        world.Reset();session.StartRun();flight=fall=0;initialBest=best;focus=(world.CurrentPad.Position+world.Next.Position)*.5f;
        sound.Begin();hud.Show(session.Phase);hud.Toast("Let's go!");Debug.Log("JUMPER_RUN_START");
    }
    void ToggleMute(){sound.Toggle();hud.Settings(sound.Muted,guide);}
    void TogglePause(){
        if(session.Phase==JumpPhase.Paused){session.Resume();sound.Pause(false);}
        else {session.Pause();sound.Pause(session.Phase==JumpPhase.Paused);}
        hud.Show(session.Phase);
    }
    void OnApplicationFocus(bool focused){
        if(!focused&&session!=null){session.CancelCharge();if(sound)sound.Charge(0,false);if(world&&world.Model)world.Model.localScale=Vector3.one;}
    }
    void Update(){
        float dt=Mathf.Min(Time.deltaTime,.05f);elapsed+=dt;
        var kb=Keyboard.current;var mouse=Mouse.current;var touch=Touchscreen.current;
        bool touchDown=touch!=null&&touch.primaryTouch.press.wasPressedThisFrame;
        bool pointerDown=touchDown||(mouse!=null&&mouse.leftButton.wasPressedThisFrame);
        bool pointerUp=(touch!=null&&touch.primaryTouch.press.wasReleasedThisFrame)||(mouse!=null&&mouse.leftButton.wasReleasedThisFrame);
        Vector2 pointer=touchDown?touch.primaryTouch.position.ReadValue():mouse!=null?mouse.position.ReadValue():Vector2.zero;
        bool space=kb!=null&&kb.spaceKey.wasPressedThisFrame,release=kb!=null&&kb.spaceKey.wasReleasedThisFrame;
        if(!qa){
            if(kb!=null&&kb.mKey.wasPressedThisFrame)ToggleMute();
            if(kb!=null&&kb.escapeKey.wasPressedThisFrame)TogglePause();
            bool consumed=pointerDown&&hud.HandlePress(pointer);
            if((space||(kb!=null&&kb.enterKey.wasPressedThisFrame))&&(session.Phase==JumpPhase.Title||session.Phase==JumpPhase.Result)){StartRun();space=false;}
            if(!consumed&&(pointerDown||space)&&session.Phase==JumpPhase.Ready)session.BeginCharge();
            if((pointerUp||release)&&session.Phase==JumpPhase.Charging)Launch();
        }
        if(session.Phase==JumpPhase.Charging){
            session.TickCharge(dt);float q=session.Charge;
            world.Model.localScale=new Vector3(1+q*.3f,1-q*.35f,1+q*.3f);
            world.Model.localPosition=new Vector3(Mathf.Sin(elapsed*70)*q*.018f,0,0);
        }else if(session.Phase==JumpPhase.Flight){
            flight+=dt/.63f;float t=Mathf.Clamp01(flight);
            world.Pawn.position=Vector3.Lerp(start,end,t)+Vector3.up*(4*2.35f*t*(1-t));
            world.Model.rotation=Quaternion.AngleAxis(360*t,Vector3.Cross(Vector3.up,end-start).normalized);
            if(t>=1){world.Model.rotation=Quaternion.identity;Land();}
        }else if(session.Phase==JumpPhase.Falling){
            fall+=dt;world.Pawn.position+=Vector3.down*(dt*(2+fall*8));world.Model.Rotate(0,0,dt*170);
            if(fall>.8f){session.Finish();hud.Results(session,session.Score>initialBest);Debug.Log("JUMPER_RESULT score="+session.Score);}
        }else if(session.Phase==JumpPhase.Ready||session.Phase==JumpPhase.Title){
            world.Model.localScale=Vector3.Lerp(world.Model.localScale,Vector3.one,dt*12);
            world.Model.localPosition=new Vector3(0,Mathf.Sin(elapsed*3)*.022f,0);
        }
        bool paused=session.Phase==JumpPhase.Paused;
        sound.Charge(session.Charge,session.Phase==JumpPhase.Charging);
        world.Animate(dt,focus,paused);
        if(!paused)UpdateCamera(dt);
        float target=(Vector3.Distance(world.Pawn.position,world.Next.Position)-.65f)/4;
        hud.Refresh(session,best,target,guide,paused?0:dt);
    }
    void Launch(){
        if(session.Phase!=JumpPhase.Charging)return;
        session.Release();flight=0;start=world.Pawn.position;
        end=start+(world.Next.Position-start).normalized*JumpRules.Distance(session.Charge);
        world.Model.localScale=Vector3.one;world.Model.localPosition=Vector3.zero;
        world.Pawn.GetComponent<TrailRenderer>().emitting=true;
        sound.Jump();Debug.Log("JUMPER_JUMP charge="+session.Charge.ToString("F3"));
    }
    void Land(){
        world.Pawn.GetComponent<TrailRenderer>().emitting=false;
        var p=world.Next;
        if(JumpRules.Lands(end,p.Position,p.Size)){
            bool perfect=JumpRules.Perfect(end,p.Position);int oldDistrict=session.District;
            session.Land(perfect,perfect);world.Advance(perfect);world.Burst(end,perfect);shake=perfect?.12f:.05f;
            sound.Land(perfect,session.Combo);
            hud.Toast(perfect?(session.Combo>1?"Perfect x"+session.Combo:"Perfect!")+"  +"+session.LastAward:"Nice!  +1",perfect);
            if(oldDistrict!=session.District)hud.Toast("New neighborhood!",true);
            if(session.Score>best){best=session.Score;if(!qa){PlayerPrefs.SetInt("JumperBest",best);PlayerPrefs.Save();}}
            Debug.Log("JUMPER_LANDED score="+session.Score+" hops="+session.Hops+" perfect="+perfect);
        }else if(JumpRules.Lands(end,world.CurrentPad.Position,world.CurrentPad.Size)){
            session.LandBack();sound.Land(false,0);hud.Toast("A little more charge!");
        }else{
            session.Miss();fall=0;sound.Miss();shake=.2f;hud.Toast("So close!");Debug.Log("JUMPER_MISS");
        }
    }
    void UpdateCamera(float dt){
        Vector3 target=(world.CurrentPad.Position+world.Next.Position)*.5f;
        if(session.Phase==JumpPhase.Title)target+=new Vector3(-1,0,1)*1.65f;
        focus=Vector3.Lerp(focus,target,1-Mathf.Exp(-dt*4));
        cam.orthographicSize=Mathf.Max(5.4f,5.7f/Mathf.Max(.65f,cam.aspect));
        cam.transform.position=focus+new Vector3(-10,12,-10);cam.transform.LookAt(focus);
        if(shake>0){cam.transform.position+=Random.insideUnitSphere*shake;shake=Mathf.Max(0,shake-dt*.7f);}
    }
#if !UNITY_WEBGL
    void Capture(string stage){ScreenCapture.CaptureScreenshot("/tmp/jumper-"+stage+(Screen.width<Screen.height?"-portrait":"")+".png");}
    IEnumerator QA(){
        yield return new WaitForSeconds(1);
        Capture("title");yield return new WaitForSeconds(.3f);
        StartRun();yield return new WaitForSeconds(.3f);
        for(int i=0;i<15;i++){
            session.BeginCharge();
            float q=(Vector3.Distance(world.Pawn.position,world.Next.Position)-.65f)/4;
            session.TickCharge(q*.95f);Launch();
            float timeout=0;while(session.Phase==JumpPhase.Flight&&timeout<3){timeout+=Time.deltaTime;yield return null;}
            if(session.Hops!=i+1){Debug.LogError("QA_FAILED hop "+i);Application.Quit(2);yield break;}
            if(i==2){Capture("gameplay");yield return new WaitForSeconds(.2f);}
            yield return new WaitForSeconds(.15f);
        }
        TogglePause();yield return new WaitForSeconds(.2f);TogglePause();
        if(session.Phase!=JumpPhase.Ready||world.ActivePads>9){Debug.LogError("QA_FAILED pause or platform bound");Application.Quit(2);yield break;}
        session.BeginCharge();session.TickCharge(1);Launch();
        // Force a known miss after launching to verify result transition independently of route geometry.
        end=world.Next.Position+Vector3.right*10;
        float wait=0;while(session.Phase!=JumpPhase.Result&&wait<4){wait+=Time.deltaTime;yield return null;}
        if(session.Phase!=JumpPhase.Result){Debug.LogError("QA_FAILED result");Application.Quit(2);yield break;}
        Capture("result");yield return new WaitForSeconds(.3f);
        StartRun();if(session.Score!=0||session.Hops!=0){Debug.LogError("QA_FAILED retry");Application.Quit(2);yield break;}
        Debug.Log("JUMPER_RUNTIME_QA_PASSED: 15 landings, bounded platforms, pause, miss, result, retry");
        yield return new WaitForSeconds(.3f);Application.Quit(0);
    }
#endif
}
