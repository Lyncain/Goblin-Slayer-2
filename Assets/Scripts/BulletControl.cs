using UnityEngine;
public class BulletControl : MonoBehaviour {
    // chỉ áp dụng cho đạn
    [SerializeField] float Lifetime, TravelForce; // lực bắn
    float DamageTake; // sát thương
    Vector3 PositionTo;
    public void Shoot(Vector3 TargetPosition, float Damage){ // sắp đặt vật bắn
        PositionTo = (TargetPosition - transform.position).normalized; // lấy vị trí từ lần di chuyển trước
        transform.LookAt(TargetPosition);
        DamageTake = Damage; // lưu sát thương
        Destroy(gameObject, Lifetime); // xóa vật
    }
    void LateUpdate(){
        transform.position += (PositionTo * TravelForce * Time.deltaTime); // tiến về hướng chỉ định
    }
    void OnTriggerEnter(Collider other){
        if(other.CompareTag("Player")){
            other.gameObject.GetComponent<HealthControl>().TakeDamage(DamageTake, Color.clear); // nhận sát thương
            gameObject.SetActive(false);
            StopAllCoroutines(); // dừng toàn bộ đếm
            Destroy(gameObject, 1f); // xóa vật
        }
    }
}