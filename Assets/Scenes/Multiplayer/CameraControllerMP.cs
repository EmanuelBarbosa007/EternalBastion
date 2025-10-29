using UnityEngine;
using Unity.Netcode;


public class CameraControllerMP : NetworkBehaviour
{
    // --- Copie TODAS as variáveis do seu CameraController.cs original para aqui ---
    public float panSpeed = 20f;
    public float panBorderThickness = 10f;
    public Vector2 panLimitMin; // Defina limites X e Z mínimos no Inspector
    public Vector2 panLimitMax; // Defina limites X e Z máximos no Inspector
    public float scrollSpeed = 20f;
    public float minY = 15f; // NOVO: Valor mais baixo para mais zoom
    public float maxY = 80f; // NOVO: Valor mais baixo para menos zoom

    // --- Lógica de Controlo de Rede ---
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            gameObject.name = $"CameraController - Local - ID {OwnerClientId}";
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cam.enabled = true;
            AudioListener listener = GetComponentInChildren<AudioListener>();
            if (listener != null) listener.enabled = true;

            // NOVO: Define a posição inicial com uma altura média, não a máxima
            float startY = (minY + maxY) / 2.0f;

            if (OwnerClientId == NetworkManager.ServerClientId) // Host (Jogador A)
            {
                transform.position = new Vector3(panLimitMin.x + 10, startY, panLimitMin.y + 10);
            }
            else // Cliente (Jogador B)
            {
                transform.position = new Vector3(panLimitMax.x - 10, startY, panLimitMax.y - 10);
            }
        }
        else
        {
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
        if (Input.GetKey("w"))
        {
            pos.z -= panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("s") )
        {
            pos.z += panSpeed * Time.deltaTime;
        }

        if (Input.GetKey("d") )
        {
            pos.x -= panSpeed * Time.deltaTime; 
        }
        if (Input.GetKey("a") )
        {
            pos.x += panSpeed * Time.deltaTime; 
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