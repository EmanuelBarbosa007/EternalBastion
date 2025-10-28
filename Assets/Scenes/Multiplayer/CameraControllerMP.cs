using UnityEngine;
using Unity.Netcode;

// Assume que o seu CameraController original se chamava CameraController
public class CameraControllerMP : NetworkBehaviour
{
    // --- Copie TODAS as variáveis do seu CameraController.cs original para aqui ---
    public float panSpeed = 20f;
    public float panBorderThickness = 10f;
    public Vector2 panLimitMin; // Defina limites X e Z mínimos no Inspector
    public Vector2 panLimitMax; // Defina limites X e Z máximos no Inspector
    public float scrollSpeed = 20f;
    public float minY = 20f; // Zoom mínimo (altura Y)
    public float maxY = 120f; // Zoom máximo (altura Y)

    // --- Lógica de Controlo de Rede ---
    public override void OnNetworkSpawn()
    {
        // Ações que acontecem quando este objeto spawna na rede

        if (IsOwner) // Este objeto pertence ao jogador local?
        {
            // Sim! Ativa a câmara e define posição inicial
            gameObject.name = $"CameraController - Local - ID {OwnerClientId}";
            Camera cam = GetComponentInChildren<Camera>(); // Procura câmara filha
            if (cam != null) cam.enabled = true;
            AudioListener listener = GetComponentInChildren<AudioListener>(); // Procura listener filho
            if (listener != null) listener.enabled = true;

            // Define posição inicial diferente para cada jogador
            if (OwnerClientId == NetworkManager.ServerClientId) // Host (Jogador A)
            {
                // Exemplo: Posição inicial para Jogador A (ajuste conforme o seu mapa)
                transform.position = new Vector3(panLimitMin.x + 10, maxY, panLimitMin.y + 10);
            }
            else // Cliente (Jogador B)
            {
                // Exemplo: Posição inicial para Jogador B (ajuste conforme o seu mapa)
                transform.position = new Vector3(panLimitMax.x - 10, maxY, panLimitMax.y - 10);
            }
        }
        else // Este objeto pertence a outro jogador
        {
            // Não! Desativa a câmara e o listener para evitar conflitos
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

        // Atualiza a posição da câmara
        transform.position = pos;
    }
}