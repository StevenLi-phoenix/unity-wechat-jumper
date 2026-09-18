using UnityEngine;
public sealed class JumpAudio : MonoBehaviour {
    AudioSource music,fx,charge;
    AudioClip jump,land,perfect,miss,click;
    public bool Muted {get;private set;}
    AudioClip Tone(string name,float frequency,float length,float end=0){
        var data=SynthesizeTone(frequency,length,end);var c=AudioClip.Create(name,data.Length,1,22050,false);c.SetData(data,0);return c;
    }
    public static float[] SynthesizeTone(float frequency,float length,float end=0){
        const int rate=22050;var data=new float[Mathf.CeilToInt(rate*length)];
        float phase=0;
        for(int i=0;i<data.Length;i++){
            float t=(float)i/data.Length;phase+=Mathf.Lerp(frequency,end==0?frequency:end,t)/rate;
            data[i]=(Mathf.Sin(phase*Mathf.PI*2)*.7f+Mathf.Sin(phase*Mathf.PI*4)*.2f)*Mathf.Sin(t*Mathf.PI)*.24f;
        }
        return data;
    }
    public void Initialize(){
        music=gameObject.AddComponent<AudioSource>();music.loop=true;music.volume=.17f;
        fx=gameObject.AddComponent<AudioSource>();fx.volume=.65f;
        charge=gameObject.AddComponent<AudioSource>();charge.loop=true;charge.volume=.10f;charge.clip=Tone("Charge hum",170,.15f);
        jump=Tone("Spring",260,.18f,750);land=Tone("Plop",240,.12f,120);perfect=Tone("Perfect",760,.3f,1200);miss=Tone("Miss",330,.5f,65);click=Tone("Button",540,.08f);
        Muted=PlayerPrefs.GetInt("JumperMuted",0)==1;
        var data=SynthesizeMusic();music.clip=AudioClip.Create("Rooftop shuffle",data.Length,1,22050,false);music.clip.SetData(data,0);ApplyMute();
    }
    public static float[] SynthesizeMusic(){
        const int rate=22050;float beat=60f/112f;int samples=Mathf.CeilToInt(beat*16*rate);var data=new float[samples];
        int[] notes={0,4,7,12,7,4,2,7,0,4,9,12,9,7,4,2};
        for(int i=0;i<samples;i++){
            float time=(float)i/rate;int b=(int)(time/beat);float t=time%beat;float f=261.63f*Mathf.Pow(2,notes[b%16]/12f);
            float melody=Mathf.Sin(time*f*2*Mathf.PI)*Mathf.Exp(-t*8)*.18f;
            float bass=Mathf.Sin(time*65.41f*(b<8?1:1.333f)*2*Mathf.PI)*Mathf.Exp(-t*6)*.2f;
            float kick=Mathf.Sin(t*(90-60*t)*2*Mathf.PI)*Mathf.Exp(-t*28)*.25f;
            data[i]=melody+bass+kick;
        }
        return data;
    }
    void ApplyMute(){music.mute=fx.mute=charge.mute=Muted;}
    public void Toggle(){Muted=!Muted;ApplyMute();PlayerPrefs.SetInt("JumperMuted",Muted?1:0);PlayerPrefs.Save();}
    public void Begin(){if(!music.isPlaying)music.Play();fx.PlayOneShot(click);}
    public void Charge(float value,bool on){if(on){charge.pitch=1+value*2;if(!charge.isPlaying)charge.Play();}else if(charge.isPlaying)charge.Stop();}
    public void Jump(){charge.Stop();fx.pitch=1;fx.PlayOneShot(jump);}
    public void Land(bool center,int combo){fx.pitch=center?1+Mathf.Min(combo,6)*.06f:1;fx.PlayOneShot(center?perfect:land);}
    public void Miss(){fx.pitch=1;fx.PlayOneShot(miss);}
    public void Pause(bool paused){if(paused)music.Pause();else if(!music.isPlaying)music.UnPause();charge.Stop();}
}
