using UnityEngine;
[CreateAssetMenu(fileName = "Achivement Lists", menuName = "New Achivement List")]
public class AchivementBaseInfo : ScriptableObject { //sử dụng để chứa danh sách thành tựu
    public Achivement[] achivements; // tạo danh sách thành tựu với những thông tin bên dưới
    [System.Serializable] // cho phép viết dữ liệu
    public class Achivement { // thông tin của thành tựu
        // bản tiếng việt
        public string    Indext   , VName,   VInfor  , VComment;
        //            | số thứ tự |  tên | thông tin | bình luận |
        // bản tiếng anh
        public string EName,   EInfo,     EComment;
        //           | tên | thông tin | bình luận |
        public Sprite Icon; // ảnh
        public int TotalProgress; // tiến trình
    }
}