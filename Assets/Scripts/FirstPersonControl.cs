using UnityEngine;
public class FirstPersonControl : MonoBehaviour { // điều khiên 1st view
    // tính năng khả dụng: chạy, nhảy nhiều lần, xoay camera, FOV
    // add physics material, set dynamic, static to 0 and friction to minimum > asign physic material to collider
    public Transform      CamHolder, PlayerHolder;
    // vị trí di chuyển: |  Camera |   nhân vật  |
    public Camera _1StCamera; // sử dụng hiệu ứng
    public Rigidbody rbc; // tác động vật lý, di chuyển
    public EnableFeature enableFeature; // sử dụng tính năng
    [System.Serializable] public class EnableFeature {
        public bool        EnableJump, EnableCamera, EnableBobbing,  HideCursor  ,   LockCursor   , EnableRun, EnableRunFOV;
        // điều kiện bật: |   nhảy   |   máy quay  |    nhịp đi   | ẩn trỏ chuột | khóa trỏ chuột |    chạy  |   chạy FOV  |
    }
#region chung
    void Start(){
        // đặt vật lý
        rbc.interpolation = RigidbodyInterpolation.Interpolate; // tránh tình huống clip qua tường
        rbc.freezeRotation = true; // khóa góc xoay
        runFeature.NormalFOV = _1StCamera.fieldOfView; // đặt tầm nhìn
        walkFeature.SpeedUse = walkFeature.WalkSpeed = walkFeature.BaseSpeed; // tốc độ di chuyển thường
    }
    void LateUpdate(){
        FeatureControl();
    }
    void FixedUpdate(){
        MoveInput();
    }
    void FeatureControl(){              // sử dụng tính năng
        if (enableFeature.EnableJump) JumpInput(); // nhảy
        if (enableFeature.EnableCamera) CamsInput(); // máy quay
        if (enableFeature.EnableBobbing) Bobbing(); // nảy tầm nhìn
        if (enableFeature.EnableRun) RunInput(); // chạy
        CursorInput();
    }
    void CursorInput(){ // sử dụng con trỏ chuột
        if (enableFeature.LockCursor) Cursor.lockState = CursorLockMode.Locked; // khóa chuột
        else Cursor.lockState = CursorLockMode.None; // bỏ khóa chuột
        if (enableFeature.HideCursor) Cursor.visible = false; // ẩn chuột
        else Cursor.visible = true; // hiện chuột
    }
#endregion
#region tính năng đi, nhảy
    public BaseFeature walkFeature;
    [System.Serializable] public class BaseFeature {
        public float BaseSpeed = 150f;  // tốc độ gốc chính - đề xuất 150f
        public float jumpforce = 5f;    // lực nhảy - đề xuất 5f
        public float JumpCD = 0.3f;     // thời gian chờ cho lần nhảy tiếp - đề xuất 0.3f
        public int JumpTime = 1;        // số lần nhảy - đề xuất 1
        [Range(0.01f, 0.3f)] public float BobbingSpeed = 0.125f; // tốc độ nảy - đề xuất 0.125f
        [Range(0.01f, 0.2f)] public float BobbingRate = 0.1f; // lực nảy - đề xuất 0.1f
        [HideInInspector] public float  BobTimeCount,     BobMove    ,    WaveSlice,        BobUse;
        // thông số:                   |  thời gian | vị trí nảy đến | vị trí thời gian | sử dụng nảy |
        [HideInInspector] public float SpeedUse , WalkSpeed;
        // tốc độ:                    | sử dụng |    đi    |
        [HideInInspector] public bool Jumpable; // khả dụng nhảy
        [HideInInspector] public float JumpCount; // đợi lực nhảy
        [HideInInspector] public int Jumpleft; // số lần nhảy còn lại
        [HideInInspector] public Vector3 MoveInput, MoveUse; // di chuyển đầu vào
    }
    void MoveInput(){
        rbc.AddForce(Vector3.down * Time.deltaTime * 5); // 1 lực nhẹ kéo xuống để cho chắc chắn tiếp đất
        // lấy số đầu vào khi bấm di chuyển
        walkFeature.MoveInput = new Vector3(Input.GetAxis("Horizontal"), walkFeature.MoveInput.y, Input.GetAxis("Vertical"));
        // sửa lại hướng và tốc độ di chuyển
        walkFeature.MoveUse = PlayerHolder.transform.TransformDirection(walkFeature.MoveInput.normalized * walkFeature.SpeedUse * Time.deltaTime);
        rbc.velocity = new Vector3(walkFeature.MoveUse.x, rbc.velocity.y, walkFeature.MoveUse.z); // bắt đầu di chuyển
    }
#endregion
#region tính năng nhảy và nảy
    void JumpInput(){           // nhảy
        if (Input.GetKey(KeyCode.Space) && walkFeature.Jumpleft > 0 && walkFeature.Jumpable){
            rbc.velocity = new Vector3(rbc.velocity.x, 0, rbc.velocity.z); // đặt lại trọng lực khi nhảy
            rbc.AddForce(transform.up * walkFeature.jumpforce, ForceMode.VelocityChange); // bắt đầu nhảy
            walkFeature.Jumpleft--;
            walkFeature.Jumpable = false;
            walkFeature.JumpCount = walkFeature.JumpCD;
        }
        if (!walkFeature.Jumpable && walkFeature.JumpCount <= 0f) walkFeature.Jumpable = true;
        else if (!walkFeature.Jumpable && walkFeature.JumpCount > 0f) walkFeature.JumpCount -= Time.deltaTime;
    }
    void OnCollisionEnter(Collision other) { // khi va chạm với vật
        if (other.collider.CompareTag("Ground")){
            walkFeature.Jumpleft = walkFeature.JumpTime; // đặt lại số lần nhảy
            walkFeature.Jumpable = true; // khả dụng nhảy
            walkFeature.JumpCount = 0f; // đặt lại thời gian hồi
        }
    }
    void Bobbing(){             // nảy
        if (walkFeature.MoveInput != Vector3.zero){ // kiểm tra điều kiện
            walkFeature.WaveSlice = Mathf.Sin(walkFeature.BobTimeCount); // lấy lực
            walkFeature.BobTimeCount = walkFeature.BobTimeCount + walkFeature.BobUse; // lấy thời gian đã qua
            if (walkFeature.BobTimeCount > Mathf.PI * 2) walkFeature.BobTimeCount = walkFeature.BobTimeCount - (Mathf.PI * 2); // giới hạn vị trí thời gian
        } else walkFeature.BobTimeCount = 0.0f;
        if (walkFeature.WaveSlice != 0){
            // lấy vị trí nảy tới
            walkFeature.BobMove = (walkFeature.WaveSlice * -walkFeature.BobbingRate) * Mathf.Clamp(Mathf.Abs(walkFeature.MoveInput.x) + Mathf.Abs(walkFeature.MoveInput.z), 0.0f, 1.0f);
            // di chuyển đến vị trí nảy tới
            CamHolder.localPosition = new Vector3(CamHolder.localPosition.x, walkFeature.BobMove, CamHolder.localPosition.z);
        }
    }
#endregion
#region tính năng chạy
    public RunFeature runFeature;
    [System.Serializable] public class RunFeature {
        [Range(1.2f, 2f)] public float RunForce = 1.5f; // tốc độ chạy - đề xuất 1.5f
        [Range(1.1f, 1.75f)] public float BobForce = 1.25f; // tốc độ nảy khi chạy - đề xuất 1.25f
        public float RunAcceleration = 10f; // thời gian tăng tốc chạy - đề xuất 10f
        [Range(1.02f, 1.1f)] public float FOVForce = 1.05f; // lực thay đổi tầm nhìn khi chạy - đề xuất 1.05f
        [Range(1f, 10f)] public float FOVLerpTime = 7f; // thời gian thay đổi tầm nhìn - đề xuất 7f
        [HideInInspector] public float NormalFOV;
        // số tầm nhìn:               |    đi   |
    }
    public void RunInput(){ // đi và chạy
        if (Input.GetKey(KeyCode.LeftShift) && walkFeature.MoveInput != Vector3.zero){   // chạy
            walkFeature.SpeedUse = Mathf.Lerp(walkFeature.SpeedUse, walkFeature.BaseSpeed * runFeature.RunForce, runFeature.RunAcceleration * Time.deltaTime); // tăng tốc
            // tăng tầm nhìn
            if (enableFeature.EnableRunFOV) _1StCamera.fieldOfView = Mathf.Lerp(_1StCamera.fieldOfView, runFeature.NormalFOV * runFeature.FOVForce, runFeature.FOVLerpTime * Time.deltaTime);
            walkFeature.BobUse = walkFeature.BobbingSpeed * runFeature.BobForce; // đặt tốc độ nảy
        } else {                                // đi
            walkFeature.SpeedUse = Mathf.Lerp(walkFeature.SpeedUse, walkFeature.WalkSpeed, runFeature.RunAcceleration * Time.deltaTime); // giảm tốc
            // giảm tầm nhìn
            if (enableFeature.EnableRunFOV) _1StCamera.fieldOfView = Mathf.Lerp(_1StCamera.fieldOfView, runFeature.NormalFOV , runFeature.FOVLerpTime * Time.deltaTime);
            walkFeature.BobUse = walkFeature.BobbingSpeed / runFeature.BobForce; // đặt tốc độ nảy
        }
    }
#endregion
#region sử dụng máy quay
    public CameraFeature cameraFeature;
    [System.Serializable] public class CameraFeature {
        public float MouseSensitive = 1.5f; // tốc độ chuột, đề xuất 2
        [Range(10, 90)] public float MaxDownView = 85f; // giới hạn tầm nhìn xuống
        [Range(-10, -90)] public float MaxUpView = -85; // giới hạn tầm nhìn lên
        [HideInInspector] public Vector2 RotateInput, RotateUse; // đầu vào khi di chuột
    }
    public void CamsInput(){
        if (enableFeature.EnableCamera){
            // lấy số gốc
            cameraFeature.RotateInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            // lấy số thay đổi
            cameraFeature.RotateUse = new Vector2(cameraFeature.RotateUse.x -= cameraFeature.RotateInput.y * cameraFeature.MouseSensitive, cameraFeature.RotateUse.y += cameraFeature.RotateInput.x * cameraFeature.MouseSensitive);
            // giới hạn góc trái phải
            if (cameraFeature.RotateUse.y > 180f) cameraFeature.RotateUse.y = -180f; else if (cameraFeature.RotateUse.y < -180f) cameraFeature.RotateUse.y = 180f;
            // giới hạn góc nhìn lên xuống để tránh gãy cổ
            cameraFeature.RotateUse.x = Mathf.Clamp(cameraFeature.RotateUse.x, cameraFeature.MaxUpView, cameraFeature.MaxDownView);
            // bắt đầu xoay góc nhìn ngang
            CamHolder.localRotation = Quaternion.Euler(cameraFeature.RotateUse.x, 0f, 0f);
            // bắt đầu xoay góc nhìn dọc
            PlayerHolder.transform.rotation = Quaternion.Euler(0, cameraFeature.RotateUse.y, 0);
        }
    }
#endregion
}