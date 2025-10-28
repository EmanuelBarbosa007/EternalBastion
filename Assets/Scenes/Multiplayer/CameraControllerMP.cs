using UnityEngine;
using Unity.Netcode;

// Assume que o seu CameraController original se chamava CameraController
public class CameraControllerMP : NetworkBehaviour
{
    // --- Copie TODAS as vari�veis do seu CameraController.cs original para aqui ---
    public float panSpeed = 20f;
    public float panBorderThickness = 10f;
    public Vector2 panLimitMin; // Defina limites X e Z m�nimos no Inspector
    public Vector2 panLimitMax; // Defina limites X e Z m�ximos no Inspector
    public float scrollSpeed = 20f;
    public float minY = 20f; // Zoom m�nimo (altura Y)
    public float maxY = 120f; // Zoom m�ximo (altura Y)

    // --- L�gica de Controlo de Rede ---
    public override void OnNetworkSpawn()
    {
        // A��es que acontecem quando este objeto spawna na rede

        if (IsOwner) // Este objeto pertence ao jogador local?
        {
            // Sim! Ativa a c�mara e define posi��o inicial
            gameObject.name = $"CameraController - Local - ID {OwnerClientId}";
            Camera cam = GetComponentInChildren<Camera>(); // Procura c�mara filha
            if (cam != null) cam.enabled = true;
            AudioListener listener = GetComponentInChildren<AudioListener>(); // Procura listener filho
            if (listener != null) listener.enabled = true;

            // Define posi��o inicial diferente para cada jogador
            if (OwnerClientId == NetworkManager.ServerClientId) // Host (Jogador A)
            {
                // Exemplo: Posi��o inicial para Jogador A (ajuste conforme o seu mapa)
                transform.position = new Vector3(panLimitMin.x + 10, maxY, panLimitMin.y + 10);
            }
            else // Cliente (Jogador B)
            {
                // Exemplo: Posi��o inicial para Jogador B (ajuste conforme o seu mapa)
                transform.position = new Vector3(panLimitMax.x - 10, maxY, panLimitMax.y - 10);
            }
        }
        else // Este objeto pertence a outro jogador
        {
            // N�o! Desativa a c�mara e o listener para evitar conflitos
            gameObject.name = $"CameraController - Remote - ID {OwnerClientId}";
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cam.enabled = false;
            AudioListener listener = GetComponentInChildren<AudioListener>();
            if (listener != null) listener.enabled = false;
        }
    }


    void Update()
    {
        
        if (!IsOwner)
        {
            
            return;
        }

        
        Vector3 pos = transform.position;

        // Movimento com Teclado ou Rato na Borda
        if (Input.GetKey("w") || Input.mousePosition.y >= Screen.height - panBorderThickness)
        {
            pos.z += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("s") || Input.mousePosition.y <= panBorderThickness)
        {
            pos.z -= panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("d") || Input.mousePosition.x >= Screen.width - panBorderThickness)
        {
            pos.x += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("a") || Input.mousePosition.x <= panBorderThickness)
        {
            pos.x -= panSpeed * Time.deltaTime;
        }

        // Zoom com Scroll do Rato
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * scrollSpeed * 100f * Time.deltaTime; 

        // Aplica Limites
        pos.x = Mathf.Clamp(pos.x, panLimitMin.x, panLimitMax.x);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = Mathf.Clamp(pos.z, panLimitMin.y, panLimitMax.y); 

        // Atualiza a posi��o da c�mara
        transform.position = pos;
    }
}