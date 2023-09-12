using UnityEngine;
using System.Collections;
public class ItemsInteract : MonoBehaviour {
    // đặt trong item
    // có thể tự động thu thập sau thời gian ngẫu nhiên
    enum ItemType{ Item , Weapon } [SerializeField] ItemType itemType; // định dạng vật phẩm
    [SerializeField] int StackType, StackAmount; // số lượng
    // trang bị: | 1 = cúp  | 2 = kiếm ngắn | 3 = kiếm dài |
    // vật phẩm: | 1 = vàng | 2 = máu       | 3 = thể lực  |
    [SerializeField] ItemBaseInfo Iteminfo; // thông tin vật phẩm
    [SerializeField] Rigidbody RB;  // khả năng vật lý
    [SerializeField] Collider collide;
    string RefName; // tên tương ứng
    void Start(){
        StartCoroutine(ItemActive());
    }
    public void SetItemInfo(int NewItemAmount){ // đặt thông tin của vật
        StackAmount = NewItemAmount;
    }
    public IEnumerator ItemActive(){
        collide.enabled = false; // tắt khả năng nhặt
        // tạo vụ nổ ngẫu nhiên
        RB.AddExplosionForce(Random.Range(50f, 200f), gameObject.transform.position + new Vector3(0,0,1f), 2f);
        yield return new WaitForSeconds(2f); // thời gian chờ cho vật di chuyển
        collide.enabled = true; // bật khả năng nhặt
        yield return new WaitForSeconds(Random.Range(5f, 8f)); // thời gian chờ trước khi xóa
        IncreaseLoot();
    }
    void OnTriggerEnter(Collider other){ // nhặt vật
        if(other.gameObject.CompareTag("Player")) IncreaseLoot();
    }
    void IncreaseLoot(){
        IngameControl IC = FindObjectOfType<IngameControl>(); // tìm script
        PlayerCombatControl PCB = FindObjectOfType<PlayerCombatControl>();
        switch (itemType){ // nhặt vật phẩm
            default:
            case ItemType.Item:
                // thêm thành tựu
                if (StackType == 1) IC.AchivePorgress(13, true, StackAmount); // định dạng là vàng
                else IC.AchivePorgress(14, true, StackAmount); // định dạng là thể lực hoặc máu
                // thêm vật phẩm
                if (StackType == 2) PCB.HealthPotChange(); // thêm máu
                else if(StackType == 3) PCB.StaRestore(PCB.skillList.Sta.ResotrePoint); // hồi phục thể lực
                if (PlayerPrefs.GetInt("CL") == 0) RefName = Iteminfo.items[StackType].VName;
                else RefName = Iteminfo.items[StackType].EName;
                // hiển thị vật phẩm vừa nhặt
                IC.ShowingItem(RefName, Iteminfo.items[StackType].Image, Iteminfo.items[StackType].color, StackAmount);
                IC.SetAmount(StackType, StackAmount); // tăng số lượng
            break;
            case ItemType.Weapon:
                PCB.weaponList.weapons[StackType].Avaliable = true; // mở khóa trang bị
                if (PlayerPrefs.GetInt("CL") == 0) RefName = Iteminfo.weapons[StackType].Info.VName;
                else RefName = Iteminfo.weapons[StackType].Info.EName;
                IC.ShowingItem(RefName, Iteminfo.weapons[StackType].Info.Image, Iteminfo.weapons[StackType].Info.color, StackAmount);
            break;
        }
        gameObject.SetActive(false); // tắt vật
        Destroy(RB);           // tắt vật lý
        StopAllCoroutines();            // dừng bộ đếm phụ
        Destroy(gameObject, 1.5f);      // xóa vật sau 1.5 giây
    }
}