using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class JumpGame : MonoBehaviour {
    class Pad { public Transform root; public float size; }
    readonly List<Pad> pads=new List<Pad>();
    Transform pawn,body; Camera cam; Text scoreLabel,hint,bestLabel; Image meter;
    int current,score,combo,best; float charge,flight,fall; bool charging,flying,dead;
    Vector3 start,end,focus; Font font; Material pawnMat;
    readonly Color[] colors={new Color(.98f,.68f,.31f),new Color(.32f,.68f,.65f),new Color(.75f,.57f,.79f),new Color(.9f,.48f,.40f)};
    void Start(){
        Application.targetFrameRate=60;
        font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        cam=new GameObject("Isometric camera").AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=6.5f;
        cam.backgroundColor=new Color(.91f,.92f,.86f);cam.clearFlags=CameraClearFlags.SolidColor;
        cam.transform.rotation=Quaternion.Euler(36,45,0);
        var light=new GameObject("Sun").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.5f;
        light.transform.rotation=Quaternion.Euler(50,-30,0);light.shadows=LightShadows.Soft;
        RenderSettings.ambientLight=new Color(.7f,.75f,.8f);
        MakeUI();ResetGame();
    }
    Material Mat(Color c){var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=c;return m;}
    Transform Shape(string name,PrimitiveType type,Transform parent,Vector3 pos,Vector3 scale,Material mat){
        var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;return g.transform;
    }
    void AddPad(Vector3 pos){
        int n=pads.Count;float size=n<2?2:Random.Range(1.5f,2.1f);
        var root=new GameObject("Platform "+n).transform;root.position=pos;
        var mat=Mat(colors[n%colors.Length]);
        Shape("Block",PrimitiveType.Cube,root,new Vector3(0,-.55f,0),new Vector3(size,1.1f,size),mat);
        Shape("Center target",PrimitiveType.Cylinder,root,new Vector3(0,.012f,0),new Vector3(.48f,.018f,.48f),Mat(new Color(1,.96f,.82f)));
        if(n%3==1){
            Shape("Band",PrimitiveType.Cube,root,new Vector3(0,-.3f,0),new Vector3(size+.012f,.12f,size+.012f),Mat(new Color(1,.94f,.78f)));
        }
        pads.Add(new Pad{root=root,size=size});
    }
    void NextPad(){
        var prev=pads[pads.Count-1].root.position;
        AddPad(prev+(Random.value>.5f?Vector3.right:Vector3.forward)*Random.Range(2.65f,3.8f));
    }
    void ResetGame(){
        foreach(var p in pads)Destroy(p.root.gameObject);pads.Clear();if(pawn)Destroy(pawn.gameObject);
        current=score=combo=0;charge=flight=fall=0;charging=flying=dead=false;
        AddPad(Vector3.zero);NextPad();NextPad();
        pawn=new GameObject("Jumper").transform;pawn.position=Vector3.zero;
        pawnMat=Mat(new Color(.15f,.20f,.25f));
        body=Shape("Body",PrimitiveType.Capsule,pawn,new Vector3(0,.46f,0),new Vector3(.35f,.45f,.35f),pawnMat);
        Shape("Head",PrimitiveType.Sphere,pawn,new Vector3(0,1,0),Vector3.one*.43f,pawnMat);
        Shape("Collar",PrimitiveType.Cylinder,pawn,new Vector3(0,.78f,0),new Vector3(.3f,.04f,.3f),Mat(new Color(1,.8f,.35f)));
        best=PlayerPrefs.GetInt("JumperBest",0);focus=(pads[0].root.position+pads[1].root.position)*.5f;
        UpdateLabels();hint.text="HOLD TO CHARGE  /  RELEASE TO JUMP";Debug.Log("JUMPER_RESET");
    }
    void Update(){
        bool down=(Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame)||(Keyboard.current!=null&&Keyboard.current.spaceKey.wasPressedThisFrame)||(Touchscreen.current!=null&&Touchscreen.current.primaryTouch.press.wasPressedThisFrame);
        bool up=(Mouse.current!=null&&Mouse.current.leftButton.wasReleasedThisFrame)||(Keyboard.current!=null&&Keyboard.current.spaceKey.wasReleasedThisFrame)||(Touchscreen.current!=null&&Touchscreen.current.primaryTouch.press.wasReleasedThisFrame);
        if(dead){
            fall+=Time.deltaTime;pawn.position+=Vector3.down*Time.deltaTime*fall*9;pawn.Rotate(0,0,Time.deltaTime*150);
            if(down&&fall>.5f)ResetGame();
        }else if(flying){
            flight+=Time.deltaTime/.62f;float t=Mathf.Clamp01(flight);
            pawn.position=Vector3.Lerp(start,end,t)+Vector3.up*(4*2.2f*t*(1-t));
            pawn.rotation=Quaternion.AngleAxis(t*360,Vector3.Cross(Vector3.up,end-start).normalized);
            if(t>=1){flying=false;pawn.rotation=Quaternion.identity;Land();}
        }else{
            if(down){charging=true;charge=0;}
            if(charging){charge=Mathf.Min(1,charge+Time.deltaTime/.95f);pawn.localScale=new Vector3(1+charge*.25f,1-charge*.32f,1+charge*.25f);}
            if(up&&charging){
                charging=false;flying=true;flight=0;start=pawn.position;
                end=start+(pads[current+1].root.position-start).normalized*JumpRules.Distance(charge);
                pawn.localScale=Vector3.one;Debug.Log("JUMPER_JUMP charge="+charge.ToString("F2"));
            }
        }
        meter.fillAmount=charging?charge:0;
        var target=(pads[current].root.position+pads[current+1].root.position)*.5f;
        focus=Vector3.Lerp(focus,target,1-Mathf.Exp(-Time.deltaTime*4));
        cam.orthographicSize=Mathf.Max(5.3f,5.5f/Mathf.Max(.65f,cam.aspect));
        cam.transform.position=focus+new Vector3(-9,11,-9);
        cam.transform.LookAt(focus);
    }
    void Land(){
        var p=pads[current+1];
        if(JumpRules.Lands(end,p.root.position,p.size)){
            bool perfect=JumpRules.Perfect(end,p.root.position);combo=perfect?combo+1:0;
            score+=JumpRules.Points(perfect,combo);current++;NextPad();
            hint.text=perfect?"PERFECT  +"+JumpRules.Points(true,combo):"NICE JUMP  +1";
            if(score>best){best=score;PlayerPrefs.SetInt("JumperBest",best);PlayerPrefs.Save();}
            if(current>4)pads[current-5].root.gameObject.SetActive(false);
            UpdateLabels();Debug.Log("JUMPER_LANDED score="+score);
        }else if(JumpRules.Lands(end,pads[current].root.position,pads[current].size)){hint.text="A LITTLE LONGER — HOLD TO CHARGE";}
        else{dead=true;fall=0;hint.text="MISSED!  CLICK / SPACE TO TRY AGAIN";Debug.Log("JUMPER_GAME_OVER score="+score);}
    }
    void UpdateLabels(){scoreLabel.text=score.ToString("00");bestLabel.text="BEST  "+best.ToString("00");}
    Text Label(Transform parent,string value,int size,Vector2 anchor,Vector2 pos,Vector2 dimensions,TextAnchor alignment){
        var go=new GameObject(value,typeof(RectTransform));go.transform.SetParent(parent,false);
        var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=pos;r.sizeDelta=dimensions;
        var t=go.AddComponent<Text>();t.font=font;t.fontSize=size;t.color=new Color(.15f,.22f,.25f);t.alignment=alignment;t.text=value;return t;
    }
    void MakeUI(){
        var go=new GameObject("HUD");var canvas=go.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=go.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1100,750);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
        Label(go.transform,"POCKET / ARCADE",16,new Vector2(0,1),new Vector2(150,-40),new Vector2(260,30),TextAnchor.MiddleLeft);
        Label(go.transform,"JUMP JUMP",30,new Vector2(0,1),new Vector2(150,-75),new Vector2(260,45),TextAnchor.MiddleLeft);
        bestLabel=Label(go.transform,"BEST 00",18,new Vector2(1,1),new Vector2(-110,-42),new Vector2(180,35),TextAnchor.MiddleRight);
        scoreLabel=Label(go.transform,"00",76,new Vector2(.5f,1),new Vector2(0,-95),new Vector2(250,100),TextAnchor.MiddleCenter);
        hint=Label(go.transform,"",20,new Vector2(.5f,0),new Vector2(0,72),new Vector2(900,45),TextAnchor.MiddleCenter);
        Label(go.transform,"MOUSE / TOUCH / SPACE     •     LAND IN THE CENTER FOR COMBOS",13,new Vector2(.5f,0),new Vector2(0,30),new Vector2(900,25),TextAnchor.MiddleCenter);
        var bar=new GameObject("Charge",typeof(RectTransform));bar.transform.SetParent(go.transform,false);
        var rt=bar.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(.5f,0);rt.anchoredPosition=new Vector2(0,110);rt.sizeDelta=new Vector2(250,8);
        meter=bar.AddComponent<Image>();meter.color=new Color(.95f,.55f,.22f);meter.type=Image.Type.Filled;meter.fillMethod=Image.FillMethod.Horizontal;
        // Filled UI images require a sprite.
        meter.sprite=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),Vector2.one*.5f);
    }
}
