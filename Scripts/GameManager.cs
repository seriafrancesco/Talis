using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;           // Il prefab dell’enemico da istanziare
    public Transform spawnPoint;             // Punto di spawn pubblico (puoi assegnarlo dall’Inspector)

    void Start()
    {
        //SpawnEnemy();
    }

    public void SpawnEnemy()
    {
        //GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    
}
