using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    public Map[] maps;

    public Transform[] obstaclePrefab;
    
    public int mapIndex;
    public Transform tilePrefab;

    public Transform mapFloor;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;


    // public Vector2 mapSize;
    public Vector2 maxMapSize;

    [Range(0, 1)]
    public float outlinePercent;

    // [Range(0, 1)]
    public float tileSize;

    // public float obstaclePercent;   //장애물을 덮을 양

    List<Coord> allTileCoords;  //타일 좌표 담을 변수
    Queue<Coord> shuffledTileCoords;
    Queue<Coord> shuffledOpenTileCoords;    //장애물 없는 바닥을 담을 변수
    Transform[,] tileMap;   //바닥 타일들 좌표를 담을 변수

    Map currentMap;

    // public int seed = 10;
    // Coord mapCentre;    //플레이어를 스폰할 맵 중앙 위치 지정

    void Awake()    //왜 start를 awake로 바꾼지는 모름
    {
        //GenerateMap();
        FindObjectOfType<Spawner> ().OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        mapIndex = waveNumber -1;
        GenerateMap();
    }

    public void GenerateMap()
    {
        currentMap = maps [mapIndex];
        System.Random prng = new System.Random(currentMap.seed);
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];

        // GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize, 0.05f, currentMap.mapSize.y);
        //맵floor를 다시 설정해서 필요없어짐 저 밑으로 이동

        //Generating Coords
        allTileCoords = new List<Coord> ();
        for (int x = 0; x < currentMap.mapSize.x; x ++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y ++)
            {
                allTileCoords.Add(new Coord(x,y));
            }
        }
        shuffledTileCoords = new Queue<Coord> (Utility.ShuffleArray (allTileCoords.ToArray(), currentMap.seed));

        // mapCentre = new Coord((int) currentMap.mapSize.x / 2, (int) currentMap.mapSize.y / 2);   //Map 클래스에서 정의

        //Create map holder object
        string holderName = "Generated Map";

        if (transform.Find (holderName))    //findchild 쓰지 말라고 경고뜸.
        {
            DestroyImmediate(transform.Find (holderName).gameObject);   //에디터에서 호출할 것이기 때문에 destroyimm~~ 사용
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        //Spawning tiles
        for (int x = 0; x < currentMap.mapSize.x; x ++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y ++)
            {
                Vector3 tilePosition = CoordToPosition (x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right*90)) as Transform;
                newTile.localScale = Vector3.one * (1-outlinePercent) * tileSize;   //타일 사이즈 지정할 수 있도록
                newTile.parent = mapHolder;
                tileMap[x,y] = newTile;
            }
        }

        //Spawning obstacles
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];
        //맵 좌표에 장애물이 있는지 여부 점검하기 위한 변수
        //장애물이 있다면 다른 좌표를 찾아서 장애물을 생성하도록 할 것임

        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0;
        List<Coord> allOpenCoords = new List<Coord> (allTileCoords);    //모든 타일 좌표를 목록화한 후, 장애물 생성되면 해당 타일은 뺄 계획

        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount ++;

            if (randomCoord != currentMap.mapCentre && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
            {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float) prng.NextDouble());
                // int obstacleHeight = (int) Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, prng.Next());
            Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);

            //랜덤 장애물 생성 -- 나이스!!
            int obstacleIndex = Random.Range(0, obstaclePrefab.Length);
            Transform newObstacle = Instantiate(obstaclePrefab[obstacleIndex], obstaclePosition + Vector3.up * obstacleHeight / 2, Quaternion.identity) as Transform;
            newObstacle.parent = mapHolder;
            newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);

            Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
            Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
            float colourPercent = randomCoord.y / (float) currentMap.mapSize.y; //둘 중 하나의 값을 float로 변환해서 결과값이 float가 되도록
            obstacleMaterial.color = Color.Lerp(currentMap.foregroundColour, currentMap.backgroundColour, colourPercent);
            obstacleRenderer.sharedMaterial = obstacleMaterial;

            allOpenCoords.Remove(randomCoord);  //장애물이 생성된 타일은 리스트에서 제거
            }
            else
            {
            obstacleMap[randomCoord.x, randomCoord.y] = false;
            currentObstacleCount --;
            }
        }

        shuffledOpenTileCoords = new Queue<Coord> (Utility.ShuffleArray (allOpenCoords.ToArray(), currentMap.seed));

        //Creating navmesh mask
        // 2f, 4f와 같이 f를 붙여주는 것은, map size를 나눈 값을 정수로 리턴하면 홀수일 경우 0.5, 0.25의 오차가 생기므로
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x)/2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x)/2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        //forward : z축 방향으로
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y)/2f) * tileSize;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y)/2f) * tileSize;

        navmeshFloor.localScale = new Vector3 (maxMapSize.x, maxMapSize.y) * tileSize;  //navi map
        mapFloor.localScale = new Vector3(currentMap.mapSize.x * tileSize, currentMap.mapSize.y * tileSize);

    }

    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord> ();
        queue.Enqueue (currentMap.mapCentre);
        mapFlags [currentMap.mapCentre.x, currentMap.mapCentre.y] = true;

        int accessibleTileCount = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();

            for (int x = -1; x <= 1; x ++)
            {
                for (int y = -1; y <=1; y ++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;
                    if (x == 0 || y == 0)
                    {
                        if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >=0 && neighbourY < obstacleMap.GetLength(1))
                        {
                            if (!mapFlags[neighbourX,neighbourY] && !obstacleMap[neighbourX,neighbourY])
                            {
                            mapFlags[neighbourX,neighbourY] = true;
                            queue.Enqueue (new Coord(neighbourX,neighbourY));
                            accessibleTileCount ++;
                            }
                        }
                    }
                }
            }
        }
        int targetAccessibleTileCount = (int) (currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }


    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-currentMap.mapSize.x/2f +0.5f + x, 0, -currentMap.mapSize.y/2f +0.5f + y) * tileSize;
        //타일과 장애물의 위치(간격)에 타일과 장애물의 크기 비율만큼 같은 변수를 곱해줌
    }

    public Transform GetTileFromPosition(Vector3 position)  //같은자리에 있으면 그 근처에 적을 스폰시키기 위한 준비
    {
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x -1) / 2f);   //roundtoint는 반올림, int는 내림
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y -1) / 2f);
        x = Mathf.Clamp (x, 0, tileMap.GetLength (0)-1);
        y = Mathf.Clamp (y, 0, tileMap.GetLength (1)-1);
        return tileMap [x, y];
    }

    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue (randomCoord);
        return randomCoord;
    }

    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = shuffledOpenTileCoords.Dequeue();
        shuffledOpenTileCoords.Enqueue (randomCoord);
        return tileMap[randomCoord.x, randomCoord.y];
    }

    [System.Serializable]   //
    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }
        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }
        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }

//to resolve override errors
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [System.Serializable]
    public class Map
    {
        public Coord mapSize;
        [Range(0, 1)]
        public float obstaclePercent;
        public int seed;
        public int minObstacleHeight;
        public int maxObstacleHeight;
        public Color foregroundColour;
        public Color backgroundColour;

        public Coord mapCentre
        {
            get
            {
                return new Coord(mapSize.x/2, mapSize.y/2);
            }
        }

    }


}
