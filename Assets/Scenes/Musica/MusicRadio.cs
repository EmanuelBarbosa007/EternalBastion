using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MusicRadio : MonoBehaviour
{
    public static MusicRadio instance;

    [Header("Configurações de Áudio")]
    public AudioSource audioSource;
    public AudioClip[] musicList;
    private int currentTrackIndex = 0;

    [Header("UI References")]
    public GameObject painelControlos;
    public TextMeshProUGUI songNameText;
    public Slider volumeSlider;

    [Header("Animação do Botão")]
    public RectTransform botaoRect; 
    public float posYFechado = -460f; // Posição inicial
    public float posYAberto = -310f;  // Para onde ele vai quando abre

    private bool isPanelOpen = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        painelControlos.SetActive(false);

        volumeSlider.onValueChanged.AddListener(SetVolume);
        volumeSlider.value = 0.5f;
        audioSource.volume = 0.5f;

        if (musicList.Length > 0)
        {
            PlayTrack(0);
        }
    }

    void Update()
    {
        if (!audioSource.isPlaying && audioSource.time == 0)
        {
            NextTrack();
        }
    }

    // --- AQUI ESTÁ A MUDANÇA ---
    public void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;
        painelControlos.SetActive(isPanelOpen);

        // Atualizar a posição do botão
        Vector2 novaPosicao = botaoRect.anchoredPosition;

        if (isPanelOpen)
        {
            novaPosicao.y = posYAberto; // Sobe para -310
        }
        else
        {
            novaPosicao.y = posYFechado; // Desce para -460
        }

        botaoRect.anchoredPosition = novaPosicao;
    }

    public void NextTrack()
    {
        currentTrackIndex++;
        if (currentTrackIndex >= musicList.Length) currentTrackIndex = 0;
        PlayTrack(currentTrackIndex);
    }

    public void PreviousTrack()
    {
        currentTrackIndex--;
        if (currentTrackIndex < 0) currentTrackIndex = musicList.Length - 1;
        PlayTrack(currentTrackIndex);
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    private void PlayTrack(int index)
    {
        if (musicList.Length == 0) return;
        audioSource.clip = musicList[index];
        audioSource.Play();

        if (songNameText != null)
            songNameText.text = musicList[index].name;
    }
}