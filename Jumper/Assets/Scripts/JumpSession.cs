using UnityEngine;
public enum JumpPhase { Title, Ready, Charging, Flight, Falling, Result, Paused }
public sealed class JumpSession {
    public JumpPhase Phase {get;private set;}=JumpPhase.Title;
    JumpPhase resumePhase;
    public int Score {get;private set;}
    public int Hops {get;private set;}
    public int Combo {get;private set;}
    public int BestCombo {get;private set;}
    public int Coins {get;private set;}
    public int LastAward {get;private set;}
    public float Charge {get;private set;}
    public int District=>Hops/12;
    public void StartRun(){Score=Hops=Combo=BestCombo=Coins=LastAward=0;Charge=0;Phase=JumpPhase.Ready;}
    public void BeginCharge(){if(Phase==JumpPhase.Ready){Charge=0;Phase=JumpPhase.Charging;}}
    public void TickCharge(float dt){if(Phase==JumpPhase.Charging)Charge=Mathf.Clamp01(Charge+Mathf.Max(0,dt)/.95f);}
    public void CancelCharge(){if(Phase==JumpPhase.Charging){Charge=0;Phase=JumpPhase.Ready;}}
    public void Release(){if(Phase==JumpPhase.Charging)Phase=JumpPhase.Flight;}
    public void Land(bool perfect,bool coin){
        if(Phase!=JumpPhase.Flight)return;
        Combo=perfect?Combo+1:0;BestCombo=Mathf.Max(BestCombo,Combo);
        LastAward=JumpRules.Points(perfect,Combo);Score+=LastAward;Hops++;
        if(coin)Coins++;Charge=0;Phase=JumpPhase.Ready;
    }
    public void LandBack(){if(Phase==JumpPhase.Flight){Charge=0;Phase=JumpPhase.Ready;}}
    public void Miss(){if(Phase==JumpPhase.Flight)Phase=JumpPhase.Falling;}
    public void Finish(){if(Phase==JumpPhase.Falling)Phase=JumpPhase.Result;}
    public void Pause(){
        if(Phase!=JumpPhase.Ready&&Phase!=JumpPhase.Charging&&Phase!=JumpPhase.Flight)return;
        CancelCharge();resumePhase=Phase;Phase=JumpPhase.Paused;
    }
    public void Resume(){if(Phase==JumpPhase.Paused)Phase=resumePhase;}
}
