using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float fastMoveSpeed = 15.0f;
    public float rotationSpeed = 3.0f;
    public float zoomSpeed = 10.0f;

    private Vector3 lastMousePosition;

    void Update()
    {
        // 鼠标右键旋转
        if (Input.GetMouseButton(1))  // 右键
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            float yaw = mouseDelta.x * rotationSpeed * Time.deltaTime;
            float pitch = -mouseDelta.y * rotationSpeed * Time.deltaTime;
            transform.eulerAngles += new Vector3(pitch, yaw, 0);
        }

        // 鼠标中键平移
        if (Input.GetMouseButton(2))  // 中键
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            Vector3 move = new Vector3(-mouseDelta.x, -mouseDelta.y, 0) * Time.deltaTime * moveSpeed;
            transform.Translate(move, Space.Self);
        }

        // 滚轮缩放
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            transform.Translate(Vector3.forward * scroll * zoomSpeed, Space.Self);
        }

        // 键盘WASD移动
        float moveMultiplier = Input.GetKey(KeyCode.LeftShift) ? fastMoveSpeed : moveSpeed;
        Vector3 moveDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) moveDirection += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) moveDirection += Vector3.back;
        if (Input.GetKey(KeyCode.A)) moveDirection += Vector3.left;
        if (Input.GetKey(KeyCode.D)) moveDirection += Vector3.right;
        if (Input.GetKey(KeyCode.Q)) moveDirection += Vector3.down;
        if (Input.GetKey(KeyCode.E)) moveDirection += Vector3.up;

        transform.Translate(moveDirection * moveMultiplier * Time.deltaTime, Space.Self);

        lastMousePosition = Input.mousePosition;
    }
}
