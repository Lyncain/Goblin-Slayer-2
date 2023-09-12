using UnityEngine;
using UnityEngine.UI;
public class AchiveChange : MonoBehaviour { // sử dụng trong menu chính
// gắn vào mục thành tựu, có thể thay đổi theo lệnh
    public int AchiveIndext;
    [SerializeField] AchivementBaseInfo AchiveBaseInfor;
    [SerializeField] Image AchiveImg;
    [SerializeField] Text AchiveName, AchiveInfo, AchiveComment, AchiveProgres;
    [SerializeField] CanvasGroup AlphaGroup;
    public void RefreshAchive(){
        AchiveImg.sprite = AchiveBaseInfor.achivements[AchiveIndext].Icon;
        if (PlayerPrefs.GetInt("CL") == 0) { // đổi ngôn ngữ: tên, thông tin, bình luận
            AchiveName.text = AchiveBaseInfor.achivements[AchiveIndext].VName;
            AchiveInfo.text = AchiveBaseInfor.achivements[AchiveIndext].VInfor;
            AchiveComment.text = AchiveBaseInfor.achivements[AchiveIndext].VComment;
        } else if (PlayerPrefs.GetInt("CL") == 1){
            AchiveName.text = AchiveBaseInfor.achivements[AchiveIndext].EName;
            AchiveInfo.text = AchiveBaseInfor.achivements[AchiveIndext].EInfo;
            AchiveComment.text = AchiveBaseInfor.achivements[AchiveIndext].EComment;
        }
        if (PlayerPrefs.GetInt("Achivement " + AchiveIndext) >= AchiveBaseInfor.achivements[AchiveIndext].TotalProgress){
            AchiveProgres.text = AchiveBaseInfor.achivements[AchiveIndext].TotalProgress + " / " + AchiveBaseInfor.achivements[AchiveIndext].TotalProgress;     
            AlphaGroup.alpha = 1f;
            AchiveComment.enabled = true;
        } else { // tiến trình chưa hoàn thành
            AchiveProgres.text = PlayerPrefs.GetInt("Achivement " + AchiveIndext) + " / " + AchiveBaseInfor.achivements[AchiveIndext].TotalProgress;
            AchiveComment.enabled = false;
            AlphaGroup.alpha = 0.7f;
        }
    }
}