using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class JumpHud : MonoBehaviour {
    class Hit {public RectTransform Rect;public Action Click;}
    readonly List<Hit> hits=new List<Hit>();
    TMP_FontAsset display,body;Sprite rounded,solid;
    Canvas canvas;CanvasScaler scaler;JumpPhase visiblePhase;Transform title,result,pause,playHud;
    TMP_Text score,best,coins,combo,district,toast,instructions,chargeLabel,resultScore,resultStats,resultTitle,resultMedal,muteLabel,assistLabel;
    Image charge,aim,progress;float toastTime,scorePulse;
    public Action StartClicked,PauseClicked,ResumeClicked,MuteClicked,AssistClicked;
    public void Initialize(){
        display=Resources.Load<TMP_FontAsset>("Fonts/DisplaySDF");body=Resources.Load<TMP_FontAsset>("Fonts/BodySDF");
        var tex=new Texture2D(32,32);tex.filterMode=FilterMode.Bilinear;
        for(int y=0;y<32;y++)for(int x=0;x<32;x++){
            float dx=Mathf.Max(8-x,x-23),dy=Mathf.Max(8-y,y-23);
            tex.SetPixel(x,y,dx>0&&dy>0&&dx*dx+dy*dy>64?Color.clear:Color.white);
        }
        tex.Apply();rounded=Sprite.Create(tex,new Rect(0,0,32,32),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(9,9,9,9));
        solid=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),Vector2.one*.5f);
        canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        scaler=gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1100,750);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
        title=Panel(transform,"Start card",new Vector2(0,.5f),new Vector2(245,0),new Vector2(430,505),JumpWorld.Cream).transform;
        Text(title,"A tiny rooftop adventure",20,new Vector2(0,205),new Vector2(380,30),JumpWorld.Ink);
        var titleText=Text(title,"JUMP\nJUMP!",67,new Vector2(0,95),new Vector2(390,195),JumpWorld.Ink,true);titleText.lineSpacing=.91f;
        Text(title,"Big leaps. Little rooftops.",23,new Vector2(0,-22),new Vector2(380,35),JumpWorld.Ink);
        Text(title,"Hold to charge. Release to fly.\nStick the center. Keep the streak.",20,new Vector2(0,-82),new Vector2(375,65),JumpWorld.Ink);
        Button(title,"Let's jump",new Vector2(0,-166),new Vector2(340,68),JumpWorld.Red,()=>StartClicked?.Invoke(),26);
        Text(title,"Mouse, touch or Space to play",15,new Vector2(0,-224),new Vector2(370,25),JumpWorld.Ink);
        playHud=new GameObject("Run HUD",typeof(RectTransform)).transform;playHud.SetParent(transform,false);Stretch(playHud);
        var s=Panel(playHud,"Score badge",new Vector2(0,1),new Vector2(137,-82),new Vector2(224,124),JumpWorld.Cream).transform;
        Text(s,"Your score",16,new Vector2(0,39),new Vector2(200,25),JumpWorld.Ink);
        score=Text(s,"00",52,new Vector2(0,-8),new Vector2(215,70),JumpWorld.Ink,true);
        best=Text(playHud,"Best 00",18,new Vector2(0,1),new Vector2(135,-167),new Vector2(220,30),JumpWorld.Ink);
        var coinPanel=Panel(playHud,"Coins",new Vector2(1,1),new Vector2(-112,-125),new Vector2(180,58),JumpWorld.Yellow).transform;
        coins=Text(coinPanel,"0 coins",22,Vector2.zero,new Vector2(165,40),JumpWorld.Ink,true);
        district=Text(playHud,"Toybox rooftops",21,new Vector2(.5f,1),new Vector2(0,-45),new Vector2(340,40),JumpWorld.Ink,true);
        var prog=Panel(playHud,"District track",new Vector2(.5f,1),new Vector2(0,-78),new Vector2(230,7),new Color(.15f,.2f,.35f,.15f));
        progress=Panel(prog.transform,"Progress",new Vector2(.5f,.5f),Vector2.zero,new Vector2(230,7),JumpWorld.Red);progress.sprite=solid;progress.type=Image.Type.Filled;progress.fillMethod=Image.FillMethod.Horizontal;
        combo=Text(playHud,"",23,new Vector2(.5f,1),new Vector2(0,-118),new Vector2(350,40),JumpWorld.Red,true);
        var footer=Panel(playHud,"Charge console",new Vector2(.5f,0),new Vector2(0,73),new Vector2(530,102),JumpWorld.Ink).transform;
        instructions=Text(footer,"Hold to charge  •  Release to jump",18,new Vector2(0,26),new Vector2(500,30),JumpWorld.Cream);
        var rail=Panel(footer,"Power rail",new Vector2(.5f,.5f),new Vector2(-35,-12),new Vector2(370,15),new Color(.4f,.48f,.62f));
        charge=Panel(rail.transform,"Charge fill",new Vector2(.5f,.5f),Vector2.zero,new Vector2(370,15),JumpWorld.Yellow);charge.sprite=solid;charge.type=Image.Type.Filled;charge.fillMethod=Image.FillMethod.Horizontal;charge.fillAmount=0;
        aim=Panel(rail.transform,"Sweet spot",new Vector2(0,.5f),Vector2.zero,new Vector2(5,25),JumpWorld.Cream);
        chargeLabel=Text(footer,"0%",17,new Vector2(211,-12),new Vector2(70,30),JumpWorld.Yellow,true);
        Text(playHud,"Space to jump    Esc to pause    M for sound",14,new Vector2(.5f,0),new Vector2(0,14),new Vector2(650,22),JumpWorld.Ink);
        Button(playHud,"II",new Vector2(1,1),new Vector2(-48,-43),new Vector2(56,52),JumpWorld.Cream,()=>PauseClicked?.Invoke(),22);
        muteLabel=Button(transform,"Sound on",new Vector2(1,1),new Vector2(-151,-43),new Vector2(130,52),JumpWorld.Cream,()=>MuteClicked?.Invoke(),16);
        assistLabel=Button(transform,"Guide on",new Vector2(1,0),new Vector2(-92,45),new Vector2(144,43),JumpWorld.Cream,()=>AssistClicked?.Invoke(),16);
        toast=Text(transform,"",38,new Vector2(.5f,.71f),Vector2.zero,new Vector2(740,80),JumpWorld.Red,true);
        toast.outlineWidth=.16f;toast.outlineColor=JumpWorld.Cream;
        pause=Panel(transform,"Pause card",new Vector2(.5f,.5f),Vector2.zero,new Vector2(440,300),JumpWorld.Cream).transform;
        Text(pause,"Take a breather",31,new Vector2(0,86),new Vector2(405,70),JumpWorld.Ink,true);
        Text(pause,"Your rooftops aren't going anywhere.",19,new Vector2(0,20),new Vector2(400,35),JumpWorld.Ink);
        Button(pause,"Keep jumping",new Vector2(0,-72),new Vector2(345,64),JumpWorld.Mint,()=>ResumeClicked?.Invoke(),24);
        result=Panel(transform,"Results card",new Vector2(.5f,.5f),Vector2.zero,new Vector2(465,515),JumpWorld.Cream).transform;
        resultTitle=Text(result,"What a run!",30,new Vector2(0,205),new Vector2(430,55),JumpWorld.Ink,true);
        resultScore=Text(result,"00",90,new Vector2(0,111),new Vector2(420,115),JumpWorld.Red,true);
        resultMedal=Text(result,"Rooftop rookie",24,new Vector2(0,27),new Vector2(430,45),JumpWorld.Ink,true);
        resultStats=Text(result,"",21,new Vector2(0,-39),new Vector2(410,74),JumpWorld.Ink);
        Button(result,"One more jump",new Vector2(0,-151),new Vector2(355,68),JumpWorld.Red,()=>StartClicked?.Invoke(),25);
        Text(result,"Tip: each perfect landing grows your combo.",16,new Vector2(0,-218),new Vector2(440,30),JumpWorld.Ink);
        Show(JumpPhase.Title);
    }
    void LateUpdate(){
        if(!scaler)return;
        bool narrow=Screen.width<(Screen.height*.85f);
        scaler.referenceResolution=narrow?new Vector2(620,1000):new Vector2(1100,750);
        var tr=(RectTransform)title;tr.anchorMin=tr.anchorMax=narrow?Vector2.one*.5f:new Vector2(0,.5f);tr.anchoredPosition=narrow?Vector2.zero:new Vector2(245,0);
        district.rectTransform.anchoredPosition=new Vector2(0,narrow?-213:-45);
        ((RectTransform)progress.transform.parent).anchoredPosition=new Vector2(0,narrow?-247:-78);
        combo.rectTransform.anchoredPosition=new Vector2(0,narrow?-281:-118);
        toast.rectTransform.anchoredPosition=new Vector2(0,narrow?-65:0);
        bool top=narrow&&visiblePhase!=JumpPhase.Title&&visiblePhase!=JumpPhase.Result;
        var anchor=new Vector2(1,top?1:0);var pos=new Vector2(-92,top?-182:45);
        var guideRect=(RectTransform)assistLabel.transform.parent;guideRect.anchorMin=guideRect.anchorMax=anchor;guideRect.anchoredPosition=pos;
        var shade=(RectTransform)transform.Find("Guide on shadow");shade.anchorMin=shade.anchorMax=anchor;shade.anchoredPosition=pos+Vector2.down*5;
    }
    void Stretch(Transform t){var r=(RectTransform)t;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
    RectTransform Rect(Transform parent,string name,Vector2 anchor,Vector2 pos,Vector2 size){
        var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(parent,false);var r=(RectTransform)g.transform;r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=pos;r.sizeDelta=size;return r;
    }
    Image Panel(Transform p,string name,Vector2 a,Vector2 pos,Vector2 size,Color c){
        var r=Rect(p,name,a,pos,size);var i=r.gameObject.AddComponent<Image>();i.sprite=rounded;i.type=Image.Type.Sliced;i.color=c;i.raycastTarget=false;return i;
    }
    TMP_Text Text(Transform p,string text,int size,Vector2 pos,Vector2 dim,Color c,bool heading=false)=>Text(p,text,size,Vector2.one*.5f,pos,dim,c,heading);
    TMP_Text Text(Transform p,string text,int size,Vector2 anchor,Vector2 pos,Vector2 dim,Color c,bool heading=false){
        var r=Rect(p,text,anchor,pos,dim);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=heading?display:body;t.fontSize=size;t.text=text;t.alignment=TextAlignmentOptions.Center;t.overflowMode=TextOverflowModes.Overflow;t.color=c;t.raycastTarget=false;return t;
    }
    TMP_Text Button(Transform p,string text,Vector2 pos,Vector2 size,Color c,Action click,int fontSize)=>Button(p,text,Vector2.one*.5f,pos,size,c,click,fontSize);
    TMP_Text Button(Transform p,string text,Vector2 anchor,Vector2 pos,Vector2 size,Color c,Action click,int fontSize){
        Panel(p,text+" shadow",anchor,pos+Vector2.down*5,size,JumpWorld.Ink);
        var bg=Panel(p,text,anchor,pos,size,c);
        var label=Text(bg.transform,text,fontSize,Vector2.zero,size,c==JumpWorld.Red?JumpWorld.Cream:JumpWorld.Ink,true);
        hits.Add(new Hit{Rect=bg.rectTransform,Click=click});return label;
    }
    public bool HandlePress(Vector2 screen){
        for(int i=hits.Count-1;i>=0;i--){var h=hits[i];if(h.Rect.gameObject.activeInHierarchy&&RectTransformUtility.RectangleContainsScreenPoint(h.Rect,screen)){h.Click();return true;}}
        return false;
    }
    public void Show(JumpPhase phase){
        visiblePhase=phase;
        title.gameObject.SetActive(phase==JumpPhase.Title);result.gameObject.SetActive(phase==JumpPhase.Result);pause.gameObject.SetActive(phase==JumpPhase.Paused);
        playHud.gameObject.SetActive(phase!=JumpPhase.Title&&phase!=JumpPhase.Result);
        if(phase==JumpPhase.Title||phase==JumpPhase.Result)toast.text="";
        foreach(var label in GetComponentsInChildren<TMP_Text>(true))label.ForceMeshUpdate(true);
    }
    public void Settings(bool muted,bool guide){muteLabel.text=muted?"Sound off":"Sound on";assistLabel.text=guide?"Guide on":"Guide off";}
    public void Refresh(JumpSession s,int record,float targetCharge,bool guide,float dt){
        score.text=s.Score.ToString("00");best.text="Best "+record.ToString("00");coins.text=s.Coins+" coins";
        combo.text=s.Combo>=2?s.Combo+" perfects in a row!":"";
        district.text=new[]{"Toybox rooftops","Candy coast","Vinyl avenue","Sunset parade"}[s.District%4];
        progress.fillAmount=(s.Hops%12)/12f;charge.fillAmount=s.Phase==JumpPhase.Charging?s.Charge:0;
        charge.color=s.Charge>.9f?JumpWorld.Red:JumpWorld.Yellow;chargeLabel.text=Mathf.RoundToInt(charge.fillAmount*100)+"%";
        aim.gameObject.SetActive(guide);aim.rectTransform.anchoredPosition=new Vector2(Mathf.Clamp01(targetCharge)*370,0);
        instructions.text=s.Phase==JumpPhase.Flight?"Stick the landing!":guide?"Aim for the white mark • release to jump":"Hold to charge • release to jump";
        toastTime=Mathf.Max(0,toastTime-dt);if(toastTime==0)toast.text="";
        if(scorePulse>0)scorePulse=Mathf.Max(0,scorePulse-dt*4);
        score.transform.localScale=Vector3.one*(1+Mathf.Sin(scorePulse*Mathf.PI)*.18f);
    }
    public void Toast(string message,bool perfect=false){toast.text=message;toast.color=perfect?JumpWorld.Red:JumpWorld.Ink;toastTime=1.7f;scorePulse=1;}
    public void Results(JumpSession s,bool record){
        resultTitle.text=record?"New best!":"What a run!";
        resultScore.text=s.Score.ToString("00");
        resultMedal.text=s.Hops>=36?"Skyline superstar":s.Hops>=20?"Rooftop royalty":s.Hops>=8?"Leap legend":"Rooftop rookie";
        resultStats.text=s.Hops+" rooftops     "+s.Coins+" coins\nBest streak: "+s.BestCombo+" perfect landings";
        Show(JumpPhase.Result);
    }
}
