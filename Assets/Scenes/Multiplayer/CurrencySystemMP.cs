using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;
using System;

public class CurrencySystemMP : NetworkBehaviour
{
    public static CurrencySystemMP Instance;

    public int startingMoney = 150;

    // Referências aos textos de UI de cada jogador
    public TextMeshProUGUI moneyTextA;
    public TextMeshProUGUI moneyTextB;

    private NetworkVariable<int> moneyJogadorA = new NetworkVariable<int>(); // Host (ID 0)
    private NetworkVariable<int> moneyJogadorB = new NetworkVariable<int>(); // Client (ID 1+)

    public int MoneyJogadorB => moneyJogadorB.Value;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            moneyJogadorA.Value = startingMoney;
            moneyJogadorB.Value = startingMoney;
        }

        // Todos os clientes atualizam o UI quando o dinheiro muda
        moneyJogadorA.OnValueChanged += (prev, next) => UpdateUI();
        moneyJogadorB.OnValueChanged += (prev, next) => UpdateUI();
        UpdateUI();
    }

    // --- Funções chamadas APENAS NO SERVER ---

    public void AddMoney(ulong clientId, int amount)
    {
        if (!IsServer) return;

        if (clientId == 0) // Jogador A (Host)
        {
            moneyJogadorA.Value += amount;
        }
        else // Jogador B
        {
            moneyJogadorB.Value += amount;
        }
    }

    public bool SpendMoney(ulong clientId, int amount)
    {
        if (!IsServer) return false;

        if (clientId == 0) // Jogador A
        {
            if (moneyJogadorA.Value >= amount)
            {
                moneyJogadorA.Value -= amount;
                return true;
            }
        }
        else // Jogador B
        {
            if (moneyJogadorB.Value >= amount)
            {
                moneyJogadorB.Value -= amount;
                return true;
            }
        }
        return false; // Não tem dinheiro
    }


    public int GetMoney(ulong clientId)
    {
        if (clientId == 0) // Jogador A
        {
            return moneyJogadorA.Value;
        }
        else // Jogador B
        {
            return moneyJogadorB.Value;
        }
    }


    //UI
    void UpdateUI()
    {
        if (moneyTextA != null)
            moneyTextA.text = "Moedas A: " + moneyJogadorA.Value;

        if (moneyTextB != null)
            moneyTextB.text = "Moedas B: " + moneyJogadorB.Value;
    }
}