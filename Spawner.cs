using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public bool devMode;    //update 메서드 안에 devmode시 강제 턴 설정
//웨이브형 게임
    public Wave[] waves;    //wave를 담을 배열 변수
    public Enemy[] enemy;     //적을 불러오기

    LivingEntity playerEntity;
    Transform playerT;


    Wave currentWave;       //배열인수
    int currentWaveNumber;  //배열순서

    int enemiesRemainingToSpawn;
    int enemiesRemainingAlive;    //남은 적 수
    float nextSpawnTime;

    MapGenerator map;   //spawn 할 좌표를 가져오기 위해

    float timeBetweenCampingChecks = 3;
    float campThresholdDistance = 1.5f;
    float nextCampCheckTime;
    Vector3 campPositionOld;    //캠핑하는 곳 좌표
    bool isCamping;
    bool isDisabled;    //플레이어가 죽었을 경우 true

    public event System.Action<int> OnNewWave;     //스테이지 변경


    void Start()
    {
        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.transform;

        nextCampCheckTime = timeBetweenCampingChecks + Time.time;
        campPositionOld = playerT.position;
        playerEntity.OnDeath += OnPlayerDeath;

        map = FindObjectOfType<MapGenerator> ();
        NextWave ();
    }

    void Update()
    {
        if (!isDisabled)
        {
            if (Time.time > nextCampCheckTime)  //camping check
            {
                nextCampCheckTime = Time.time + timeBetweenCampingChecks;
                isCamping = (Vector3.Distance(playerT.position, campPositionOld) < campThresholdDistance);  //자체 조건문 결과값을 bool변수에 할당
                campPositionOld = playerT.position;
            }

            if ((enemiesRemainingToSpawn >0 || currentWave.infinite) && Time.time > nextSpawnTime) {
                enemiesRemainingToSpawn--;        //스폰해야 할 적 숫자를 하나씩 줄이면서
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

                StartCoroutine("SpawnEnemy");   //SpawnEnemy());

            }
        }
        if (devMode)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StopCoroutine("SpawnEnemy");
                foreach (Enemy enemy in FindObjectsOfType<Enemy>())
                {
                    GameObject.Destroy(enemy.gameObject);
                }
                NextWave();
            }
            if (Input.GetKeyDown(KeyCode.Backspace))    //bestscore reset
            {
                PlayerPrefs.SetInt("BestScore", 0);
            }
        }
    }

    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1;
        float tileFlashSpeed = 4;

        Transform spawnTile = map.GetRandomOpenTile ();

        if (isCamping)
        {
            spawnTile = map.GetTileFromPosition(playerT.position);
        }

        Material tileMat = spawnTile.GetComponent<Renderer> ().material;
        Color initialColour = Color.white; //tileMat.color; 같은 타일에서 적이 연속해서 생성될 경우, 이니셜컬러를 플래시컬러로 오인하는 경우가 있어서
        Color flashColour = Color.red;
        float spawnTimer = 0;

        while (spawnTimer < spawnDelay)
        {
            tileMat.color = Color.Lerp(initialColour, flashColour, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1)); //pingpong 함수
            spawnTimer += Time.deltaTime;
            yield return null;  //한 프레임 만큼 대기?
        }
// Enemy를 배열로 정의하고, instantiate할 때 배열을 shuffle해서 랜덤하게 가져오게 하고, if (spawnedEnemy == enemy종류) {setcharacteristics}
        Enemy spawnedEnemy = Instantiate(enemy[Random.Range(0, enemy.Length)], spawnTile.position + Vector3.up * 2, Quaternion.identity) as Enemy;     //중앙(vector3.zero)에 생성
        spawnedEnemy.OnDeath += OnEnemyDeath;
        // spawnedEnemy.SetCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.skinColour);
        spawnedEnemy.SetCharacteristics(spawnedEnemy.enemySpeed, spawnedEnemy.enemyAtkPwr * currentWave.atkMultiplier, spawnedEnemy.startingHealth * currentWave.enemyHealth, new Color (Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1));
    }

    void OnPlayerDeath()
    {
        isDisabled = true;
    }

    void OnEnemyDeath() {
        //print ("Enemy Died");
        enemiesRemainingAlive --;
        
        if (enemiesRemainingAlive == 0) {     //다 죽고 나면 다음 웨이브 시작
            NextWave();
        }
    }

    void ResetPlayerPosition()
    {
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3; //플레이어가 약간 위에서 떨어지게
    }

    void NextWave()
    {
        if (currentWaveNumber > 0)
        {
            AudioManager.instance.PlaySound2D("Level Complete");
        }
        currentWaveNumber ++;   //현재 웨이브 순서를 1씩 증가
        
        //print ("Wave :" + currentWaveNumber);

        if (currentWaveNumber - 1 < waves.Length) {     //배열은 0부터 시작, 하나씩 더해가다가 배열의 총 길이를 넘어가지 않게
            currentWave = waves [currentWaveNumber - 1];    //배열 내에 있는 총 웨이브 개수를 하나씩 돌아가면서
            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;    //왜 바로 currentwave.enemycount를 바로 넣지 않는걸까?

            if (OnNewWave !=null)
            {
                OnNewWave(currentWaveNumber);
            }
            ResetPlayerPosition();
        }
    }

    [System.Serializable]   //컴포넌트에 wave 표시되게
    public class Wave {
        public bool infinite;
        public int enemyCount;
        public float timeBetweenSpawns;

        // public float moveSpeed;
        public float atkMultiplier;
        public float enemyHealth;
        // public Color skinColour;

    }

}
