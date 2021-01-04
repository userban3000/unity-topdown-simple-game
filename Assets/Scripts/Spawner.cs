using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public Wave[] waves;

    LivingEntity playerEntity;
    Transform playerT;

    public Wave currentWave;
    int waveNumber;
    int enemiesRemainingToSpawn;
    int enemiesRemainingAlive;
    float nextSpawnTime;

    MapGenerator map;

    float timeBetweenCampingChecks = 2;
    float nextCampCheckTime;
    float campingTresholdDistance = 1.5f;
    Vector3 campPositionOld;
    bool isCamping;

    bool isDisabled;

    Vector3 lastDeath;
    Color lastColor;

    public TimeDilationEffect timeDilationEffect;
    public ParticleSystem endWaveDeathFX;

    public event System.Action<int> OnNewWave;
    public event System.Action OnHealedPlayer;
    public event System.Action OnPlayerWin;

    void Start() {
        playerEntity = FindObjectOfType<Player> ();
        playerT = playerEntity.transform;

        nextCampCheckTime = timeBetweenCampingChecks + Time.time;
        campPositionOld = playerT.position;
        playerEntity.OnDeath += onPlayerDeath;

        map = FindObjectOfType<MapGenerator>();

        NextWave();
    }

    void Update() {
        if ( !isDisabled ) {
            //CAMP CHECK
            if ( Time.time > nextCampCheckTime ) {
                nextCampCheckTime = Time.time + timeBetweenCampingChecks;

                isCamping = (Vector3.Distance(playerT.position, campPositionOld) < campingTresholdDistance);
                campPositionOld = playerT.position;
            }

            if ( ( enemiesRemainingToSpawn > 0 || currentWave.infinite ) && Time.time > nextSpawnTime ) {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

                int enemyTypeIndex = Random.Range(0,currentWave.enemies.Length);
                while ( currentWave.enemies[enemyTypeIndex].count == 0 )
                    enemyTypeIndex = Random.Range(0,currentWave.enemies.Length);

                currentWave.enemies[enemyTypeIndex].count--;
                StartCoroutine(SpawnEnemy(enemyTypeIndex));
            }
        }
    }

    IEnumerator SpawnEnemy(int index) {
        float spawnDelay = 1; //in seconds
        float tileFlashSpeed = 6; //flashes per second

        Transform spawnTile = map.GetRandomOpenTile();
        if ( isCamping ) {
            spawnTile = map.GetTileFromPosition(playerT.position);
        }
        Material tileMat = spawnTile.GetComponent<Renderer> ().material;
        Color initialColor = Color.white;
        Color flashColor = Color.red;
        float spawnTimer = 0;

        while ( spawnTimer < spawnDelay ) {
            tileMat.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1) );

            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = Instantiate(currentWave.enemies[index].enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += onEnemyDeath;
        spawnedEnemy.DiedAt += StoreLastDeath;
        //spawnedEnemy.SetCharacteristics(currentWave.moveSpeed, currentWave.damage, currentWave.damageVariance, currentWave.enemyHealth, currentWave.enemyColor);
    }

    void onPlayerDeath() {
        isDisabled = true;
    }

    void onEnemyDeath() {
        enemiesRemainingAlive--;

        if ( enemiesRemainingAlive == 0 ) {
            Instantiate( timeDilationEffect, Vector3.zero, Quaternion.identity);
            ParticleSystem waveDeathFX = Instantiate(endWaveDeathFX, lastDeath, Quaternion.identity) as ParticleSystem;
            Material mat = waveDeathFX.GetComponent<Renderer>().material;
            mat.color = lastColor;
            AudioManager.instance.PlaySound("Last Enemy Killed", lastDeath);
            StartCoroutine(NextWaveAfter(3f));
        }
    }

    void StoreLastDeath(Vector3 pos, Color col) {
        lastDeath = pos;
        lastColor = col;
    }

    IEnumerator NextWaveAfter(float t) {
        yield return new WaitForSeconds(t);
        NextWave();
    }

    void ResetPlayer() {
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up;
        playerEntity.SetHealth(playerEntity.startingHealth);
        
        OnHealedPlayer();
    }

    void NextWave() {

        if ( waveNumber + 1 > waves.Length ) {
            OnPlayerWin();
            return;
        }
        currentWave = waves[waveNumber];

        enemiesRemainingToSpawn = currentWave.totalEnemyCount;
        enemiesRemainingAlive = enemiesRemainingToSpawn;

        if ( OnNewWave != null ) {
            OnNewWave(waveNumber);
        }

        waveNumber++;

        ResetPlayer();
    }

    [System.Serializable]
    public class EnemyType {
        public Enemy enemy;
        public int count;
    }

    [System.Serializable]
    public class Wave {
        public bool infinite;

        public EnemyType[] enemies;
        public float timeBetweenSpawns;

        public int totalEnemyCount {
            get {
                int a = 0;
                for ( int i = 0; i < enemies.Length; i++ )
                    a += enemies[i].count;
                return a;
            }
        }

    }
}
