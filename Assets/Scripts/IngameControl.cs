using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
public class IngameControl : MonoBehaviour {
#region cài đặt, tạm dừng
    [SerializeField] GameObject PauseBoard; // bảng tạm dừng chơi
    [SerializeField] bool Pausing; // xác định là màn hình chính, điều kiện tạm dừng
    [SerializeField] Slider AmbeSlider, EffectSlider, MouseSlider;
    //   chỉnh âm lượng    |  âm nền  |   hiệu ứng  |    chuột   |
    [SerializeField] AudioMixer mixer; // âm thanh tổng
    // âm thanh sẽ chỉnh thẳng từ PlayerPrefs > set value thay vì PlayerPrefs > float > set value
    // toàn màn hình và ngôn ngữ sẽ điều chỉnh và lưu thẳng vào dữ liệu thay vì xác nhận lưu
    void Start(){
        Array.Resize(ref AchiveIndextProgress, AchiveBaseData.achivements.Length); // đặt độ lớn danh sách
        // kiểm tra thành tựu
        for (int i = 0; i < AchiveIndextProgress.Length; i++)
            AchiveIndextProgress[i] = PlayerPrefs.GetInt("Achivement " + i); // đặt tiến trình thành tựu
        Pausing = true;
        PauseAction(); // bỏ tạm dừng
        // hiển thị số liệu đã lưu
        MouseSlider.value = PlayerPrefs.GetFloat("MouseVal"); // hiển thị tốc độ chuột
        AmbeSlider.value = PlayerPrefs.GetFloat("Aval"); // hiển thị âm nền
        EffectSlider.value = PlayerPrefs.GetFloat("Eval"); // hiển thị hiệu ứng
        // đặt số liệu đã lưu
        PlayerControl.cameraFeature.MouseSensitive = PlayerPrefs.GetFloat("MouseVal") * 3f; // tốc độ chuột
        mixer.SetFloat("AVol", Mathf.Log10(PlayerPrefs.GetFloat("Aval")) * 20); // âm nền
        mixer.SetFloat("EVol", Mathf.Log10(PlayerPrefs.GetFloat("Eval")) * 20); // hiệu ứng
        
        foreach(Text RefTxt in AmountCountTxt) if (RefTxt != null) RefTxt.text = "0";
        SetPlayerControl(false); // tắt khả năng di chuyển
        SceneAnimate.SetTrigger("Start"); // hoạt ảnh
        gameOverBoard.GameBoard.SetActive(false); // ẩn bảng kết thúc game
        AchiveGet.gameObject.SetActive(false);
    }
    void LateUpdate(){
        if (Input.GetKeyDown(KeyCode.Escape)) PauseAction(); // tạm dừng
        PlayerHealthCheck();
    }
    public void PauseAction(){
        if (Pausing){               // tiếp tục
            Pausing = false;        // đặt điều kiện
            Time.timeScale = 1f;    // đặt lại thời gian
            // khóa chuột
            Cursor.lockState = CursorLockMode.Locked; // khóa chuột
            Cursor.visible = false; // ẩn chuột
        } else {                    // tạm dừng
            Pausing = true;         // đặt điều kiện
            Time.timeScale = 0.001f; // đặt thời gian
            // mở khóa chuột
            Cursor.lockState = CursorLockMode.None; // bỏ khóa chuột
            Cursor.visible = true; // hiện chuột
        }
        PlayerControl.enabled = !Pausing; // khóa máy quay
        PauseBoard.SetActive(Pausing); // ẩn, hiện bảng tạm dừng
    }
    public void Restart(){
        Time.timeScale = 1f;
        Invoke(nameof(Restart1), 0.2f);
    }
    public void Restart1(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // chơi lại màn hiện tại
    }
    public void Return(){
        Pausing = true;
        PauseAction();
    }
    public void MainMenu(){
        Time.timeScale = 1f;
        Invoke(nameof(MainMenu1), 0.25f);
    }
    void MainMenu1(){
        SceneManager.LoadScene(0); // quay về màn hình chính
    }
    // hiệu chỉnh sẽ lưu trực tiếp
    public void AmbeianceAdjust(float AVal){ // điều chỉnh âm môi trường
        PlayerPrefs.SetFloat("Aval", AVal);
        mixer.SetFloat("AVol", Mathf.Log10(AVal) * 20);
        PlayerPrefs.Save();
    }
    public void EffectAdjust(float EVal){ // điều chỉnh hiệu ứng
        PlayerPrefs.SetFloat("Eval", EVal);
        mixer.SetFloat("EVol", Mathf.Log10(EVal) * 20);
        PlayerPrefs.Save();
    }
    public void MouseAdjust(float MoVal){ // điều chỉnh tốc độ chuột
        PlayerPrefs.SetFloat("MouseVal", MoVal);
        PlayerControl.cameraFeature.MouseSensitive = MoVal * 3f;
        PlayerPrefs.Save();
    }
#endregion
#region điều khiển quá trình chơi
    public PlayerCombatControl CombatSystem; // điều khiển trang bị
    [SerializeField] FirstPersonControl PlayerControl; // điều khiển di chuyển
    [SerializeField] WaveControl waveControl; // đã qua bao nhiêu đợt quái
    [SerializeField] Animator SceneAnimate; // hoạt ảnh hình nền
    public Transform EntityContain, ItemContain; // mục chưa các Entity
    [SerializeField] GameOverBoard gameOverBoard;
    [System.Serializable] public class GameOverBoard {
        public GameObject GameBoard;
        public Text WaveTxt, GoldTxt, HealthTxt, StaTxt;
    }
    public SoundManager Sounds; // âm thanh sử dụng
    void PlayerHealthCheck(){
        if (!CombatSystem.healthControl.Alive){
            SetPlayerControl(false);
            SceneAnimate.SetTrigger("Die");
            CombatSystem.healthControl.Alive = true;
            waveControl.spawningFeature.IsSpawning = false;
        }
    }
    public void SetPlayerControl(bool IsOn){
        CombatSystem.enabled = CombatSystem.WeaponAnimated.enabled = PlayerControl.enabled = waveControl.spawningFeature.EnableSpawn = IsOn;
    }
    public void EVStartIn(){
        SetPlayerControl(true);
        CombatSystem.SetWeapon(0);
        waveControl.WaitForNewSpawn();
    }
    public void EVGameOver(){
        Destroy(EntityContain.gameObject); // loại bỏ toàn bộ Entity
        Destroy(ItemContain.gameObject);   // loại bỏ toàn bộ vật phẩm
        Cursor.lockState = CursorLockMode.None; // bỏ khóa chuột
        Cursor.visible = true; // hiện chuột
        gameOverBoard.GameBoard.SetActive(true);
        // hiển thị thông số đã đạt được
        if (PlayerPrefs.GetInt("CurMode") == 1){ // lưu chỉ số đã thu thập
            gameOverBoard.WaveTxt.text = "Bạn đã vượt qua: " + AmountCount[0] + " đợt"; // số đợt đã qua
            if (PlayerPrefs.GetInt("CL") == 1)
            gameOverBoard.WaveTxt.text = "You Survived: " + AmountCount[0] + " waves"; // số đợt đã qua
            gameOverBoard.GoldTxt.text = AmountCount[1].ToString();     // lượng vàng thu thập
            gameOverBoard.HealthTxt.text = AmountCount[2].ToString();   // lượng máu thu thập
            gameOverBoard.StaTxt.text = AmountCount[3].ToString();      // lượng thể lực thu thập
        }
    }
#endregion
#region tiến trình trong khi chơi
// người chơi
    [SerializeField] float ItemShowTime; // thời gian hiển thị vật phẩm
    [SerializeField] ItemSlots[] SlotList; // danh sách mục hiển thị
    [SerializeField] Text[] AmountCountTxt; // hiển thị vật phẩm
    [SerializeField] int[] AmountCount; // danh sách số lượng đã nhặt
    // vật phẩm: | 0 = đợt | 1 = vàng | 2 = máu | 3 = thể lực |
    int Tracking; // số đếm
    // hiển thị vật phẩm vừa nhặt
    public void ShowingItem(string Name, Sprite Image, Color color, int Amount){
        if (SlotList[Tracking].Cour != null) StopCoroutine(SlotList[Tracking].Cour); // dừng bộ đếm nếu có
        // thay đổi hiển thị
        SlotList[Tracking].ItemImage.sprite = Image; // hình ảnh
        SlotList[Tracking].ItemName.text = Amount + " x " + Name; // thông tin + số lượng
        SlotList[Tracking].ItemName.color = color; // màu chữ
        SlotList[Tracking].Cour = StartCoroutine(Showing(Tracking)); // tạo bộ đếm
        Tracking++;
        if (Tracking >= SlotList.Length) Tracking = 0;
        Sounds.PlayRandom("GetItem"); // âm thanh
    }
    public void SetAmount(int Indext, int Amount){ // thay đổi số lượng nhặt
        AmountCount[Indext] += Amount;
        AmountCountTxt[Indext].text = AmountCount[Indext].ToString();
    }
    IEnumerator Showing(int Indext){
        SlotList[Indext].ItemObject.SetActive(false);
        yield return new WaitForSeconds(.2f);
        SlotList[Indext].ItemObject.SetActive(true);
        yield return new WaitForSeconds(ItemShowTime);
        SlotList[Indext].ItemObject.SetActive(false);
        SlotList[Indext].Cour = null;
        if (Indext == 0) Tracking = 0;
    }
    [System.Serializable] public class ItemSlots {
        public Coroutine Cour;
        public GameObject ItemObject; // hiển thị chính
        public Image ItemImage; // hình ảnh
        public Text ItemName; // tên
    }
#endregion
#region thành tựu
    [Space] [SerializeField] AchivementBaseInfo AchiveBaseData;
    [SerializeField] Animation AchiveGet;
    [SerializeField] Image DisplayImg;
    [SerializeField] Text DisplayName, DisplayInfo;
    int[] AchiveIndextProgress; // tiến trình thành tựu
    public void AchivePorgress(int AchiveIndext, bool IsIncrement, int AchiveSetAmount){ // phát triển thành tựu nếu chưa hoàn thành
        if(AchiveIndextProgress[AchiveIndext] < AchiveBaseData.achivements[AchiveIndext].TotalProgress){ // điều kiện chưa hoàn thành
            // đặt số liệu
            if (IsIncrement) AchiveIndextProgress[AchiveIndext] += AchiveSetAmount; // tăng thêm
            else AchiveIndextProgress[AchiveIndext] = AchiveSetAmount; // đặt
            // kiểm tra tiến trình
            if (AchiveIndextProgress[AchiveIndext] >= AchiveBaseData.achivements[AchiveIndext].TotalProgress){ // khi hoàn thành
                AchiveIndextProgress[AchiveIndext] = AchiveBaseData.achivements[AchiveIndext].TotalProgress; // đặt lại nếu vượt quá
                // đặt thông tin thành tựu vừa nhận
                if (PlayerPrefs.GetInt("CL") == 0) SetAChiveText(AchiveBaseData.achivements[AchiveIndext].Icon, AchiveBaseData.achivements[AchiveIndext].VName, AchiveBaseData.achivements[AchiveIndext].VInfor);
                else SetAChiveText(AchiveBaseData.achivements[AchiveIndext].Icon, AchiveBaseData.achivements[AchiveIndext].EName, AchiveBaseData.achivements[AchiveIndext].EInfo);
                AchiveGet.gameObject.SetActive(true);
                AchiveGet.Play(); // hoạt ảnh
            }
            PlayerPrefs.SetInt("Achivement " + AchiveIndext, AchiveIndextProgress[AchiveIndext]); // đặt số lưu thành tựu
            PlayerPrefs.Save(); // lưu lại
        }
    }
    public void SetAChiveText(Sprite NewImg, string NewName, string NewInfo){
        DisplayImg.sprite = NewImg;
        DisplayName.text = NewName;
        DisplayInfo.text = NewInfo;
        Sounds.Play("AchiveGet"); // âm thanh
    }
#endregion
}