using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class StoryControl : MonoBehaviour { // điều khiển cốt truyện
    [SerializeField] Animator SceneAnimation; // hoạt ảnh cho cốt truyện
    [SerializeField] GameObject StoryScene; // nhóm hiển thị cốt truyện
    [SerializeField] WaveControl waveControl;
    [SerializeField] IngameControl IC;
    [SerializeField] bool CanSkip; // điều kiện : dẫn truyện từ đầu | bỏ qua cốt truện
    [SerializeField] int WaveLimit; // số đợt tối đa
    [SerializeField] int AchiveIndextAdding; // thành tựu
    [SerializeField] SoundManager Sounds;
    [SerializeField] StoryText[] storyText;
    [System.Serializable] public class StoryText {
        public Text text;
        public string EText, VText;
    }
    void Start(){
        StoryScene.SetActive(true); // hiện nhóm cốt truyện
        foreach(var text in storyText){ // lưu ngôn ngữ
            text.text.text = text.VText;
            if (PlayerPrefs.GetInt("CL") == 1) text.text.text = text.EText;
        }
        SceneAnimation.SetTrigger("Story");
    }
    void Update(){
        if (Input.GetKeyDown(KeyCode.Escape) && CanSkip) SkipStory();
        WaveCheck();
    }
    public void SkipStory(){ // bắt đầu vào chơi
        CanSkip = false;
        StoryScene.SetActive(false); // tắt nhóm hiển thị cốt truyện
        SceneAnimation.SetTrigger("Start");
        IC.PauseAction();
    }
    void WaveCheck(){
        if (waveControl.spawningFeature.WaveCount >= WaveLimit){
            IC.AchivePorgress(AchiveIndextAdding, false, 1); // thêm thành tựu
            Invoke(nameof(ChangingScene), 5f); // chuyển địa điểm sau thời gian nhất định
            SceneAnimation.SetTrigger("FadeInBlack");
            this.enabled = false; // tắt script tránh lặp lệnh
        }
    }
    void ChangingScene(){
        if (AchiveIndextAdding == 1) SceneManager.LoadScene(0); // quay về menu
        else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // chuyển địa điểm tiếp theo
    }
    void PlaySound(){
        Sounds.PlayRandom("Popup"); // âm thanh
    }
}