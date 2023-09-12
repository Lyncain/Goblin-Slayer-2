using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
public class MainMenuControl : MonoBehaviour { // điều khiển menu chính
    [SerializeField] Transform AchivementParent;
    [SerializeField] GameObject OGAchive;
    [SerializeField] AchivementBaseInfo achivementBaseInfo;
    [SerializeField] Slider MuiscSlider, AmbeSlider, EffectSlider;
    //   chỉnh âm lượng    | nhạc nền  |   âm nền  |   hiệu ứng  |
    [SerializeField] Dropdown languageDrop; // ngôn ngữ
    [SerializeField] AudioMixer mixer; // âm thanh tổng
    LanguageText[] TextChanges; // mục tương ứng
    AchiveChange[] AchiveList; // danh sách thành tựu
    int Tracking;
    void Start(){
        Cursor.visible = false; // ẩn chuột
        Cursor.visible = true; // hiện chuột
        TextChanges = FindObjectsOfType<LanguageText>(true); // lấy Script
        CreateAchivementList();
        Keycheck();
    }
    public void LanguageChange(int LanguageIndex){ // language set
        PlayerPrefs.SetInt("CL", LanguageIndex); // đặt ngôn ngữ
        PlayerPrefs.Save(); // lưu lại
        foreach (LanguageText Languages in TextChanges) Languages.Start(); // làm mới
        foreach (AchiveChange Achive in AchiveList) Achive.RefreshAchive(); // làm mới
    }
    public void MusicAdjust(float MVal){ // điều chỉnh âm nhạc
        PlayerPrefs.SetFloat("Mval", MVal);
        mixer.SetFloat("MVol", Mathf.Log10(MVal) * 20);
        PlayerPrefs.Save();
    }
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
    void Keycheck(){
        if(!PlayerPrefs.HasKey("Has Data")){ // tạo mới dữ liệu nếu trống
            PlayerPrefs.SetInt("Has Data", 1); // xác nhận có dữ liệu
            PlayerPrefs.SetInt("CL", 0); // đặt tiếng việt
            PlayerPrefs.SetFloat("MouseVal", 0.3f * 3f); // đặt tốc độ chuột
            PlayerPrefs.SetFloat("Mval", 1f); // đặt âm nhạc
            PlayerPrefs.SetFloat("Eval", 1f); // đặt hiệu ứng
            PlayerPrefs.SetFloat("Aval", 1f); // đặt âm nền
            for (Tracking = 0; Tracking < AchiveList.Length; Tracking++) PlayerPrefs.SetInt("Achivement " + Tracking, 0);
            PlayerPrefs.Save();
        }
        // tải cài đặt
        // hiển thị âm lượng
        languageDrop.value = PlayerPrefs.GetInt("CL"); // ngôn ngữ
        AmbeSlider.value = PlayerPrefs.GetFloat("Aval"); // âm nền
        MuiscSlider.value = PlayerPrefs.GetFloat("Mval"); // âm nhạc
        EffectSlider.value = PlayerPrefs.GetFloat("Eval"); // hiệu ứng
        // đặt âm lượng
        LanguageChange(PlayerPrefs.GetInt("CL")); // ngôn ngữ
        MusicAdjust(PlayerPrefs.GetFloat("Mval")); // âm nhạc
        AmbeianceAdjust(PlayerPrefs.GetFloat("Aval")); // âm nền
        EffectAdjust(PlayerPrefs.GetFloat("Eval")); // hiệu ứng
    }
    public void Story(){
        Invoke(nameof(Story1), 3f);
    }
    void Story1(){
        PlayerPrefs.GetInt("CurMode", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene(1);
    }
    public void Survival(){
        Invoke(nameof(Survival1), 3f);
    }
    void Survival1(){
        PlayerPrefs.GetInt("CurMode", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(UnityEngine.Random.Range(4,6));
    }
    public void ConfirmClearData(){
        // tiến hành đặt lại dữ liệu
        for (Tracking = 0; Tracking < AchiveList.Length; Tracking++){
            PlayerPrefs.SetInt("Achivement " + Tracking, 0); // đặt lại tiến trình
            PlayerPrefs.Save(); // lưu lại
            AchiveList[Tracking].RefreshAchive();
        }
    }
    public void QuitGame(){
        Invoke(nameof(QuitGame1), 2.5f);
    }
    void QuitGame1(){
        Application.Quit();
    }
    public void CreateAchivementList(){
        Array.Resize(ref AchiveList, achivementBaseInfo.achivements.Length); // đặt số danh sách thành tựu
        for (Tracking = 0; Tracking < achivementBaseInfo.achivements.Length; Tracking++){
            AchiveList[Tracking] = Instantiate(OGAchive, AchivementParent).GetComponent<AchiveChange>(); // tạo và lưu
            AchiveList[Tracking].AchiveIndext = Tracking; // đặt mục
            AchiveList[Tracking].RefreshAchive(); // làm mới
        }
    }
#region nhạc nền
    [SerializeField] Text MusicNames;
    [SerializeField] AudioSource MusicScource;
    [SerializeField] bool Isplaying;
    void Update(){
        if (!MusicScource.isPlaying && Isplaying){
            Invoke(nameof(RepeatMusic), 5f);
            Isplaying = false;
        }
    }
    void RepeatMusic(){
        MusicScource.Play(); // chạy nhạc
        // đổi lại tên nhạc
        if (PlayerPrefs.GetInt("CL") == 0) MusicNames.text = "Nhạc nền: " + MusicScource.clip.name;
        else MusicNames.text = "Music: " + MusicScource.clip.name;
        Isplaying = true;
    }
#endregion
}