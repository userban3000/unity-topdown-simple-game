using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

    public Map[] maps;
    public int mapIndex = 0;

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Vector2Int maxMapSize;
    public Transform NavmeshFloor;
    public Transform NavmeshMaskPrefab;


    [Range(0,1)]
    public float outlinePercent;
    public float tileSize;

    List<Coord> allTileCoords;
    Queue<Coord> shuffledOpenTileCoords;
    Queue<Coord> shuffledTileCoords;

    Map currentMap;

    Transform[,] tileMap;

    void Awake() {
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave ( int waveNumber ) {
        mapIndex = waveNumber;
        GenerateMap();
    }

    public void GenerateMap() {

        currentMap = maps[mapIndex];
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
        GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize, .05f, currentMap.mapSize.y * tileSize);

        //HEIGHT GENERATION
        System.Random prng = new System.Random(currentMap.seed);

        //SHUFFLE TILES TO DECIDE WHERE TO PLACE OBSTACLES
        allTileCoords = new List<Coord> ();
        for ( int i = 0; i < currentMap.mapSize.x; i++ ) {
            for ( int j = 0; j < currentMap.mapSize.y; j++) {
                allTileCoords.Add(new Coord(i,j) );
            }
        }
        shuffledTileCoords = new Queue<Coord>( Utility.ShuffleArray( allTileCoords.ToArray(), currentMap.seed ) );

        //DESTROY OLD MAP
        string holderName = "Generated Map";
        if ( transform.Find(holderName) ) {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        //FOLDER-IZE
        Transform mapHolder = new GameObject (holderName).transform;
        mapHolder.parent = transform;

        //GENERATE GROUND PLANE TILES
        for ( int i = 0; i < currentMap.mapSize.x; i++ ) {
            for ( int j = 0; j < currentMap.mapSize.y; j++) {
                Vector3 tilePosition = CoordToPosition(i,j);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right*90) ) as Transform;
                newTile.localScale = Vector3.one * (1-outlinePercent) * tileSize;
                newTile.parent = mapHolder;
                newTile.name = "Tile " + i + " " + j;
                tileMap[i, j] = newTile;
            }
        }

        //GENERATE OBSTACLES
        bool[,] obstacleMap = new bool [currentMap.mapSize.x, currentMap.mapSize.y];

        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0;
        List<Coord> allOpenCoords = new List<Coord> (allTileCoords);

        for ( int i = 0; i < obstacleCount; i++ ) {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;
            
            if ( randomCoord != currentMap.mapCenter && FloodFill(obstacleMap, currentObstacleCount ) ) {
                
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble() );
                Vector3 obstaclePos = CoordToPosition(randomCoord.x, randomCoord.y);
            
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePos + Vector3.up * (obstacleHeight/2), Quaternion.identity ) as Transform;
                newObstacle.parent = mapHolder;
                newObstacle.localScale = new Vector3( (1-outlinePercent) * tileSize, obstacleHeight, (1-outlinePercent) * tileSize );
                newObstacle.name = "Obstacle" + i;

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);

                float colorPercent = randomCoord.y / (float)currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);

                obstacleRenderer.sharedMaterial = obstacleMaterial;

                allOpenCoords.Remove(randomCoord);
            }
            else {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }

        shuffledOpenTileCoords = new Queue<Coord>( Utility.ShuffleArray( allOpenCoords.ToArray(), currentMap.seed ) );

        //AI NAVMESH
        
        //WALLS

		Transform maskLeft = Instantiate (NavmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
		maskLeft.parent = mapHolder;
		maskLeft.localScale = new Vector3 ((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;
		
		Transform maskRight = Instantiate (NavmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
		maskRight.parent = mapHolder;
		maskRight.localScale = new Vector3 ((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;
		
		Transform maskTop = Instantiate (NavmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
		maskTop.parent = mapHolder;
		maskTop.localScale = new Vector3 (maxMapSize.x, 1, (maxMapSize.y-currentMap.mapSize.y)/2f) * tileSize;
		
		Transform maskBottom = Instantiate (NavmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
		maskBottom.parent = mapHolder;
		maskBottom.localScale = new Vector3 (maxMapSize.x, 1, (maxMapSize.y-currentMap.mapSize.y)/2f) * tileSize;
		
		NavmeshFloor.localScale = new Vector3 (maxMapSize.x, maxMapSize.y) * tileSize;
    }

    public bool FloodFill(bool[,] obstacleMap, int inaccessibleTiles ) {
        Coord[] nextDir = {
            new Coord(-1,0),
            new Coord(0,1),
            new Coord(1,0),
            new Coord(0,-1)
        }; //LEFT TOP RIGHT BOTTOM
        Queue<Coord> q = new Queue<Coord> ();

        bool[,] map = (bool[,])obstacleMap.Clone();

    
        int targetTiles = currentMap.mapSize.x * currentMap.mapSize.y - inaccessibleTiles;
        int currentTiles = 1;
        q.Enqueue(currentMap.mapCenter);
        map[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

        while ( q.Count > 0 ) {
            Coord tile = q.Dequeue();
            foreach ( Coord nextDelta in nextDir ) {
                Coord next = tile + nextDelta;
                //Debug.Log("next is " + next.x + " " + next.y);
                if ( next.x >= 0 && next.x < currentMap.mapSize.x && next.y >= 0 && next.y < currentMap.mapSize.y ) {
                    if ( !map[next.x, next.y] ) {
                        q.Enqueue(next);
                        map[next.x, next.y] = true;
                        currentTiles++;
                    }
                }
            }
        }
        return currentTiles == targetTiles;
    }

    Vector3 CoordToPosition( int x, int y ) {
        return new Vector3 (-currentMap.mapSize.x/2f + 0.5f + x, 0, -currentMap.mapSize.y/2f + 0.5f + y) * tileSize; //+0.5f is used for alignment
    }

    public Transform GetTileFromPosition(Vector3 position) {
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x-1) /2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y-1) /2f);
        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1 );
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1 );
        return tileMap[x,y];
    }

    public Coord GetRandomCoord() {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

    public Transform GetRandomOpenTile() {
        Coord randomCoord = shuffledOpenTileCoords.Dequeue();
        shuffledOpenTileCoords.Enqueue(randomCoord);
        return tileMap[randomCoord.x, randomCoord.y];
    }

    [System.Serializable]
    public struct Coord {
        public int x;
        public int y;

        public Coord(int _x, int _y) {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coord c1, Coord c2) {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2) {
            return !(c1==c2);
        } 

        //code to get rid of warnings on not overriding Object.Equals and Object.GetHashCode
        //thanks to 45M0D3E on yt
        public override bool Equals(object obj)
        {
            if ( obj is Coord coord) {
                return this == coord;
            }
            return false;
        }

        public override int GetHashCode() => new {x, y}.GetHashCode();

        //overloads + to allow Coord addition
        public static Coord operator +(Coord a, Coord b) => new Coord ( a.x + b.x, a.y + b.y );
    }

    [System.Serializable]
    public class Map {
        
        public Coord mapSize;
        [Range(0,1)]
        public float obstaclePercent;
        public int seed;

        public float minObstacleHeight;
        public float maxObstacleHeight;

        public Color foregroundColor;
        public Color backgroundColor;

        public Coord mapCenter {
            get {
                return new Coord (mapSize.x/2, mapSize.y/2);
            }
        }

    }

}
