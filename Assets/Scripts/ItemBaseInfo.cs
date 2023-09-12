using UnityEngine;
[CreateAssetMenu(fileName = "Item List", menuName = "New Item List")]
public class ItemBaseInfo : ScriptableObject {
    public Weapons[] weapons;
    public Item[] items;
    [System.Serializable] public class Item {
        public Sprite Image; // ảnh đại diện
        public string VName, EName; // tên
        public GameObject Object; // vật thể
        public Color color; // màu chữ
    }
    [System.Serializable] public class Weapons {
        public Item Info; // thông tin
        public float MinDamage, MaxDamage, AtkRange; // sát thương, tầm tấn công
        public int ActionLimits; // số hoạt ảnh tối đa
        [Range(0, 100)] public int CritChance; // tỉ lệ chí mạng
        [Range(0,3)] public float AtkSpeed, CritDamage; // tốc độ tấn công, sát thương chí mạng
    }
}