using UnityEngine;
using Unity.Netcode;

// Garante que herda de TowerMP
public class FireTowerMP : TowerMP
{
    // Override Start para definir o nome e ler stats base
    protected override void Start()
    {
        // Chama o Start da classe base (TowerMP) para inicializar NetworkVariables
        base.Start();

        // Define o nome APENAS DEPOIS da inicializa��o base
        // N�o precisa ser NetworkVariable, � s� para o UI local
        if (IsOwner) // Ou talvez nem precise verificar IsOwner, depende de como o UI l�
        {
            towerName = "Fire Tower";
        }
    }

    // Override para guardar os stats base da FireballMP
    protected override void StoreBaseBulletStats()
    {
        if (bulletPrefab != null)
        {
            // Procura o componente FireballMP no prefab
            FireballMP fb = bulletPrefab.GetComponent<FireballMP>();
            if (fb != null)
            {
                baseBulletDamage = fb.damage; // Guarda o dano base
                baseBulletSpeed = fb.speed;   // Guarda a velocidade base
            }
            else
            {
                Debug.LogError($"O prefab '{bulletPrefab.name}' atribu�do a {gameObject.name} n�o tem o script FireballMP!", this);
            }
        }
        else
        {
            Debug.LogError($"A torre {gameObject.name} n�o tem prefab de bala atribu�do!", this);
        }
    }

    // Override Shoot para spawnar FireballMP
    protected override void Shoot()
    {
        // Esta fun��o S� corre no Server (verifica��o j� feita no TowerMP.Update)
        if (bulletPrefab == null || firePoint == null || target == null) return;

        // 1. Instancia o prefab da FireballMP
        GameObject fireballGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // 2. Obt�m o NetworkObject para spawnar na rede
        NetworkObject fireballNO = fireballGO.GetComponent<NetworkObject>();
        if (fireballNO == null)
        {
            Debug.LogError($"Prefab '{bulletPrefab.name}' n�o tem NetworkObject!", this);
            Destroy(fireballGO); // Destr�i o objeto local se n�o puder ser spawnado
            return;
        }
        fireballNO.Spawn(); // Spawna a bola de fogo para todos os clientes

        // 3. Obt�m o script FireballMP
        FireballMP fireball = fireballGO.GetComponent<FireballMP>();
        if (fireball != null)
        {
            // 4. Define o dono e aplica stats de upgrade (se n�vel 3)
            fireball.ownerClientId = this.donoDaTorreClientId; // Diz � bola de fogo quem a disparou

            int currentDamage = baseBulletDamage;
            float currentSpeed = baseBulletSpeed;

            // Aplica melhorias de N�vel 3 (lendo o valor da NetworkVariable 'level')
            if (level.Value == 3)
            {
                currentDamage = (int)(baseBulletDamage * 1.5f); // +50% Dano
                currentSpeed = baseBulletSpeed * 1.5f;        // +50% Velocidade
            }

            fireball.damage = currentDamage;
            fireball.speed = currentSpeed;


            // 5. Define o alvo
            fireball.Seek(target);
        }
        else
        {
            Debug.LogError($"Prefab '{bulletPrefab.name}' n�o tem o script FireballMP!", this);
            fireballNO.Despawn(); // Despawna o objeto da rede se o script estiver em falta
        }
    }
}