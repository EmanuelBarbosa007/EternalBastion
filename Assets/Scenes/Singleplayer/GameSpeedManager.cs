using UnityEngine;
using TMPro;

public class GameSpeedManager : MonoBehaviour
{
    [Header("Speed Settings")]
    public float normalSpeed = 1f;
    public float fastSpeed = 2f;

    [Header("UI")]
    public TextMeshProUGUI speedButtonText; //para o texto "x1" / "x2"

    private bool isFastForward = false;

    void Start()
    {
        // Garante que o jogo começa na velocidade normal
        isFastForward = false;
        ApplyCurrentSpeed();
    }


    public void ToggleSpeed()
    {
        // Não faz nada se o jogo estiver pausado
        if (EscMenuManager.IsGamePaused)
        {
            return;
        }

        isFastForward = !isFastForward;
        ApplyCurrentSpeed();
    }

    public void ApplyCurrentSpeed()
    {
        // Se o jogo estiver pausado, Time.timeScale deve ser 0
        if (EscMenuManager.IsGamePaused)
        {
            Time.timeScale = 0f;
            return;
        }

        if (isFastForward)
        {
            Time.timeScale = fastSpeed;
            if (speedButtonText != null)
                speedButtonText.text = "x2";
        }
        else
        {
            Time.timeScale = normalSpeed;
            if (speedButtonText != null)
                speedButtonText.text = "x1";
        }
    }
}