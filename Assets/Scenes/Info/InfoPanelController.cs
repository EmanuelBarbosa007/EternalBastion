using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class InfoPanelController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject infoPanelObject;
    public Image displayImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("Botões de Categoria")] 
    public Image btnTorreImage;
    public Image btnInimigoImage; 
    public Image btnConsumivelImage;

    [Header("Configuração de Cores")] 
    public Color corSelecionado = Color.red;
    public Color corNormal = Color.white;

    [Header("Data")]
    public List<InfoItemData> todosOsItens;

    // Variáveis internas para controlar o estado
    private List<InfoItemData> listaAtualFiltrada;
    private int indiceAtual = 0;

    void Start()
    {
        infoPanelObject.SetActive(false);
        listaAtualFiltrada = new List<InfoItemData>();
    }

    // Funções de Abertura/Fecho

    public void TogglePanel()
    {
        bool estaAtivo = infoPanelObject.activeSelf;
        infoPanelObject.SetActive(!estaAtivo);

        if (!estaAtivo)
        {
            // Ao abrir, seleciona Torre por padrão e já pinta o botão
            MudarCategoria("Torre");
        }
    }

    //  Lógica das Categorias 

    public void MudarCategoria(string categoriaNome)
    {
        //  Converter string para Enum
        InfoCategory catSelecionada;

        //  Atualiza as cores dos botões baseado na string recebida
        AtualizarCoresBotoes(categoriaNome);

        if (categoriaNome == "Torre") catSelecionada = InfoCategory.Torre;
        else if (categoriaNome == "Tropa") catSelecionada = InfoCategory.Tropa;
        else catSelecionada = InfoCategory.Consumivel;

        //  Filtrar a lista
        listaAtualFiltrada = todosOsItens.Where(x => x.categoria == catSelecionada).ToList();

        //  Resetar o índice e atualizar a tela
        indiceAtual = 0;
        AtualizarUI();
    }

    // 
    private void AtualizarCoresBotoes(string categoriaAtiva)
    {
        // reseta todos para a cor normal 
        if (btnTorreImage != null) btnTorreImage.color = corNormal;
        if (btnInimigoImage != null) btnInimigoImage.color = corNormal;
        if (btnConsumivelImage != null) btnConsumivelImage.color = corNormal;

        // pinta apenas o ativo de vermelho
        if (categoriaAtiva == "Torre" && btnTorreImage != null)
        {
            btnTorreImage.color = corSelecionado;
        }
        else if (categoriaAtiva == "Tropa" && btnInimigoImage != null)
        {
            btnInimigoImage.color = corSelecionado;
        }
        else if ((categoriaAtiva == "Consumivel" || categoriaAtiva == "Consumiveis") && btnConsumivelImage != null)
        {
            btnConsumivelImage.color = corSelecionado;
        }
    }

    // Lógica de Navegação (Setas)

    public void ProximaPagina()
    {
        if (listaAtualFiltrada.Count == 0) return;

        indiceAtual++;
        if (indiceAtual >= listaAtualFiltrada.Count)
        {
            indiceAtual = 0;
        }
        AtualizarUI();
    }

    public void PaginaAnterior()
    {
        if (listaAtualFiltrada.Count == 0) return;

        indiceAtual--;
        if (indiceAtual < 0)
        {
            indiceAtual = listaAtualFiltrada.Count - 1;
        }
        AtualizarUI();
    }

    // Atualização Visual

    private void AtualizarUI()
    {
        if (listaAtualFiltrada.Count > 0)
        {
            InfoItemData item = listaAtualFiltrada[indiceAtual];

            titleText.text = item.nomeItem;
            descriptionText.text = item.descricao;
            displayImage.sprite = item.imagem;
            displayImage.preserveAspect = true;
        }
        else
        {
            titleText.text = "Vazio";
            descriptionText.text = "Nenhum item encontrado.";
            displayImage.sprite = null;
        }
    }
}