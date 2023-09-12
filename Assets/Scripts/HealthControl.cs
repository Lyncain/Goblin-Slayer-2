using UnityEngine.UI;
using UnityEngine;
using System.Collections;
public class HealthControl : MonoBehaviour {
    public float  MaxHealth, Health, HealRate  ;
    // thông số: |    lượng máu    | tỉ lệ hồi |
    public bool Alive, CanTakeDamage;           // điều kiện còn sống, nhận sát thương
    [SerializeField] Image HealthBar;           // thanh máu
    [SerializeField] Text HealthText;           // số máu, số sát thương
    [SerializeField] SoundManager Sound;        // âm thanh
    public Coroutine ChangeCour, HealCour;      // bộ đếm tránh spam
    float ChangeElaps, PreHealth, AtHealth;
    public void TakeDamage(float Amount, Color DmgColor){   // nhận sát thương
        if (CanTakeDamage && Alive){
            Amount = (int)(Amount);                         // làm tròn sát thương
            Sound.PlayRandom("TakeDamage");                 // âm thanh nhận sát thương
            PreHealth = Health;                             // lưu lại lượng máu trước
            Health -= Amount;                               // nhận sát thương - làm tròn sát thương nhận
            Health = Mathf.Clamp(Health, 0, MaxHealth);     // giới hạn máu
            if (Health <= 0){
                Alive = CanTakeDamage = false;              // đặt điều kiện máu
                StopAllCoroutines();                        // dừng toàn bộ bộ đếm phụ
            }
            if (ChangeCour != null) StopCoroutine(ChangeCour); // kiểm tra bộ đếm
            ChangeCour = StartCoroutine(HealthShow());      // hiển thị lại lượng máu
            if (entityDamage.ShowDamage){                   // hiện sát thương
                entityDamage.DamageText.color = DmgColor;   // đổi màu
                entityDamage.DamageText.text = Amount.ToString("0"); // đặt số
                // đặt vị trí ngẫu nhiên
                entityDamage.DamageText.rectTransform.localPosition = new Vector3(Random.Range(-entityDamage.ShowRange, entityDamage.ShowRange), Random.Range(-entityDamage.ShowRange, entityDamage.ShowRange), 0f);
                entityDamage.DamageAnimation.Play();        // hoạt ảnh phụ
            }
        }
    }
    public void SetHealth(float Amount){                    // đặt lại lượng máu
        Health = MaxHealth = Amount;                        // đặt lượng máu
        ShowHealth(MaxHealth);                              // hiển thị lượng máu
        Alive = CanTakeDamage = true;                       // tránh lỗi vừa xuất hiện đã chết
    }
    public void HealthRestore(float Amount){                // hồi phục lập tức
        PreHealth = Health;                                 // đặt số thay đổi lần cuối
        Health += Amount;                                   // hồi phục
        Health = Mathf.Clamp(Health, 0, MaxHealth);         // giới hạn máu
        if (ChangeCour != null) StopCoroutine(ChangeCour);  // kiểm tra bộ đếm
        ChangeCour = StartCoroutine(HealthShow());          // hiển thị theo thời gian
    }
    public void ActiveRegen(int RecoverPoint, float RecoverRate){ // hồi phục theo thời gian
        if (HealCour != null) StopCoroutine(HealCour);
        HealCour = StartCoroutine(RegenHealth(RecoverPoint, RecoverRate));
    }
    public void ShowHealth(float Amount){   // hiển thị lại lượng máu
        HealthBar.fillAmount = Amount / MaxHealth;              // độ đầy
        HealthText.text = (int)(Amount) + " / " + (int)(MaxHealth); // số lượng
    }
    IEnumerator RegenHealth(int RecoverPoint, float RecoverRate){ // hồi máu theo thời gian
        yield return new WaitForSeconds(2f); // tránh lặp
        while(RecoverPoint > 0){
            RecoverPoint--;                 // giảm điểm dự trữ
            Health++;                       // tăng máu
            Health = Mathf.Clamp(Health, 0, MaxHealth); // giới hạn máu
            ShowHealth(Health);             // hiển thị lại
            yield return new WaitForSeconds(RecoverRate);
        }
        yield return null;
    }
    IEnumerator HealthShow(){   // Hiển thị thay đổi theo thời gian
        ChangeElaps = 0f;       // đặt lại số đếm
        AtHealth = Health;      // đặt
        while (ChangeElaps < 0.3f){
            ShowHealth(Mathf.Lerp(PreHealth, AtHealth, ChangeElaps / 0.3f));
            ChangeElaps += Time.deltaTime;
            yield return null;
        }
        ShowHealth(Health);     // hiển thị lại lần cuối
    }
    public EntityDamage entityDamage;
    [System.Serializable] public class EntityDamage {
        public bool ShowDamage;         // hiện sát thương
        public float ShowRange;         // tầm ngẫu nhiên
        public Text DamageText;         // hiện sát thương
        public Animation DamageAnimation; // hoạt ảnh nhận sát thương
    }
}