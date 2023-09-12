using UnityEngine;
using UnityEngine.UI;
public class LanguageText : MonoBehaviour {
    // thay đổi ngôn ngữ
    public string VText, EText;
    public void Start(){
        if (PlayerPrefs.GetInt("CL") == 0){
            GetComponent<Text>().text = VText;
        } else if (PlayerPrefs.GetInt("CL") == 1) {
            GetComponent<Text>().text = EText;
        }
    }
}