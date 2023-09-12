using UnityEngine;
using UnityEngine.Audio;
using System;
public class SoundManager : MonoBehaviour {
    public MusicAssets[] Sounds; // danh sách âm thanh
    MusicAssets RefSound; // âm thanh tương ứng
    void Awake(){
        foreach (MusicAssets RefSound in Sounds){                       // tạo danh sách âm thanh
            RefSound.source = gameObject.AddComponent<AudioSource>();   // thêm nguồn
            RefSound.source.clip = RefSound.Clip;                       // đặt đoạn âm thanh
            RefSound.source.outputAudioMixerGroup = RefSound.output;    // đặt loại âm thanh
            RefSound.source.volume = RefSound.Volume;                   // đặt âm lượng
            RefSound.source.pitch = RefSound.Pitch;                     // đặt âm mạnh
            RefSound.source.loop = RefSound.loop;                       // điều kiện lặp lại
            RefSound.source.spatialBlend = RefSound.Set2DTo3D;          // 2D - 3D
        }
    }
    public void Play(string Name){              // chơi âm thanh thường
        RefSound = Array.Find(Sounds, sound => sound.AudioName == Name); // tìm tên
        if (RefSound != null) RefSound.source.Play(); // điều kiện tồn tại
        else {
            Debug.Log("Sound Unfound: " + Name);
            return; // quay lại nếu không có
        }
    }
    public void PlayRandom(string Name){        // chơi âm thanh với cường độ ngẫu nhiên
        RefSound = Array.Find(Sounds, sound => sound.AudioName == Name); // tìm tên
        if (RefSound != null){
            // đặt cường độ ngâu nhiên
            RefSound.source.pitch = RefSound.Pitch + UnityEngine.Random.Range(-RefSound.RandomPitch, RefSound.RandomPitch);
            RefSound.source.Play(); // điều kiện tồn tại
        } else Debug.Log("Sound Unfound: " + Name);
    }
    [System.Serializable][HideInInspector] public class MusicAssets { //lưu trữ âm thanh
        public string AudioName;                        // tên âm thanh
        public AudioClip Clip;                          // đoạn âm thanh
        public bool loop;                               // điều kiện chơi lại
        public AudioMixerGroup output;                  // loại âm thanh (hiệu ứng, âm nền, nhạc nền)
        [Range(0.01f, 1f)] public float Volume = 1f;    // âm lượng âm thanh
        [Range(0.1f, 3f)] public float Pitch = 1f;      // âm mạnh của âm thanh
        [Range(0.1f, 1f)] public float RandomPitch = 0.1f; // mức độ ngẫu nhiên
        [Range(0.001f, 1f)] public float Set2DTo3D = 0.01f; // mức độ 2D và 3D
        [HideInInspector] public AudioSource source;    // nguồn âm thanh
    }
}