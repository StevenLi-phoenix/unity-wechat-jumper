using System.Collections.Generic;
using UnityEngine;

public sealed class JumpWorld : MonoBehaviour {
    public static readonly Color Ink=Hex("233457"),Cream=Hex("FFF4DE"),Red=Hex("F45B69"),Yellow=Hex("FFD85A"),Mint=Hex("62D5B4"),Blue=Hex("79CFEA"),Purple=Hex("AA8EE0");
    public sealed class Pad { public Transform Root,Top,Coin;public Vector3 Position;public float Size,Bounce;public int Number; }
    readonly Dictionary<Color,Material> materials=new Dictionary<Color,Material>();
    readonly List<Pad> pads=new List<Pad>();
    readonly List<Transform> clouds=new List<Transform>();
    readonly List<Particle> particles=new List<Particle>();
    class Particle {public Transform T;public Vector3 V;public float Life,Max,Size;}
    Transform scenery,sea,targetRing,shadow;Material trailMaterial;System.Random rng=new System.Random(3481);
    public Transform Pawn {get;private set;}
    public Transform Model {get;private set;}
    public int Current {get;private set;}
    public Pad CurrentPad=>pads[Current];
    public Pad Next=>pads[Current+1];
    public int ActivePads=>pads.Count;
    int generated;
    float elapsed;
    public static Color Hex(string h){ColorUtility.TryParseHtmlString("#"+h,out var c);return c;}
    Material Mat(Color c){
        if(materials.TryGetValue(c,out var m))return m;
        m=new Material(Resources.Load<Material>("Surface"));m.color=c;m.SetFloat("_Smoothness",.28f);materials[c]=m;return m;
    }
    Transform Form(string name,PrimitiveType type,Transform parent,Vector3 p,Vector3 scale,Color color){
        var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;
        g.GetComponent<Renderer>().sharedMaterial=Mat(color);Destroy(g.GetComponent<Collider>());return g.transform;
    }
    Transform Box(Transform parent,Vector3 p,Vector3 s,Color c)=>Form("Toy detail",PrimitiveType.Cube,parent,p,s,c);
    Transform Ball(Transform parent,Vector3 p,Vector3 s,Color c)=>Form("Rounded detail",PrimitiveType.Sphere,parent,p,s,c);
    Transform Disk(Transform parent,Vector3 p,float diameter,float height,Color c)=>Form("Disc",PrimitiveType.Cylinder,parent,p,new Vector3(diameter,height*.5f,diameter),c);
    public void Initialize(){
        scenery=new GameObject("Cloud archipelago").transform;
        sea=Box(null,new Vector3(0,-2,0),new Vector3(250,.2f,250),Blue);
        for(int i=0;i<18;i++){
            var c=new GameObject("Marshmallow cloud").transform;c.SetParent(scenery,false);
            c.localPosition=new Vector3((i%6-2.5f)*5.7f,-.9f+(i%3)*.35f,(i/6-1)*12f+9);
            float k=.6f+(i%4)*.25f;
            for(int n=0;n<3;n++)Ball(c,new Vector3(n*.7f,Mathf.Sin(n)*.15f,0),new Vector3(1.5f,1,1.1f)*k,Cream);
            clouds.Add(c);
        }
        // A miniature skyline beyond the play path, with candy-striped awnings.
        for(int i=0;i<12;i++){
            var pos=new Vector3(i*3-17,-.8f,18+(i%3)*3);
            float h=1.5f+(i%4)*.8f;Color c=new[]{Mint,Red,Purple,Yellow}[i%4];
            Box(scenery,pos+Vector3.up*h*.5f,new Vector3(1.6f,h,1.6f),c);
            Box(scenery,pos+Vector3.up*(h+.1f),new Vector3(1.9f,.25f,1.9f),Cream);
            for(int j=0;j<3;j++)Box(scenery,pos+new Vector3(0,.5f+j*.7f,-.81f),new Vector3(.7f,.3f,.03f),Ink);
        }
        targetRing=new GameObject("Next landing beacon").transform;Ring(targetRing,1.3f,Yellow,.045f);
        shadow=Disk(null,Vector3.zero,.65f,.006f,new Color(.29f,.53f,.61f));
        Reset();
    }
    void Ring(Transform parent,float radius,Color color,float width){
        for(int n=0;n<24;n++){
            float a=n*Mathf.PI*2/24;var t=Box(parent,new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius),new Vector3(width,.016f,radius*.20f),color);
            t.localRotation=Quaternion.Euler(0,-a*Mathf.Rad2Deg,0);
        }
    }
    public void Reset(){
        foreach(var p in pads)Destroy(p.Root.gameObject);pads.Clear();
        foreach(var p in particles)p.T.gameObject.SetActive(false);
        if(Pawn)Destroy(Pawn.gameObject);
        Current=generated=0;rng=new System.Random(3481);
        Add(Vector3.zero);for(int i=0;i<4;i++)AddNext();
        Pawn=new GameObject("Pip the rooftop courier").transform;
        Model=new GameObject("Pip model").transform;Model.SetParent(Pawn,false);
        Form("Suit",PrimitiveType.Capsule,Model,new Vector3(0,.43f,0),new Vector3(.44f,.4f,.44f),Cream);
        Ball(Model,new Vector3(0,.97f,0),Vector3.one*.6f,Ink);
        var face=Ball(Model,new Vector3(-.15f,1,-.21f),new Vector3(.39f,.22f,.17f),Blue);face.localRotation=Quaternion.Euler(0,35,0);
        Ball(Model,new Vector3(-.24f,1.02f,-.25f),Vector3.one*.055f,Cream);
        Ball(Model,new Vector3(-.08f,1.02f,-.35f),Vector3.one*.055f,Cream);
        Disk(Model,new Vector3(0,.72f,0),.48f,.1f,Red);
        var scarf=Box(Model,new Vector3(.3f,.70f,.18f),new Vector3(.48f,.11f,.19f),Red);scarf.localRotation=Quaternion.Euler(0,-25,-12);
        Box(Model,new Vector3(.18f,.48f,.22f),new Vector3(.28f,.35f,.26f),Yellow);
        Ball(Model,new Vector3(-.15f,.09f,-.06f),new Vector3(.23f,.17f,.33f),Ink);
        Ball(Model,new Vector3(.15f,.09f,-.06f),new Vector3(.23f,.17f,.33f),Ink);
        Ball(Model,new Vector3(0,1.3f,0),new Vector3(.07f,.24f,.07f),Red);
        Ball(Model,new Vector3(0,1.44f,0),Vector3.one*.14f,Yellow);
        var trail=Pawn.gameObject.AddComponent<TrailRenderer>();trail.time=.22f;trail.startWidth=.13f;trail.endWidth=0;trail.minVertexDistance=.08f;
        trailMaterial=Mat(Yellow);trail.sharedMaterial=trailMaterial;trail.startColor=Yellow;trail.endColor=Red;trail.emitting=false;
    }
    void AddNext(){
        var prev=pads[pads.Count-1].Position;int n=generated;
        var direction=n==1?Vector3.right:(rng.NextDouble()>.48?Vector3.right:Vector3.forward);
        float previousGap=pads.Count>1?Vector3.Distance(prev,pads[pads.Count-2].Position):2.6f;
        float gap=n==1?2.6f:JumpRules.PlatformGap(previousGap,(float)rng.NextDouble());
        Add(prev+direction*gap);
    }
    void Add(Vector3 pos){
        int n=generated++;float size=n<3?2.1f:Mathf.Lerp(2.05f,1.55f,Mathf.Min(.9f,n/65f))+(float)rng.NextDouble()*.15f;
        var root=new GameObject("Rooftop "+n).transform;root.position=pos;
        var top=new GameObject("Bouncy rooftop").transform;top.SetParent(root,false);
        Color c=new[]{Red,Mint,Yellow,Purple,Blue,Red,Mint,Purple}[n%8];
        Box(top,new Vector3(0,-.59f,0),new Vector3(size,.95f,size),c);
        Box(top,new Vector3(0,-1.12f,0),new Vector3(size*.84f,.18f,size*.84f),Ink);
        Box(top,new Vector3(0,-.09f,0),new Vector3(size+.09f,.18f,size+.09f),Cream);
        // Unique toy objects all preserve a square, level landing surface.
        switch(n%8){
            case 0: // Gift box
                Box(top,new Vector3(0,-.55f,-size*.505f),new Vector3(.23f,.95f,.035f),Yellow);
                Box(top,new Vector3(-size*.505f,-.55f,0),new Vector3(.035f,.95f,.23f),Yellow);
                Box(top,new Vector3(0,.012f,0),new Vector3(.18f,.024f,size),Red);
                Box(top,new Vector3(0,.014f,0),new Vector3(size,.024f,.18f),Red);break;
            case 1: // Record player
                Disk(top,new Vector3(0,.02f,0),size*.78f,.035f,Ink);Disk(top,new Vector3(0,.046f,0),.42f,.016f,Red);
                for(int k=0;k<4;k++)Box(top,new Vector3(-.7f+k*.18f,-.36f,-size*.505f),new Vector3(.06f,.28f,.03f),Cream);
                var arm=Box(top,new Vector3(size*.36f,.08f,.1f),new Vector3(.07f,.08f,.85f),Yellow);arm.localRotation=Quaternion.Euler(0,25,0);break;
            case 2: // Arcade cabinet
                Box(top,new Vector3(0,-.4f,-size*.507f),new Vector3(size*.65f,.45f,.04f),Ink);
                Box(top,new Vector3(0,-.4f,-size*.53f),new Vector3(size*.48f,.28f,.02f),Blue);
                for(int k=0;k<3;k++)Ball(top,new Vector3(-.35f+k*.35f,-.78f,-size*.53f),Vector3.one*.13f,new[]{Red,Mint,Purple}[k]);
                Box(top,new Vector3(0,.015f,0),new Vector3(size*.72f,.03f,size*.72f),Yellow);break;
            case 3: // Pool
                Box(top,new Vector3(0,.01f,0),new Vector3(size*.8f,.035f,size*.8f),Blue);
                for(int k=0;k<3;k++)Box(top,new Vector3((k-1)*.37f,.03f,0),new Vector3(.025f,.012f,size*.7f),Cream);
                Disk(top,new Vector3(size*.31f,.07f,size*.3f),.28f,.1f,Red);break;
            case 4: // Parcel
                for(int k=-1;k<=1;k++)Box(top,new Vector3(k*.5f,-.55f,-size*.505f),new Vector3(.2f,.85f,.02f),Cream);
                Box(top,new Vector3(0,.025f,0),new Vector3(.8f,.04f,.55f),Mint);break;
            case 5: // Toy piano
                for(int k=0;k<7;k++){float x=(k-3)*size/8;Box(top,new Vector3(x,.025f,0),new Vector3(size/9,.04f,size*.7f),k%2==0?Ink:Cream);}
                break;
            case 6: // Diner
                for(int k=0;k<6;k++)Box(top,new Vector3((k-2.5f)*size/6,-.15f,-size*.54f),new Vector3(size/6,.25f,.13f),k%2==0?Red:Cream);
                Disk(top,new Vector3(0,.025f,0),size*.72f,.035f,Yellow);break;
            case 7: // Cassette
                Box(top,new Vector3(0,.02f,0),new Vector3(size*.85f,.03f,size*.7f),Ink);
                Disk(top,new Vector3(-.43f,.045f,0),.43f,.03f,Cream);Disk(top,new Vector3(.43f,.045f,0),.43f,.03f,Cream);break;
        }
        Disk(top,new Vector3(0,.065f,0),.3f,.025f,Yellow);
        var coin=new GameObject("Perfect landing star").transform;coin.SetParent(root,false);coin.localPosition=Vector3.up*.65f;
        var star=Box(coin,Vector3.zero,new Vector3(.25f,.25f,.09f),Yellow);star.localRotation=Quaternion.Euler(0,0,45);
        pads.Add(new Pad{Root=root,Top=top,Coin=coin,Size=size,Position=pos,Number=n});
        if(n==0)coin.gameObject.SetActive(false);
    }
    public void Advance(bool collected){
        if(collected)Next.Coin.gameObject.SetActive(false);
        Current++;CurrentPad.Bounce=1;AddNext();
        if(Current>3){Destroy(pads[0].Root.gameObject);pads.RemoveAt(0);Current--;}
    }
    public void Burst(Vector3 at,bool perfect){
        int count=perfect?32:12;
        for(int i=0;i<count;i++){
            Particle p=particles.Find(x=>!x.T.gameObject.activeSelf);
            if(p==null){
                if(particles.Count>=100)break;
                p=new Particle{T=Box(null,at,Vector3.one*.1f,new[]{Red,Yellow,Mint,Cream,Purple}[particles.Count%5])};particles.Add(p);
            }
            p.T.gameObject.SetActive(true);p.T.position=at+Vector3.up*.1f;p.Max=p.Life=.5f+Random.value*.6f;p.Size=.06f+Random.value*.1f;
            p.V=new Vector3(Random.Range(-2.8f,2.8f),Random.Range(2,4f),Random.Range(-2.8f,2.8f));
        }
    }
    public void Animate(float dt,Vector3 focus,bool paused){
        if(paused)return;elapsed+=dt;
        scenery.position=Vector3.Lerp(scenery.position,new Vector3(focus.x,0,focus.z),dt*.4f);
        sea.position=new Vector3(focus.x,-2,focus.z);
        foreach(var p in pads){
            if(p.Bounce>0)p.Bounce=Mathf.Max(0,p.Bounce-dt*3);
            float squash=Mathf.Sin(p.Bounce*Mathf.PI)*.15f;
            p.Top.localScale=new Vector3(1+squash*.15f,1-squash,1+squash*.15f);p.Top.localPosition=Vector3.down*squash*.25f;
            p.Coin.localPosition=Vector3.up*(.65f+Mathf.Sin(elapsed*2+p.Number)*.1f);p.Coin.Rotate(0,dt*85,0);
        }
        targetRing.position=Next.Position+Vector3.up*.095f;targetRing.localScale=Vector3.one*(.75f+Mathf.Sin(elapsed*3)*.045f);
        shadow.position=new Vector3(Pawn.position.x,.085f,Pawn.position.z);shadow.localScale=new Vector3(.65f, .003f,.65f)*(1+Mathf.Max(0,Pawn.position.y)*.18f);
        bool supported=JumpRules.Lands(Pawn.position,CurrentPad.Position,CurrentPad.Size)||JumpRules.Lands(Pawn.position,Next.Position,Next.Size);
        shadow.gameObject.SetActive(supported);
        foreach(var p in particles){
            if(!p.T.gameObject.activeSelf)continue;
            p.Life-=dt;if(p.Life<=0){p.T.gameObject.SetActive(false);continue;}
            p.V+=Vector3.down*dt*7;p.T.position+=p.V*dt;p.T.Rotate(dt*200,dt*160,0);p.T.localScale=Vector3.one*p.Size*Mathf.Min(1,p.Life*4);
        }
    }
    void OnDestroy(){foreach(var m in materials.Values)Destroy(m);foreach(var p in particles)if(p.T)Destroy(p.T.gameObject);}
}
