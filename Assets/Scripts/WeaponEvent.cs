using UnityEngine;
public class WeaponEvent : MonoBehaviour {
    [SerializeField] PlayerCombatControl CombatControl;
    void EVNormalAtk(){ // đánh thường
        CombatControl.DealDmg(CombatControl.skillList.skills[0]);
    }
    void EVStab(){ // đâm kiếm
        CombatControl.DealDmg(CombatControl.skillList.skills[1]);
    }
    void EVFourSlash(){ // tấn công 4 lần
        CombatControl.DealDmg(CombatControl.skillList.skills[2]);
    }
    void EVSingleSlash(){ // chém mạnh 1 lần
        CombatControl.DealDmg(CombatControl.skillList.skills[3]);
    }
    public void EVOnTrail(){ // bật hiệu ứng chém
        CombatControl.weaponList.weapons[CombatControl.CurrWP].trail.emitting = true;
    }
    public void EVOffTrail(){ // tắt hiệu ứng chém
        CombatControl.weaponList.weapons[CombatControl.CurrWP].trail.emitting = false;
    }
    void EVRefreshAction(){
        CombatControl.ReAtk(); // làm mới hành động
    }
    void EVFootStep(){
        CombatControl.FootStep(); // tiếng bước chân
    }
}