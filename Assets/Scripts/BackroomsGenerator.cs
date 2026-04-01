using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BackroomsGenerator : MonoBehaviour
{
    [SerializeField, Min(0f)] private float tileSize = 1f;
    
    [Space]
    [SerializeField] private Transform generationOrigin;
    [SerializeField,  Min(0)] private int generationDistance = 5;
    
    [Space]
    [SerializeField] private List<Vector2Int> serializedValidConnections = new();
    [SerializeField] private List<Pair<float, Pair<GameObject, EdgeSignature>>> serializedTiles = new();
    
    private Dictionary<int, HashSet<int>> validConnections = new();
    
    private List<(Tile, float)> weightedTiles = new();
    
    private HashSet<(long, long)> toGenerate = new();
    private Grid2D<Tile> tileGrid = new();

    private Grid2D<GameObject> objectGrid = new();

    private void Awake()
    {
        validConnections.TryAdd(-1, new HashSet<int>());
        validConnections[-1].Add(-1);
        
        foreach (Vector2Int pair in serializedValidConnections)
        {
            validConnections.TryAdd(pair[0], new HashSet<int>());
            validConnections[pair[0]].Add(pair[1]);
            
            validConnections.TryAdd(pair[1], new HashSet<int>());
            validConnections[pair[1]].Add(pair[0]);
        }

        foreach (Pair<float, Pair<GameObject, EdgeSignature>> tile in serializedTiles)
        {
            for (int i = 0; i < 4; i++)
            {
                weightedTiles.Add((new Tile(tile.Second, i), tile.First));
            }
        }

        tileGrid.onAdd += (pos, tile) =>
        {
            objectGrid[pos.Item1, pos.Item2] = tile.CreateObject(new Vector2(pos.Item1, pos.Item2) * tileSize, transform);
        };

        tileGrid.onRemove += (pos, tile) =>
        {
            Destroy(objectGrid[pos.Item1, pos.Item2]);
            objectGrid.Remove(pos.Item1, pos.Item2);
        };
    }

    private void Start()
    {
        StartCoroutine(GenerateCoroutine());
    }

    private IEnumerator GenerateCoroutine()
    {
        while (true)
        {
            Vector3 generationCentreVec3 = transform.InverseTransformPoint(generationOrigin.position) / tileSize;
            (long, long) generationCentre = (
                (long)(generationCentreVec3.x + (generationCentreVec3.x < 0 ? -0.5f : 0.5f)), 
                (long)(generationCentreVec3.z + (generationCentreVec3.z < 0 ? -0.5f : 0.5f)));
        
            for (long x = generationCentre.Item1 - generationDistance; x <= generationCentre.Item1 + generationDistance; x++)
            {
                for (long y = generationCentre.Item2 - generationDistance; y <= generationCentre.Item2 + generationDistance; y++)
                {
                    if (!tileGrid.ContainsPoint(x, y))
                    {
                        toGenerate.Add((x, y));
                    }
                }
            }
        
            tileGrid.ForEach((pos, tile) =>
            {
                if (pos.Item1 < generationCentre.Item1 - generationDistance || pos.Item1 > generationCentre.Item1 + generationDistance || 
                    pos.Item2 < generationCentre.Item2 - generationDistance || pos.Item2 > generationCentre.Item2 + generationDistance)
                {
                    if (!InLineOfSight(pos)) tileGrid.Remove(pos.Item1, pos.Item2);
                }
            });

            while (toGenerate.Count > 0)
            {
                while (toGenerate.Count > 0)
                {
                    List<(long, long)> lowestEntropy = GetLowestEntropy();
                    int index = Random.Range(0, lowestEntropy.Count);
                    (long, long) pos = lowestEntropy[index];
            
                    List<(Tile, float)> validTiles = GetValidTiles(pos);

                    if (validTiles.Count == 0)
                    {
                        toGenerate.Remove(pos);
                        continue;
                    }
            
                    Tile tile = RandomFromWeightedList(validTiles);
            
                    tileGrid[pos.Item1, pos.Item2] = tile;
                    toGenerate.Remove(pos);
                }
                
                tileGrid.ForEach((pos, tile) =>
                {
                    if (tile.tileObject.Second.Rotated(tile.rotation).north != -1 
                        && !tileGrid.ContainsPoint(pos.Item1, pos.Item2 + 1) 
                        && InLineOfSight((pos.Item1, pos.Item2 + 1)))
                        toGenerate.Add((pos.Item1, pos.Item2 + 1));
                    
                    if (tile.tileObject.Second.Rotated(tile.rotation).east != -1 
                        && !tileGrid.ContainsPoint(pos.Item1 + 1, pos.Item2) 
                        && InLineOfSight((pos.Item1 + 1, pos.Item2)))
                        toGenerate.Add((pos.Item1 + 1, pos.Item2));
                    
                    if (tile.tileObject.Second.Rotated(tile.rotation).south != -1 
                        && !tileGrid.ContainsPoint(pos.Item1, pos.Item2 - 1) 
                        && InLineOfSight((pos.Item1, pos.Item2 - 1)))
                        toGenerate.Add((pos.Item1, pos.Item2 - 1));
                    
                    if (tile.tileObject.Second.Rotated(tile.rotation).west != -1 
                        && !tileGrid.ContainsPoint(pos.Item1 - 1, pos.Item2) 
                        && InLineOfSight((pos.Item1 - 1, pos.Item2)))
                        toGenerate.Add((pos.Item1 - 1, pos.Item2));
                });
            }

            yield return new WaitForSeconds(0.05f);
        }
    }

    private T RandomFromWeightedList<T>(List<(T, float)> weightedList)
    {
        if (weightedList.Count == 1)
        {
            return weightedList[0].Item1;
        }
        
        float totalWeight = weightedList.Sum(entry => entry.Item2);
        float randomValue = Random.value * totalWeight;
        
        float cumulativeWeight = 0f;
        foreach ((T, float) weightedItem in weightedList)
        {
            cumulativeWeight += weightedItem.Item2;
            if (cumulativeWeight > randomValue) return weightedItem.Item1;
        }
        
        return weightedList.Last().Item1;
    }
    
    private List<(long, long)> GetLowestEntropy()
    {
        int minEntropy = toGenerate.Min(GetEntropy);
        return toGenerate.Where(pos => GetEntropy(pos) == minEntropy).ToList();
    }

    private int GetEntropy((long, long) pos)
    {
        List<(Tile, float)> validTiles = GetValidTiles(pos);
        return validTiles.Count;
    }
    
    private List<(Tile, float)> GetValidTiles((long, long) pos)
    {
        List<(Tile, float)> validTiles = weightedTiles;

        if (tileGrid.ContainsPoint(pos.Item1, pos.Item2 + 1))
        {
            Tile northTile = tileGrid[pos.Item1, pos.Item2 + 1];
            
            validTiles = validTiles.Where(tile => validConnections[tile.Item1.tileObject.Second.Rotated(tile.Item1.rotation).north]
                .Contains(northTile.tileObject.Second.Rotated(northTile.rotation).south)).ToList();
        }
        
        if (tileGrid.ContainsPoint(pos.Item1 + 1, pos.Item2))
        {
            Tile eastTile = tileGrid[pos.Item1 + 1, pos.Item2];
            
            validTiles = validTiles.Where(tile => validConnections[tile.Item1.tileObject.Second.Rotated(tile.Item1.rotation).east]
                .Contains(eastTile.tileObject.Second.Rotated(eastTile.rotation).west)).ToList();
        }
        
        if (tileGrid.ContainsPoint(pos.Item1, pos.Item2 - 1))
        {
            Tile southTile = tileGrid[pos.Item1, pos.Item2 - 1];
            
            validTiles = validTiles.Where(tile => validConnections[tile.Item1.tileObject.Second.Rotated(tile.Item1.rotation).south]
                .Contains(southTile.tileObject.Second.Rotated(southTile.rotation).north)).ToList();
        }
        
        if (tileGrid.ContainsPoint(pos.Item1 - 1, pos.Item2))
        {
            Tile westTile = tileGrid[pos.Item1 - 1, pos.Item2];
            
            validTiles = validTiles.Where(tile => validConnections[tile.Item1.tileObject.Second.Rotated(tile.Item1.rotation).west]
                .Contains(westTile.tileObject.Second.Rotated(westTile.rotation).east)).ToList();
        }
        
        return validTiles;
    }

    private bool InLineOfSight((long, long) pos)
    {
        //TODO: check line of sight
        return false;
    }
    
    [Serializable]
    public struct Pair<T, U>
    {
        [SerializeField] private T first;
        [SerializeField] private U second;
        
        public T First => first;
        public U Second => second;
    }
    
    [Serializable]
    private struct EdgeSignature
    {
        public int north;
        public int east;
        public int south;
        public int west;

        public EdgeSignature(int north, int east, int south, int west)
        {
            this.north = north;
            this.east = east;
            this.south = south;
            this.west = west;
        }
        
        /// <param name="rotation">the amount of times to rotate by 90 degrees clockwise</param>
        /// <returns>a rotated version of this edge signature</returns>
        public EdgeSignature Rotated(int rotation)
        {
            int i = rotation & 3;

            return i switch
            {
                0 => this,
                1 => new EdgeSignature(west, north, east, south),
                2 => new EdgeSignature(south, west, north, east),
                3 => new EdgeSignature(east, south, west, north),
                _ => default
            };
        }
    }
    
    [Serializable]
    private class Tile
    {
        public Pair<GameObject, EdgeSignature> tileObject;
        public int rotation;

        public Tile(Pair<GameObject, EdgeSignature> tileObject, int rotation)
        {
            this.tileObject = tileObject;
            this.rotation = rotation;
        }

        public GameObject CreateObject(Vector2 position, Transform parent)
        {
            GameObject gameObject = Instantiate(tileObject.First, parent);
            gameObject.transform.SetLocalPositionAndRotation(new Vector3(position.x, 0, position.y), Quaternion.Euler(0, rotation * 90, 0));
            return gameObject;
        }
    }
    
    private class Grid2D<T>
    {
        private Dictionary<(long, long), T> _grid = new();
        
        public T this[long x, long y]
        {
            get => _grid.ContainsKey((x, y)) ? _grid[(x, y)] : default;

            set
            {
                onRemove?.Invoke((x, y), _grid.TryGetValue((x, y), out T oldValue) ? oldValue : default);
                onAdd?.Invoke((x, y), value);
                _grid[(x, y)] = value;
            }
        }

        public T this[Vector2Int position]
        {
            get => this[position.x, position.y];
            set => this[position.x, position.y] = value;
        }

        public void Remove(long x, long y)
        {
            onRemove?.Invoke((x, y), _grid[(x, y)]);
            _grid.Remove((x, y));
        }
        
        public bool ContainsPoint(long x, long y) => _grid.ContainsKey((x, y));
        public bool ContainsPoint(Vector2Int pos) => ContainsPoint(pos.x, pos.y);
        
        public void Clear() { _grid.Clear(); }

        public bool IsEmpty => _grid.Count == 0;
        public long Count => _grid.Count;
        
        public void ForEach(Action<(long, long), T> action)
        {
            List<KeyValuePair<(long, long), T>> keyValuePairs = new();
            
            foreach (KeyValuePair<(long, long), T> keyValuePair in _grid)
            {
                keyValuePairs.Add(keyValuePair);
            }
            
            foreach (KeyValuePair<(long, long), T> pair in keyValuePairs) action(pair.Key, pair.Value);
        }

        public event Action<(long, long), T> onAdd;
        public event Action<(long, long), T> onRemove;
    }
}
