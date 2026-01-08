using UnityEngine;

public interface IEnemyDecorator
{
    void Decorate(GameObject enemyObj);
}

// --- DECORATOR NORMAL ---
public class NormalEnemyDecorator : IEnemyDecorator
{
    private AudioClip _deathSound;

    // Construtor agora recebe o som
    public NormalEnemyDecorator(AudioClip sound)
    {
        _deathSound = sound;
    }

    public void Decorate(GameObject enemyObj)
    {
        var health = enemyObj.GetComponent<EnemyHealth>();
        var movement = enemyObj.GetComponent<Enemy>();

        health.maxHealth = 15;
        health.reward = 5;
        movement.speed = 3.0f;
        enemyObj.transform.localScale = Vector3.one;

        // APLICA O SOM ESPECÍFICO DO NORMAL
        if (_deathSound != null) health.deathSound = _deathSound;
    }
}

// --- DECORATOR TANQUE ---
public class TankEnemyDecorator : IEnemyDecorator
{
    private Material _material;
    private AudioClip _deathSound;

    // Construtor recebe Material E Som
    public TankEnemyDecorator(Material mat, AudioClip sound)
    {
        _material = mat;
        _deathSound = sound;
    }

    public void Decorate(GameObject enemyObj)
    {
        var health = enemyObj.GetComponent<EnemyHealth>();
        var movement = enemyObj.GetComponent<Enemy>();

        health.maxHealth = 80;
        health.reward = 15;
        movement.speed = 3.0f;
        enemyObj.transform.localScale = new Vector3(2f, 2f, 2f);

        var rend = enemyObj.GetComponentInChildren<Renderer>();
        if (rend != null && _material != null) rend.material = _material;

        // APLICA O SOM ESPECÍFICO DO TANQUE
        if (_deathSound != null) health.deathSound = _deathSound;
    }
}