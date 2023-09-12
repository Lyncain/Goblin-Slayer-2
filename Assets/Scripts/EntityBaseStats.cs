using UnityEngine;
using System;
[CreateAssetMenu(fileName = "Enemy BaseStat", menuName = "Enemy Base stats list")]
public class EntityBaseStats : ScriptableObject { //sử dụng để chứa thông tin gốc
    public EntityStats[] OGStats, UseStats;
    [HideInInspector] int RefIndext;
    void Awake(){
        Array.Copy(OGStats, UseStats, OGStats.Length);
    }
    public void ResetStats(){ // đặt lại chỉ số
        for (RefIndext = 0; RefIndext < OGStats.Length; RefIndext++){
            UseStats[RefIndext].MinHealth = OGStats[RefIndext].MinHealth;
            UseStats[RefIndext].MaxHealth = OGStats[RefIndext].MaxHealth;
            UseStats[RefIndext].MinDmg = OGStats[RefIndext].MinDmg;
            UseStats[RefIndext].MaxDmg = OGStats[RefIndext].MaxDmg;
        }
    }
    public void StatAdjust(float ScaleAmount){ // áp dụng chỉ số (chỉ số mới = chỉ số hiện tại * tỉ lệ)
        foreach(var RefStats in UseStats){
            RefStats.MinHealth *= ScaleAmount;
            RefStats.MaxHealth *= ScaleAmount;
            RefStats.MinDmg *= ScaleAmount;
            RefStats.MaxDmg *= ScaleAmount;
        }
    }
    [System.Serializable] public class EntityStats {
        [HideInInspector] public string name; // tên của mục tiêu
        public float MinHealth, MaxHealth, MinSpeed, MaxSpeed, MinDmg, MaxDmg, MinScale, MaxScale,  StopRange,   AttackRange,  AtkCD   , SkillCD , SkillRange  ;
        // chỉ số   |        máu         |       tốc độ      |   sát thương  |       kích cỡ     | tầm dừng lại |  tầm đánh | đòn đánh | kỹ năng | tầm kỹ năng |
        public ItemDrop[] itemDrop; // vật phẩm rơi ra
    }
    [System.Serializable] public class ItemDrop {
        public GameObject Object;
        [Range(0,100)] public float Rate; // tỉ lệ rơi
        public int MinAmount, MaxAmount, AmountPerItem; // số lượng rơi, lượng tối đa trong mỗi vật
    }
}