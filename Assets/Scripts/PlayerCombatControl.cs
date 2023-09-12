using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class PlayerCombatControl : MonoBehaviour {
#region thông số chung
    // hệ thống đánh cận chiến và sử dụng kỹ năng
    [SerializeField] Image    WPImg   , StabImg, SkillImg, StaBar  , HPPotImg;
    // ảnh:                | trang bị |   đâm  | kỹ năng | thể lực |  dự trữ |
    [SerializeField] Text StaText, HPPotText;       // chữ thể lực, dự trữ
    [SerializeField] IngameControl IC;              // thay đổi thông số
    public int CurrWP, CurSkill;                    // trang bị và kỹ năng hiện tại
    [SerializeField] float MaxSta, Sta, StaUseSpeed , StaResSpeed;
    // thông số           |  thể lực  | tốc độ dùng | tốc độ hồi |
    public Animator WeaponAnimated;       // hoạt ảnh hành động
    [SerializeField] FirstPersonControl MovementControl; // sử dụng cho hoạt ảnh di chuyển
    public HealthControl healthControl;   // điều chỉnh máu khi hồi phục, hút máu
    [SerializeField] SoundManager sounds;           // âm thanh
    [SerializeField] LayerMask TargetLayer;         // lớp mục tiêu
    [SerializeField] Color NormalDmgColor, CritDmgColor; // màu sát thương
    [SerializeField] Transform AttackSet;           // điểm tấn công
    [SerializeField] bool DrawlAttackRange;         // vẽ tầm tấn công
    public WeaponList weaponList;                   // danh sách trang bị
    public SkillList skillList;                     // danh sách kỹ năng
    bool Criting, ActionAllowed;                    // điều kiện chí mạng, khả dụng hành động
    float          DmgDeal  , HealthElaps, StabElaps, SkillElaps;
    // số đếm: | sát thương |  hồi phục  |    đâm   |  kỹ năng  |
    Coroutine HealCD, Skill1CD, Skill2CD; // bộ đếm hồi kỹ năng | 1 = đâm | 2 = kỹ năng |
#endregion
#region phím sử dụng và tính năng chung
    void Start(){
        HPPotText.text = skillList.health.Stock + " / " + skillList.health.MaxStock; // hiển thị lại số lượng
        SkillChange();
        healthControl.ShowHealth(healthControl.Health);
        SetWeapon(1);
        SetWeapon(0);
    }
    public void SetGear(){          // sắp đặt trang bị
        healthControl.ShowHealth(0); // hiện máu 0
        ShowSta(0);                 // hiện thể lực 0
        SkillChange();              // đặt lại kỹ năng
        SetWeapon(0);               // đặt trang bị
        StartCoroutine(StatActive()); // hiển thị theo thời gian
    }
    IEnumerator StatActive(){
        DmgDeal = 0f;
        while (DmgDeal < 2f){
            healthControl.ShowHealth(Mathf.Lerp(0, healthControl.MaxHealth, DmgDeal / 2f)); // hiện máu 0
            ShowSta(Mathf.Lerp(0, MaxSta, DmgDeal / 2f)); // hiện thể lực 0
            yield return null;
        }
        // hiển thị lần cuối
        healthControl.ShowHealth(healthControl.MaxHealth);
        ShowSta(MaxSta);
    }
    void Update(){
        WeaponAnimated.SetFloat("Moving", MovementControl.walkFeature.MoveUse.magnitude); // hoạt ảnh di chuyển
        ActionInput();
        Damping();
    }
    void ActionInput(){
        // đổi trang bị với số bấm
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetWeapon(0); // đổi rìu
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetWeapon(1); // đổi cúp chim
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetWeapon(2); // đổi kiếm ngắn
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetWeapon(3); // đổi kiếm dài
        if (ActionAllowed){                         // sử dụng hành động, kỹ năng
            // hành động
            if (Input.GetMouseButton(0)) TriggerAnimation("Atk " + Random.Range(1, weaponList.WeaponStats.weapons[CurrWP].ActionLimits +1));
            if (Input.GetMouseButton(1)) SkillUse(1, "Stab", StabElaps, StabImg, true);                            // kỹ năng đâm
            if (Input.GetKeyDown(KeyCode.R)) SkillUse(CurSkill, "Skill " + CurSkill, SkillElaps, SkillImg, false); // kỹ năng chính
        }
        if (Input.GetKeyDown(KeyCode.V)) SkillChange();         // đổi kỹ năng
        if (Input.GetKeyDown(KeyCode.C)) HealthPotUse();        // sử dụng bình máu
        if (Input.GetKey(KeyCode.LeftShift) && Sta > 0 && WeaponAnimated.GetFloat("Moving") > 0){ // kiểm soát tốc độ
            Sta -= Time.deltaTime * StaUseSpeed;
            ShowSta(Sta);
            MovementControl.enableFeature.EnableRun = true;
            MovementControl.RunInput();
        } else if (Sta < MaxSta){ // hồi phục
            Sta += Time.deltaTime * StaResSpeed;
            ShowSta(Sta);
            MovementControl.enableFeature.EnableRun = false;
            MovementControl.RunInput();
        }
    }
    void TriggerAnimation(string animateName){  // sử dụng hành động
        WeaponAnimated.speed = weaponList.WeaponStats.weapons[CurrWP].AtkSpeed; // đặt tốc độ tấn công
        WeaponAnimated.SetTrigger(animateName); // chơi hoạt ảnh
        ActionAllowed = false;                  // tắt khả năng sử dụng
    }
    public void SetWeapon(int NewWP){           // đổi trang bị
        if (weaponList.weapons[NewWP].Avaliable && CurrWP != NewWP){
            foreach(var WP in weaponList.weapons){ // ẩn trang bị thừa
                WP.trail.emitting = false;      // tắt vệt chém cũ
                WP.Mesh.SetActive(false);       // tắt vật thể cũ
            }
            weaponList.weapons[NewWP].Mesh.SetActive(true);                 // hiện vật thể mới
            weaponList.weapons[NewWP].trail.emitting = false;               // ẩn vệt chém
            WPImg.sprite = weaponList.WeaponStats.weapons[NewWP].Info.Image; // đổi ảnh
            TriggerAnimation("Switch");                                     // hoạt ảnh
            CurrWP = NewWP;                                                 // lưu trang bị
            sounds.Play("TakeOut"); // âm thanh thay đổi
        }
        if (CurrWP > 1) StabImg.color = SkillImg.color = new Color(1f, 1f, 1f, 1f); // làm sáng ảnh
        else StabImg.color = SkillImg.color = new Color(1f, 1f, 1f, 0.5f); // làm tối ảnh
    }
    void SkillUse(int SkillNumb, string SkillName, float SkillElaps, Image SkillImg, bool Skill1){ // sử dụng kỹ năng
        if (CurrWP > 1 && Sta > skillList.skills[SkillNumb].StaCost && SkillImg.fillAmount >= 1f){ // kiểm tra điều kiện
            TriggerAnimation(SkillName); // hoạt ảnh
            IC.AchivePorgress(9, true, 1); // thêm thành tựu
            StaRestore(-skillList.skills[SkillNumb].StaCost); // giảm thể lực
            if (Skill1){
                if (Skill1CD != null) StopCoroutine(Skill1CD); // kiểm tra bộ đếm
                Skill1CD = StartCoroutine(SkillRefresh(skillList.skills[SkillNumb].CD, SkillElaps, SkillImg)); // hồi chiêu
            } else {
                if (Skill2CD != null) StopCoroutine(Skill2CD); // kiểm tra bộ đếm
                Skill2CD = StartCoroutine(SkillRefresh(skillList.skills[SkillNumb].CD, SkillElaps, SkillImg)); // hồi chiêu
            }
        }
    }
    void SkillChange(){                     // thay đổi kỹ năng chính
        if (CurSkill == 2) CurSkill = 3;    // đổi sang chém 1
        else CurSkill = 2;                  // đổi sang chém 4
        SkillImg.sprite = skillList.skills[CurSkill].image; // đổi ảnh
        if (Skill2CD != null) StopCoroutine(Skill2CD);      // dừng hồi chiêu
        Skill2CD = StartCoroutine(SkillRefresh(skillList.skills[CurSkill].CD, SkillElaps, SkillImg)); // làm mới hồi chiêu
    }
#endregion
#region hồi phục máu và thể lực
    void HealthPotUse(){        // sử dụng dự trữ
        if(skillList.health.Stock > 0 && HPPotImg.fillAmount >= 1f && healthControl.Health < healthControl.MaxHealth -1){
            skillList.health.Stock--; // giảm dự trữ
            HPPotText.text = skillList.health.Stock + " / " + skillList.health.MaxStock; // hiển thị lại số lượng
            healthControl.HealthRestore(skillList.health.ResotrePoint); // sử dụng
            healthControl.ActiveRegen(skillList.health.ResotrePoint, skillList.health.AdditionRate); // hồi phục thêm
            if (HealCD != null) StopCoroutine(HealCD);
            HealCD = StartCoroutine(SkillRefresh(skillList.health.CD, HealthElaps, HPPotImg));
            IC.AchivePorgress(15, true, 1); // tăng thành tựu
            sounds.Play("UsePotion"); // âm thanh
        }
    }
    public void HealthPotChange(){      // thêm cộng dồn dự trữ
        if (skillList.health.Stock < skillList.health.MaxStock){
            skillList.health.Stock ++;  // tăng dự trữ
            HPPotText.text = skillList.health.Stock + " / " + skillList.health.MaxStock; // đặt số lượng bình máu
        } else healthControl.HealthRestore(skillList.health.ResotrePoint); // nếu đã đủ sẽ sử dụng luôn
    }
    public void StaRestore(float Amount){   // hồi phục thể lực
        Sta += Amount;                      // tăng lượng
        Sta = Mathf.Clamp(Sta, 0, MaxSta);  // giới hạn thể lực
        ShowSta(Sta);                       // hiển thị lại
    }
    void ShowSta(float Amount){ // hiển thị thể lực
        StaBar.fillAmount = Amount / MaxSta;            // độ đầy
        StaText.text = (int)(Amount) + " / " + MaxSta;  // số lượng
    }
#endregion
#region tấn công, hiệu ứng, kỹ năng
    Collider[] HitList;
    public void DealDmg(SkillList.Skill skill){ // gây sát thương
        HitList = Physics.OverlapSphere(AttackSet.position, weaponList.WeaponStats.weapons[CurrWP].AtkRange + skill.Range, TargetLayer);
        foreach(var Entity in HitList){
            Damage(weaponList.WeaponStats.weapons[CurrWP].MinDamage * skill.DmgRate, weaponList.WeaponStats.weapons[CurrWP].MaxDamage * skill.DmgRate, weaponList.WeaponStats.weapons[CurrWP].CritChance, weaponList.WeaponStats.weapons[CurrWP].CritDamage);
            if (Criting) Entity.GetComponent<HealthControl>().TakeDamage(DmgDeal, CritDmgColor);
            else Entity.GetComponent<HealthControl>().TakeDamage(DmgDeal, NormalDmgColor);
            healthControl.HealthRestore(skill.LifeSteal); // hút máu với mỗi mục tiêu
        }
        sounds.PlayRandom("AttackSound"); // âm thanh tấn công
    }
    public void FootStep(){ // tiếng bước chân
        sounds.PlayRandom("FootStep");
    }
    public void ReAtk(){
        ActionAllowed = true;
        WeaponAnimated.speed = 1f; // đặt tốc độ hoạt ảnh
    }
    float Damage(float MinDmg, float MaxDmg, float CritRate, float CritDmg){
        DmgDeal = Random.Range(MinDmg, MaxDmg);
        Criting = false;
        if (Random.Range(0, 100) < CritRate){
            Criting = true;
            return (int)(DmgDeal *= CritDmg);
        }
        return (int)(DmgDeal);
    }
    IEnumerator SkillRefresh(float SkillCD, float Elaps, Image SkillImg){ // thay đổi chỉ số theo thời gian ngắn
        SkillImg.fillAmount = Elaps = 0f;
        while (Elaps < SkillCD){
            SkillImg.fillAmount = Mathf.Lerp(0f, 1f, Elaps / SkillCD);
            Elaps += Time.deltaTime;
            yield return null;
        }
        SkillImg.fillAmount = 1f;
        sounds.Play("SkillReady");
    }
#endregion
#region Sway, damp trang bị
    // di chuyển vũ khí cho mượt mà hơn
    // gắn vào parent vũ khí
    public float SmoothTime, SmoothForce;
    Vector2 DampInput; // đầu sử dụng
    [SerializeField] Transform WeaponHolder;
    Quaternion RotateUse;
    void Damping(){
        DampInput = MovementControl.cameraFeature.RotateInput * SmoothForce; // lấy số đầu vào
        // lấy góc xoay
        RotateUse = Quaternion.AngleAxis(DampInput.x, Vector3.down) * Quaternion.AngleAxis(DampInput.y, Vector3.left);
        // xoay
        WeaponHolder.localRotation = Quaternion.Slerp(WeaponHolder.localRotation, RotateUse, SmoothTime * Time.deltaTime);
    }
#endregion
#region hiển thị tầm đánh, danh sách trang bị, kỹ năng
    void OnDrawGizmos() { // tầm đánh
        if (DrawlAttackRange){
            Gizmos.color = Color.green; // hiện tầm đánh
            Gizmos.DrawWireSphere(AttackSet.position, weaponList.WeaponStats.weapons[CurrWP].AtkRange);
            Gizmos.color = Color.red; // hiện tầm kỹ năng
            Gizmos.DrawWireSphere(AttackSet.position, weaponList.WeaponStats.weapons[CurrWP].AtkRange + skillList.skills[CurSkill].Range);
        }
    }
    [System.Serializable] public class SkillList{
        public Skill[] skills;
        public Consume health, Sta;
        // 0 = hồi máu | 1 = đâm | 2 = chém 4 lần | 3 = chém 1 lần | 4 = trống |
        [System.Serializable] public class Skill {
            public Sprite image; // ảnh hiển thị
            public float   DmgRate   ,  ScaleRate  ,  Range   ,    StaCost    ,    CD     , MinCD, CDReduce , LifeSteal ;
            // chỉ số:  | sát thương | tỉ lệ tăng |  tầm đánh |    tiêu hao   | hồi chiêu | tỉ lệ hồi chiêu |  hút máu  |
        }
        [System.Serializable] public class Consume {
            public int      Stock  , MaxStock, ResotrePoint;
            // thông số: | còn lại |  tối đa |  lượng hồi  |
            public float    AdditionRate  ,       CD      ,   MinCD   ,   CDReduce    ;
            // thông số: | tỉ lệ hồi thêm | thời gian hồi | hồi chiêu | tỉ lệ hồi lại |
        }
    }
    [System.Serializable] public class WeaponList {
        public ItemBaseInfo WeaponStats;    // chỉ số gốc của trang bị
        public Weapon[] weapons;
        // 0 = rìu | 1 = cúp | 2 = kiếm ngắn | 3 = kiếm dài
        [System.Serializable] public class Weapon {
            public bool Avaliable;          // khả dụng
            public GameObject Mesh;         // vật thể
            public TrailRenderer trail;     // vệt chém
        }
    }
#endregion
}