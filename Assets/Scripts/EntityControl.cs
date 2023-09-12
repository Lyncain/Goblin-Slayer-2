using UnityEngine;
using UnityEngine.AI;
using System.Collections;
public class EntityControl : MonoBehaviour {
#region var
    [SerializeField] int StatsIndext;                       // chỉ số theo thứ tự
    enum AgentState { StandFirm, Moving} AgentState AGstate; // lấy trạng thái của AI
    enum AGSkill {None, Healing, Charge, Dash, ExtraLife} [SerializeField] AGSkill aGSkill; // dạng kỹ năng của AI
    [SerializeField] EntityBaseStats AgentStats;            // chỉ số của AI
    [SerializeField] bool  DrawlAttackRange , Atked, CanUseSkill;
    // điều kiện:         | vẽ tầm tấn công | tấn công, kỹ năng |
    [SerializeField] NavMeshAgent Agent;                    // kiểm soát AI
    [SerializeField] Animator animator;                     // thực hiện hành động và động tác
    [SerializeField] Transform ParentObject, AtkPoint, HealthCanvas; // điểm tấn công
    [SerializeField] HealthControl healthControl;           // tình trạng máu
    [SerializeField] SoundManager Sounds;                   // âm thanh cho chạy, tấn công
    [SerializeField] TrailRenderer WeaponTrail;             // vệt chém tấn công
    [SerializeField] GameObject BulletObject;               // vật bắn
    Transform Target;                                       // điểm AI sẽ nhìn vào khi chạy đến và tấn công
    HealthControl TargetHealth;                             // tình trạng máu mục tiêu
    Quaternion StartLook, EndLook;                          // vị trí xoay
    Vector3 StartPos, EndPos, RefPos;                       // vị trí di chuyển
    Coroutine DashCour, LookCour;                           // chỉ cho 1 quá trình chạy
    [HideInInspector] float BasicSpeed, DashElaps, LookElaps, RefDistant; // tốc độ cơ bản, khoảng cách
#endregion
#region thay đổi skin dành cho slime
    [SerializeField] SkinType skinType;
    [System.Serializable] public class SkinType {
        public bool HaveSkin;
        public GameObject[] Head, Eyes, Body;
    }
    void SkinCheck(GameObject[] ObjectList){
        foreach(GameObject RefSkin in ObjectList) RefSkin.SetActive(false);
        ObjectList[Random.Range(0, ObjectList.Length)].SetActive(true);
    }
    void CallSkin(){
        if (skinType.HaveSkin){
            SkinCheck(skinType.Head);
            SkinCheck(skinType.Eyes);
            SkinCheck(skinType.Body);
        }
    }
#endregion
#region di chuyển và trạng thái
    void Start(){
        CallSkin();
        Target = GameObject.FindGameObjectWithTag("Player").transform; // lấy mục tiêu
        Sounds.Play("Spawning"); // âm thanh xuất hiện
        AGstate = AgentState.StandFirm; // đặt trạng thái khi xuất hiện
        TargetHealth = Target.GetComponent<HealthControl>(); // lấy mục máu của mục tiêu
        // đặt máu
        healthControl.SetHealth(Random.Range(AgentStats.UseStats[StatsIndext].MinHealth, AgentStats.UseStats[StatsIndext].MaxHealth)); // đặt máu
        // đặt tốc độ
        Agent.speed = BasicSpeed = Random.Range(AgentStats.OGStats[StatsIndext].MinSpeed, AgentStats.OGStats[StatsIndext].MaxSpeed); // đặt tốc độ di chuyển
        // đặt kích cỡ
        gameObject.transform.localScale = Vector3.one * Random.Range(AgentStats.OGStats[StatsIndext].MinScale, AgentStats.OGStats[StatsIndext].MaxScale); // đặt kích cỡ
        // sử dụng kỹ năng
        if (aGSkill == AGSkill.Healing) healthControl.ActiveRegen((int)(healthControl.MaxHealth / 2f), 1f); // hồi phục theo thời gian
        if (CanUseSkill){
            CanUseSkill = false;
            Invoke(nameof(ResetSkill), AgentStats.OGStats[StatsIndext].SkillCD);
        }

    }
    void LateUpdate(){
        // đổi trạng thái tùy theo tình huống
        if (!healthControl.Alive){ // kiểm tra tình trạng máu
            if (aGSkill == AGSkill.ExtraLife) ActiveRevive(); // điều kiện thêm 1 mạng
            else Ded();
            return;
        }
        HealthCanvas.LookAt(Target.position); // hướng thanh máu theo tầm nhìn
        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f); // ngăn độ xoay lỗi
        if (AGstate == AgentState.Moving) StartWalk(); // sử dụng hành động
    }
    void StartWalk(){
        if (GetDistant() <= AgentStats.OGStats[StatsIndext].StopRange && TargetHealth.CanTakeDamage){ // kiểm tra đòn đánh
            StartLookAt();                          // quay về phía mục tiêu
            AGStation();                            // cố định vị trí
            if (CanUseSkill){                       // kiểm tra kỹ năng
                TriggerAction("Skill");             // sử dụng kỹ năng
                CanUseSkill = false;                // xác nhận sử dụng kỹ năng
            } else if (!Atked) TriggerAction("Atk " + Random.Range(1, 3)); // kiểm tra đánh thường
            return;
        }
        if (aGSkill == AGSkill.Dash && GetDistant() > AgentStats.OGStats[StatsIndext].SkillRange && CanUseSkill) ActiveRangeSkill(); // kiểm tra lướt
        else {
            // kiểm tra kỹ năng chạy
            if (aGSkill == AGSkill.Charge && GetDistant() > AgentStats.OGStats[StatsIndext].SkillRange && CanUseSkill){
                Agent.speed = BasicSpeed * 2f;          // đặt tốc độ
                animator.SetInteger("Walk", 0);         // hoạt ảnh đi
                animator.SetInteger("Run", 1);          // hoạt ảnh chạy
                Agent.SetDestination(Target.position);  // đặt điểm đến và tiến tới
                // bước đi bình thường
            } else if (GetDistant() > AgentStats.OGStats[StatsIndext].StopRange){ // kiểm tra thường
                Agent.speed = BasicSpeed;               // đặt tốc độ
                animator.SetInteger("Walk", 1);         // hoạt ảnh đi
                Agent.SetDestination(Target.position);  // đặt điểm đến và tiến tới
                // tấn công khi trong tầm
            }
        }
    }
    float GetDistant(){
        return Vector3.Distance(transform.position, Target.position);
    }
    void TriggerAction(string ActionName){
        animator.SetTrigger(ActionName);
        Atked = true;
    }
    void ActiveRangeSkill(){            // sử dụng kỹ năng tầm xa
        StartLookAt();                  // nhìn về phía mục tiêu
        AGStation();                    // cố định vị trí
        animator.SetTrigger("Skill");   // sử dụng kỹ năng
    }
    void StartLookAt(){
        if (LookCour != null) StopCoroutine(LookCour);
        LookCour = StartCoroutine(LookAt());
    }
    void AGStation(){ // cố định vị trí
        AGstate = AgentState.StandFirm;
        if (Agent.enabled) Agent.SetDestination(transform.position);
        animator.SetInteger("Walk", 0); // hoạt ảnh đi
        animator.SetInteger("Run", 0);  // hoạt ảnh chạy
    }
    void ActiveRevive(){                // khả năng hồi sinh
        AGStation();                    // cố định vị trí
        aGSkill = AGSkill.None;         // tắt khả năng hồi sinh
        healthControl.ShowHealth(0f);   // đặt độ đầy của thanh máu
        this.enabled = healthControl.CanTakeDamage = false;
        animator.SetTrigger("Skill");   // hoạt ảnh
    }
    void Ded(){ // tạch !
        healthControl.ShowHealth(0f);
        AGStation(); // cố định lại vị trí
        gameObject.AddComponent<Rigidbody>(); // thêm trọng lực
        DropItem(); // rơi vật phẩm
        FindObjectOfType<WaveControl>().AmountReduce(); // giảm số lượng tồn tại
        IngameControl IC = FindObjectOfType<IngameControl>(); // thêm tiến trình thành tựu
        foreach(int Number in AchiveAdding) IC.AchivePorgress(Number, true, 1); // tăng tiến trình
        Destroy(ParentObject.gameObject, 10f); // xóa mục tiêu sau thời gian nhất định
        if (WeaponTrail != null) WeaponTrail.emitting = false;
        healthControl.Alive = true; // đổi lại điều kiện
        healthControl.CanTakeDamage = animator.enabled = Agent.enabled = this.enabled = false;
    // tắt điều kiện | nhận sát thương |  hoạt ảnh     |      AI       |    script    |
    }
    public void DropItem(){ // rơi đồ
        foreach(var item in AgentStats.OGStats[StatsIndext].itemDrop){
            if (Random.Range(0,100) < item.Rate){ // xác nhận rơi
                for (int RefInt = 0; RefInt < Random.Range(item.MinAmount, item.MaxAmount) + 1; RefInt++){ // rơi số lượng ngẫu nhiên
                    Instantiate(item.Object, // tạo vật phẩm
                    // đặt vị trí và xoay hướng
                    gameObject.transform.position + (Vector3.up * 1.5f) + (Random.insideUnitSphere * 0.5f), Random.rotation).
                    // đặt số lượng
                    GetComponent<ItemsInteract>().SetItemInfo(Random.Range(1, item.AmountPerItem));
                }
            }
        }
    }
