using UnityEngine;
using UnityEngine.UI;
using TMPro; // Se usares TextMeshPro
using UnityEngine.SceneManagement;

public class MusicRadio : MonoBehaviour
{
    // --- Singleton (Para manter entre cenas) ---
    public static MusicRadio instance;

    [Header("Configurações de Áudio")]
    public AudioSource audioSource;
    public AudioClip[] musicList; // Arrasta as tuas músicas para aqui no Inspector
    private int currentTrackIndex = 0;

    [Header("UI References")]
    public GameObject painelControlos; // O painel que abre e fecha
    public TextMeshProUGUI songNameText; // Texto do nome da música
    public Slider volumeSlider;

    // Estado do painel (Aberto/Fechado)
    private bool isPanelOpen = false;

    void Awake()
    {
        // Lógica para não destruir entre cenas
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Se já existir um rádio, destrói este novo para não duplicar
            return;
        }
    }

    void Start()
    {
        // Configuração inicial
        painelControlos.SetActive(false); // Começa fechado

        // Configurar o volume inicial
        volumeSlider.onValueChanged.AddListener(SetVolume);
        volumeSlider.value = 0.5f; // Começa a 50%
        audioSource.volume = 0.5f;

        // Tocar a primeira música
        if (musicList.Length > 0)
        {
            PlayTrack(0);
        }
    }

    void Update()
    {
        // Opcional: Se a música acabar, passar para a próxima automaticamente
        if (!audioSource.isPlaying && audioSource.time == 0)
        {
            NextTrack();
        }
    }

    // --- Funções dos Botões ---

    public void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;

        // Aqui podes substituir por uma Animação (ver explicação abaixo)
        painelControlos.SetActive(isPanelOpen);
    }

    public void NextTrack()
    {
        currentTrackIndex++;
        if (currentTrackIndex >= musicList.Length)
        {
            currentTrackIndex = 0; // Volta ao início (Loop)
        }
        PlayTrack(currentTrackIndex);
    }

    public void PreviousTrack()
    {
        currentTrackIndex--;
        if (currentTrackIndex < 0)
        {
            currentTrackIndex = musicList.Length - 1; // Vai para a última
        }
        PlayTrack(currentTrackIndex);
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    // --- Lógica Interna ---

    private void PlayTrack(int index)
    {
        if (musicList.Length == 0) return;

        audioSource.clip = musicList[index];
        audioSource.Play();

        // Atualiza o texto na UI
        if (songNameText != null)
        {
            songNameText.text = musicList[index].name; // Usa o nome do ficheiro ou cria uma lista de Strings com nomes bonitos
        }
    }
}