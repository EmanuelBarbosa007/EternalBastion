using UnityEngine;
using UnityEngine.UI;

public class ConsumableManager : MonoBehaviour
{
    [Header("Configurações da Bomba")]
    public GameObject bombPrefab; // O prefab da bomba criado no passo anterior
    public int bombCost = 15;     // Custo pedido: 15 moedas
    public Button bombButton;     // O botão da UI

    [Header("Configurações do Caminho")]
    public LayerMask pathLayer;   // Layer onde estão os objetos do "chão/estrada"
    public string roadTag = "Path"; // Tag dos objetos do caminho para destacar
    public Color highlightColor = Color.yellow; // Cor do destaque

    private bool isPlacingBomb = false;
    private GameObject[] roadSegments;
    private Color[] originalColors;

    void Start()
    {
        roadSegments = GameObject.FindGameObjectsWithTag(roadTag);

        // Guarda as cores originais para repor depois
        if (roadSegments.Length > 0)
        {
            originalColors = new Color[roadSegments.Length];
            for (int i = 0; i < roadSegments.Length; i++)
            {
                Renderer rend = roadSegments[i].GetComponent<Renderer>();
                if (rend != null)
                {
                    originalColors[i] = rend.material.color;
                }
            }
        }

        // Adiciona o evento ao botão
        if (bombButton != null)
        {
            bombButton.onClick.AddListener(ToggleBombPlacement);
        }
    }

    void Update()
    {
        // Se não estamos no modo de colocar bomba, não faz nada
        if (!isPlacingBomb) return;

        // Lógica para colocar a bomba com clique esquerdo
        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceBomb();
        }

        // Opcional: Cancelar com clique direito
        if (Input.GetMouseButtonDown(1))
        {
            ToggleBombPlacement();
        }
    }

    // Chamado pelo Botão da UI
    public void ToggleBombPlacement()
    {
        isPlacingBomb = !isPlacingBomb;
        HighlightRoads(isPlacingBomb);
        Debug.Log("Modo Bomba: " + isPlacingBomb);
    }

    void TryPlaceBomb()
    {
        // Cria um raio a partir da câmara onde o rato está
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Verifica se o raio bateu na Layer do Caminho (pathLayer)
        if (Physics.Raycast(ray, out hit, 1000f, pathLayer))
        {
            // Tenta gastar o dinheiro usando o teu CurrencySystem
            if (CurrencySystem.SpendMoney(bombCost))
            {
                // Cria a bomba na posição onde clicaste
                Instantiate(bombPrefab, hit.point, Quaternion.identity);

                // Desativa o modo de colocação após colocar
                ToggleBombPlacement();
            }
            else
            {
                Debug.Log("Dinheiro insuficiente!");
                // Aqui podes adicionar um som de erro ou efeito na UI
            }
        }
        else
        {
            Debug.Log("Local inválido! Clica no caminho.");
        }
    }

    void HighlightRoads(bool highlight)
    {
        if (roadSegments == null) return;

        for (int i = 0; i < roadSegments.Length; i++)
        {
            Renderer rend = roadSegments[i].GetComponent<Renderer>();
            if (rend != null)
            {
                // Se highlight for true, pinta de amarelo, se não, volta à cor original
                rend.material.color = highlight ? highlightColor : originalColors[i];
            }
        }
    }
}