#endregion
#region Coroutine
    IEnumerator Dashing(){
        DashElaps = 0f;
        StartPos = transform.position;
        // lấy điểm lướt tới
        EndPos = Target.position - (Vector3.Normalize(Target.position - transform.position) * AgentStats.OGStats[StatsIndext].StopRange);
        EndPos.y = RefPos.y;
        Agent.enabled = false;
        transform.LookAt(EndPos);
        while (DashElaps < 0.4f){
            transform.position = Vector3.Lerp(StartPos, EndPos, DashElaps / 0.4f); // bay tới vị trí
            DashElaps += Time.deltaTime; // tăng theo thời gian
            yield return null;
        }
        transform.position = EndPos; // điểm cuối
        Agent.enabled = true;
    }
    IEnumerator LookAt(){
        LookElaps = 0f;
        EndLook = Quaternion.LookRotation(Target.position - transform.position);
        StartLook = transform.rotation;
        while (LookElaps < 0.3f) {
            transform.rotation = Quaternion.Slerp(StartLook, EndLook, LookElaps / 0.3f);
            LookElaps += Time.deltaTime;
            yield return null;
        }
        transform.rotation = EndLook;
    }
#endregion
#region Event
    float GetAtkRange(){
        return Vector3.Distance(AtkPoint.position, Target.position);
    }
    void EVSetNewHealth(){
        healthControl.SetHealth((int)(healthControl.MaxHealth / 2f)); // đặt máu còn lại 1 nửa
        healthControl.ActiveRegen((int)(healthControl.MaxHealth / 2f), 1f); // đặt hồi phục
        this.enabled = healthControl.Alive = healthControl.CanTakeDamage = true;
    }
    void EVStartDash(){
        Sounds.PlayRandom("Jump"); // âm thanh
        if (DashCour != null) StopCoroutine(DashCour);
        DashCour = StartCoroutine(Dashing());
    }
    void EVMeleeAtk(){ // đòn tấn công tầm gần
        // nếu trong tầm sẽ gây sát thương
        if (GetAtkRange() < AgentStats.OGStats[StatsIndext].AttackRange) TargetHealth.TakeDamage(Random.Range(AgentStats.UseStats[StatsIndext].MinDmg, AgentStats.UseStats[StatsIndext].MaxDmg + 1), Color.clear);
        Invoke(nameof(ResetAtk), AgentStats.OGStats[StatsIndext].AtkCD); // hồi chiêu đòn đánh
        Sounds.PlayRandom("Swing");
    }
    void EVMeleeSkillAtk(){ // đòn tấn công tầm gần
        // nếu trong tầm sẽ gây sát thương
        if (GetAtkRange() < AgentStats.OGStats[StatsIndext].AttackRange) TargetHealth.TakeDamage(Random.Range(AgentStats.UseStats[StatsIndext].MinDmg, AgentStats.UseStats[StatsIndext].MaxDmg + 1) * 1.5f, Color.clear);
        Invoke(nameof(ResetSkill), AgentStats.OGStats[StatsIndext].SkillCD); // hồi chiêu kỹ năng
        Invoke(nameof(ResetAtk), AgentStats.OGStats[StatsIndext].AtkCD); // hồi chiêu đòn đánh
        Sounds.PlayRandom("Swing");
    }
    void EVRangeAtk(){ // đòn tấn công tầm xa
        Instantiate(BulletObject, AtkPoint.position, Quaternion.identity).GetComponent<BulletControl>().Shoot(Target.position, Random.Range(AgentStats.UseStats[StatsIndext].MinDmg, AgentStats.UseStats[StatsIndext].MaxDmg + 1));
        Invoke(nameof(ResetAtk), AgentStats.OGStats[StatsIndext].AtkCD); // hồi chiêu đòn đánh
        Sounds.PlayRandom("Swing");
    }
    void ResetAtk(){ // làm mới đòn tấn công
        Atked = false;
        AGstate = AgentState.Moving;
    }
    void ResetSkill(){ // làm mới đòn kỹ năng
        CanUseSkill = true;
    }
    void EVOnTrail(){
        WeaponTrail.emitting = true;
    }
    void EVOffTrail(){
        WeaponTrail.emitting = false;
    }
    void EVFootStep(){ // âm thanh bước chân
        Sounds.PlayRandom("Step");
    }
    void OnDrawGizmos() {
        if (DrawlAttackRange){
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AgentStats.OGStats[StatsIndext].StopRange); // hiện tầm dừng lại
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(AtkPoint.position, AgentStats.OGStats[StatsIndext].AttackRange); // hiện tầm đánh
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(AtkPoint.position, AgentStats.OGStats[StatsIndext].SkillRange); // hiện tầm gây sát thương
        }
    }
#endregion
#region Achivement
    [SerializeField] int[] AchiveAdding;                // tăng tiến trình thành tựu sau khi bị hạ
#endregion
}