using UnityEngine;
using UnityEngine.UI;
public class WaveControl : MonoBehaviour {
    // chung
    // thử các vật phẩm, Entity
    [SerializeField] TesingEntity testingEntity;
    [System.Serializable] public class TesingEntity{
        public GameObject RefEntity;
        public Transform SpawnPosition;
    }
#region tính năng chính
    public void Start(){
        if (PlayerPrefs.GetInt("CL") == 0) spawningFeature.WaveText = "Đợt: ";
        else spawningFeature.WaveText = "Wave: ";
        spawningFeature.WaveCountDownTxt.text = ""; // để trống
        spawningFeature.WaveCountTxt.text = spawningFeature.WaveText + spawningFeature.WaveCount;
        entityStats.StatsList.ResetStats(); // đặt lại chỉ số
        WaitForNewSpawn();
    }
    void Update(){
        if (spawningFeature.EnableSpawn)
        if (!spawningFeature.IsSpawning){ // đếm ngược cho tới đợt tiếp theo
            if (spawningFeature.WaveCD >= 0f){
                spawningFeature.WaveCD -= Time.deltaTime; // đếm ngược
                // hiển thị thời gian đếm ngược
                spawningFeature.WaveCountDownTxt.text = spawningFeature.WaveCD.ToString("0");
            } else if (spawningFeature.WaveCD <= 0f){
                spawningFeature.IsSpawning = true;          // đặt lại điều kiện
                spawningFeature.WaveCountDownTxt.text = ""; // đặt trống chữ
                SetupNewSpawn();
            }
        } else {
            // đếm ngược để tạo mục tiêu mới
            if (spawningFeature.SpawnCount > 0 && spawningFeature.SpawnCD <= 0f && spawningFeature.ExistCount < spawningFeature.MaxExist){
                spawningFeature.SpawnCD += spawningFeature.SpawnDelay; // đặt mới đếm ngược
                SpawnEntity(); // tạo Entity
            } else if (spawningFeature.SpawnCD > 0) spawningFeature.SpawnCD -= Time.deltaTime; 
        }
        if (Input.GetKeyDown(KeyCode.P)){ // nầu !
            SetupNewSpawn();
            StopAllCoroutines();
            spawningFeature.EnableSpawn = false;
            spawningFeature.WaveCountDownTxt.color = Color.red;
            spawningFeature.WaveCountDownTxt.text = "! " + spawningFeature.WaveCount + " !";
            SpawnEndLess();
        }
    }
    public void AmountReduce(){ // giảm lượng mục tiêu
        spawningFeature.ExistCount--;
        if (spawningFeature.ExistCount <= 0 && spawningFeature.SpawnCount <= 0) WaitForNewSpawn(); // bắt đầu đợt mới
    }
#endregion
#region tính năng tạo chính
    public SpawningFeature spawningFeature;
    [System.Serializable] public class SpawningFeature {
        public bool EnableSpawn;
        public int  WaveCount,  MaxSpawn   ,  MaxExist  , MinIncrease, MaxIncrease;
        // số đếm: |   đợt   | số tạo tổng | số tồn tại |   số tăng sau mỗi đợt   |
        public float     WaveDelay  ,  SpawnDelay  , MinDelayed, DelayReduce, SpawnRadious;
        // thông số: | chờ đợt tiếp | chờ tạo tiếp |   giảm thời gian tạo   | phạm vi tạo |
        public bool DrawSpawnRange;       // vẽ phạm vi tạo
        public Text    WaveCountDownTxt, WaveCountTxt ;
        //  hiển thị: |    đếm ngược   | đợt hiện tại |
        public Transform[] SpawnPosition; // vị trí ngẫu nhiên sẽ tạo ra
        public GameObject[] entityList;   // danh sách tạo
        public int CurType; // mức chủng loài hiện tại
        [HideInInspector] public int     ExistCount    , SpawnCount;
        // số đếm:                  | tồn tại hiện tại | tạo tối đa |
        public float   SpawnCD = 8f, WaveCD = 10f;
        // đếm ngược: |    tạo     |     đợt     |
        [HideInInspector] public bool IsSpawning; // điều kiện tạo
        [HideInInspector] public string WaveText;
    }
    void SpawnEntity(){ // tạo mục tiêu
        spawningFeature.ExistCount++; // tăng số lượng hoạt động
        spawningFeature.SpawnCount--;
        // tạo
        Instantiate(spawningFeature.entityList[GetRandomNumber(spawningFeature.CurType)],
        // đặt vị trí xuất hiện
        spawningFeature.SpawnPosition[Random.Range(0, spawningFeature.SpawnPosition.Length)].position,
        // đặt hướng xoay
        Quaternion.Euler(0f, Random.Range(0f, 360), 0f), IC.EntityContain);
    }
    public void WaitForNewSpawn(){
        // đặt đếm ngược đợt tiếp
        spawningFeature.WaveCD = spawningFeature.WaveDelay;
        spawningFeature.IsSpawning = false;
    }
    void SetupNewSpawn(){ // đặt thông số đợt mới
        // điều chỉnh đợt tiếp theo
        // giảm thời gian tạo
        if (spawningFeature.SpawnDelay > spawningFeature.MinDelayed) spawningFeature.SpawnDelay -= spawningFeature.DelayReduce;
        // tăng số lượng tối đa
        spawningFeature.SpawnCount = spawningFeature.MaxSpawn += Random.Range(spawningFeature.MinIncrease, spawningFeature.MaxIncrease + 1);
        // tăng số lượng tồn tại
        spawningFeature.MaxExist ++;
        // tăng đợt đã qua
        spawningFeature.WaveCount++; // tăng đợt hiện tại
        spawningFeature.WaveCountTxt.text = spawningFeature.WaveText + spawningFeature.WaveCount;
        IC.SetAmount(0, 1); // hiển thị đợt hiện tại
        CheckWeapon();          // kiểm tra trang bị
        AdjustEntityStats();    // điều chỉnh chỉ số mục tiêu
        AdjustSkillStats();     // điều chỉnh sát thương kỹ năng
        AchiveIncreasing();     // thêm tiến trình thành tựu
    }
    void SpawnEndLess(){ // tạo không giới hạn
        if (spawningFeature.ExistCount < spawningFeature.MaxExist) SpawnEntity(); // tạo mục tiêu
        Invoke(nameof(SpawnEndLess), spawningFeature.SpawnDelay); // gọi lại
    }
    void OnDrawGizmos() { // vẽ phạm vi tạo
        if (spawningFeature.DrawSpawnRange){
            Gizmos.color = Color.black; // đổi màu
            // vẽ phạm vi
            foreach(Transform SpawnPos in spawningFeature.SpawnPosition) Gizmos.DrawWireSphere(SpawnPos.position, spawningFeature.SpawnRadious);
        }
    }
    int GetRandomNumber(int MaxLenght){
        return Random.Range(0, MaxLenght);
    }
#endregion
#region mở rộng Entity
    [SerializeField] EntityStatsAdjust entityStats;
    [System.Serializable] public class EntityStatsAdjust {
        [Range(1f,1.1f)] public float StatsScale = 1.015f; // tỉ lệ sức mạnh theo thời gian - đề xuất 1.015f
        public EntityBaseStats StatsList; // danh sách chỉ số
    }
    void AdjustEntityStats(){
        // tăng chỉ số
        entityStats.StatsList.StatAdjust(entityStats.StatsScale);
        // tăng chủng loài
        if (spawningFeature.CurType < spawningFeature.entityList.Length) spawningFeature.CurType ++;
    }
#endregion
#region thêm trang bị
    [SerializeField] ItemDropFeature DropFeature;
    [System.Serializable] public class ItemDropFeature {
        public int DropAfter, DropCount;
        public PlayerCombatControl weaponList; // theo dõi số lượng trang bị
        public GameObject[] WeaponDrop;
        [HideInInspector] public int RefWeaponDext;
    }
    void CheckWeapon(){
        foreach(var RefWeapon in DropFeature.weaponList.weaponList.weapons){ // kiểm tra trang bị
            if (!RefWeapon.Avaliable){
                DropFeature.DropCount--; // giảm đêm ngược
                if (DropFeature.DropCount < 0){
                    // đặt lại số đợi mới
                    DropFeature.DropCount += GetRandomNumber(DropFeature.DropAfter);
                    // tạo vật mới
                    Instantiate(GetWeapon(), DropFeature.weaponList.transform.position + Random.insideUnitSphere * 2f, Random.rotation);
                } break;
            }
        }
    }
    void AdjustSkillStats(){
        foreach(var Skill in IC.CombatSystem.skillList.skills){
            if (Skill.CD > Skill.MinCD) Skill.CD -= Skill.CDReduce; // giảm thời gian hồi
            Skill.DmgRate += Skill.ScaleRate;                       // tăng sát thương
        }
    }
    public GameObject GetWeapon(){
        DropFeature.RefWeaponDext = GetRandomNumber(DropFeature.WeaponDrop.Length); // lấy ngẫu nhiên số tương ứng
        if (DropFeature.weaponList.weaponList.weapons[DropFeature.RefWeaponDext].Avaliable) return GetWeapon();
        else return DropFeature.WeaponDrop[DropFeature.RefWeaponDext];
    }
#endregion
#region thêm thành tựu
    [SerializeField] IngameControl IC; // thành tựu 
    [SerializeField] int[] AchiveAdding; // danh sách thêm
    void AchiveIncreasing(){
        foreach (int RefAchive in AchiveAdding) IC.AchivePorgress(RefAchive, false, spawningFeature.WaveCount); // đặt thành tựu
    }
#endregion
}