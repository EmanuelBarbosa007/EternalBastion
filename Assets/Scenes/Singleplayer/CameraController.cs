using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 20f;        // Velocidade de movimento
    public float zoomSpeed = 200f;       //scroll para zoom
    public float minY = 10f;             // zoom mínimo
    public float maxY = 25f;             // zoom máximo

    [Header("Map Limits")]
    public float minX = -30f;
    public float maxX = 30f;
    public float minZ = -15f;
    public float maxZ = 30f;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        // Ajusta posição inicial 
        transform.position = new Vector3( -27.5f, 20f, 20f);
        transform.rotation = Quaternion.Euler(60f, 180f, 0f);
    }

    void Update()
    {
        MoveCamera();
        ZoomCamera();
    }

    void MoveCamera()
    {
        float x = 0f;
        float z = 0f;

        // WASD movement
        if (Input.GetKey(KeyCode.W)) z -= 1f; 
        if (Input.GetKey(KeyCode.S)) z += 1f; 
        if (Input.GetKey(KeyCode.A)) x += 1f; 
        if (Input.GetKey(KeyCode.D)) x -= 1f;


        Vector3 dir = new Vector3(x, 0, z).normalized;
        Vector3 move = dir * moveSpeed * Time.deltaTime;

        // Move e limita dentro do mapa
        Vector3 newPos = transform.position + move;
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.z = Mathf.Clamp(newPos.z, minZ, maxZ);

        transform.position = newPos;
        transform.position = Vector3.Lerp(transform.position, newPos, 10f * Time.deltaTime);

    }

    void ZoomCamera()
    {
        
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            Vector3 pos = transform.position;
            pos.y -= scroll * zoomSpeed * Time.deltaTime;
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.position = pos;
        }
    }
}
