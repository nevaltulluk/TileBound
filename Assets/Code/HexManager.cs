using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Code
{
    public class HexManager : MonoBehaviour, IService, IPersistable
    {
        public List<GameObject> fallPrefabs; // Prefab for the hex tile
        public List<GameObject> springPrefabs; // Prefab for the hex tile
        public List<GameObject> summerPrefabs; // Prefab for the hex tile
        public List<GameObject> winterPrefabs; // Prefab for the hex tile
        public List<GameObject> basicHexPrefabs; // Prefab for the hex tile
        public SerializedDictionary<SpecialTiles, GameObject> specialTiles;
        public SerializedDictionary<Booster, GameObject> boosterTiles;
        public GameObject placeholderPrefab; // Prefab for the placeholder tile
        public Transform hexParent; // Parent object for hexes
        public Transform placeholderParent; // Parent object for placeholders

        public GameObject initialHex; // Reference to the initial hex on the scene

        // --- MODIFIED VARIABLE ---
        // Changed from HashSet<Vector2Int> to Dictionary<Vector2Int, GameObject>
        // This lets us find a hex GameObject from its coordinates.
        private Dictionary<Vector2Int, GameObject> _takenHexes = new Dictionary<Vector2Int, GameObject>();
        
        private List<GameObject> _activePlaceholders = new List<GameObject>(); // Tracks active placeholders
        private HashSet<string> _spawnedStarPairs = new HashSet<string>(); // Tracks which hex pairs have already spawned stars
        [NonSerialized]public GameObject LastPlacedHex;
        
        // Debug visualization for trigger matching
        private struct TriggerMatchDebug
        {
            public Vector3 myTriggerPos;
            public Vector3 neighborTriggerPos;
            public Vector3 starSpawnPos;
            public float timestamp;
        }
        private List<TriggerMatchDebug> _triggerMatches = new List<TriggerMatchDebug>();
        private EventBus _eventBus;
        private EventType _currentEventType = EventType.None;
        private List<GameObject> _addedHexes = new List<GameObject>();
        private int _currentLevel = 0;
        private DataManager _dataManager;
        
        [Header("Preview (Next Piece)")]
        public Transform previewParent;        // assign in Inspector (a clean 3D spot/camera)

        private GameObject _previewInstance;   // spawned under previewParent
        private EventType _previewEventType;
        private int _previewPrefabIndex = -1;

        private GameObject _pendingHex;
        private Vector2Int _pendingCoords;
        
        public GameObject PendingHex => _pendingHex;

        private void Awake()
        {
            MainContainer.instance.Register(this);
            
            _dataManager = MainContainer.instance.Resolve<DataManager>();
            _dataManager.AddToPersistable(this);
            
            _eventBus = MainContainer.instance.Resolve<EventBus>();
            _eventBus.Subscribe<Events.RestartButtonClicked>(OnGameRestart);
            _eventBus.Subscribe<Events.RequestNextLevel>(OnRequestNextLevel);
        }
        
        private void OnRequestNextLevel(Events.RequestNextLevel obj)
        {
            StartLevel(_currentLevel + 1);
        }
        
        private List<GameObject> GetPrefabListByEvent(EventType eventType)
        {
            switch (eventType)
            {
                case EventType.Spring: return springPrefabs;
                case EventType.Summer: return summerPrefabs;
                case EventType.Fall:   return fallPrefabs;
                case EventType.Winter: return winterPrefabs;
                default:               return basicHexPrefabs;
            }
        }

        private void DestroyPreview()
        {
            if (_previewInstance != null)
            {
                Destroy(_previewInstance);
                _previewInstance = null;
            }
            _previewPrefabIndex = -1;
        }

        private void GenerateNewPreview()
        {
            DestroyPreview();

            _previewEventType = _currentEventType; // preview always reflects current theme
            var list = GetPrefabListByEvent(_previewEventType);
            if (list == null || list.Count == 0)
            {
                Debug.LogWarning($"No prefabs available for preview: {_previewEventType}");
                return;
            }

            _previewPrefabIndex = UnityEngine.Random.Range(0, list.Count);
            var prefab = list[_previewPrefabIndex];

            if (prefab == null || previewParent == null)
            {
                Debug.LogWarning("Preview: missing prefab or previewParent.");
                return;
            }

            _previewInstance = Instantiate(prefab, previewParent.position, previewParent.rotation, previewParent);
            _previewInstance.name = "PREVIEW_" + prefab.name;

            // make it non-interactive
            foreach (var c in _previewInstance.GetComponentsInChildren<Collider>(true)) c.enabled = false;
            foreach (var c2 in _previewInstance.GetComponentsInChildren<Collider2D>(true)) c2.enabled = false;
            foreach (var rb in _previewInstance.GetComponentsInChildren<Rigidbody>(true)) rb.isKinematic = true;
        }

        
        private void UpdateInitialHexVisuals()
        {
            if (initialHex == null) return;

            // 1. Determine which list of prefabs to use based on the current event type
            List<GameObject> prefabList = basicHexPrefabs;
            switch (_currentEventType)
            {
                case EventType.Spring: prefabList = springPrefabs; break;
                case EventType.Summer: prefabList = summerPrefabs; break;
                case EventType.Fall:   prefabList = fallPrefabs;   break;
                case EventType.Winter: prefabList = winterPrefabs; break;
            }

            // 2. Ensure the list is valid and has at least one prefab
            if (prefabList == null || prefabList.Count == 0)
            {
                Debug.LogWarning($"Prefab list for {_currentEventType} is empty. Cannot update initial hex.");
                return;
            }

            // 3. Select the first prefab from the list as the visual source
            GameObject targetPrefab = prefabList[0];

            // 4. Get the Mesh and Renderer components from both the initial hex and the target prefab
            MeshFilter initialHexMeshFilter = initialHex.GetComponent<MeshFilter>();
            MeshRenderer initialHexRenderer = initialHex.GetComponent<MeshRenderer>();
            MeshFilter prefabMeshFilter = targetPrefab.GetComponent<MeshFilter>();
            MeshRenderer prefabRenderer = targetPrefab.GetComponent<MeshRenderer>();

            // 5. If all components are present, swap the mesh and materials
            if (initialHexMeshFilter && initialHexRenderer && prefabMeshFilter && prefabRenderer)
            {
                initialHexMeshFilter.sharedMesh = prefabMeshFilter.sharedMesh;
                initialHexRenderer.sharedMaterials = prefabRenderer.sharedMaterials;
            }
            else
            {
                Debug.LogError("Could not update initial hex visuals. Missing MeshFilter or MeshRenderer.");
            }
        }
        
        private void StartLevel(int level)
        {
            _currentLevel = level;
            SetEventTypeForLevel(_currentLevel);
            ClearAllHexes(); // This also destroys any pending hex
            UpdateInitialHexVisuals();
            GenerateNewPreview();
            Debug.Log($"Starting Level {_currentLevel} with theme {_currentEventType}");
            _eventBus.Fire(new Events.OnLevelStarted { Level = _currentLevel });
        }
        
        private void SetEventTypeForLevel(int level)
        {
            int themeIndex = level % 5; // Will result in 0, 1, 2, 3, or 4
            switch (themeIndex)
            {
                case 0: _currentEventType = EventType.None; break;
                case 1: _currentEventType = EventType.Spring; break;
                case 2: _currentEventType = EventType.Summer; break;
                case 3: _currentEventType = EventType.Fall; break;
                case 4: _currentEventType = EventType.Winter; break;
            }
        }

        private void Start()
        {
            if (_dataManager.HasSavedData() && _dataManager.GetData() != null)
            {
                Debug.Log("Save data found. Loading game state.");
                LoadData(_dataManager.GetData());
            }
            else
            {
                Debug.Log("No saved data found. Initializing new game.");
                Initialize();
            }
            
            _eventBus.Fire(new Events.OnGameStarted());
        }

        // --- MODIFIED METHOD ---
        private void Initialize()
        {
            if (initialHex != null)
            {
                Vector3 initialPosition = initialHex.transform.position;
                Vector2Int initialCoordinates = HexGrid.Instance.WorldToAxial(new Vector2(initialPosition.x, initialPosition.y));
                
                // Use new Dictionary
                if (!_takenHexes.ContainsKey(initialCoordinates))
                {
                    _takenHexes.Add(initialCoordinates, initialHex);
                }
                
                ShowPlaceholdersForAllHexes(HexGrid.Instance);
            }
        }

        private void OnGameRestart()
        {
            StartLevel(_currentLevel);
        }

        // --- MODIFIED METHOD ---
        // This is now primarily used by LoadData
        public void InstantiateHex(Vector2Int hexCoordinates, Vector3 worldPosition, EventType eventType = EventType.None, int prefabIndex = -1)
        {
            // Use new Dictionary
            if (_takenHexes.ContainsKey(hexCoordinates))
            {
                return;
            }

            var hexPrefabs = basicHexPrefabs;
            switch (eventType)
            {
                case EventType.Spring:
                    hexPrefabs = springPrefabs;
                    break;
                case EventType.Summer:
                    hexPrefabs = summerPrefabs;
                    break;
                case EventType.Fall:
                    hexPrefabs = fallPrefabs;
                    break;
                case EventType.Winter:
                    hexPrefabs = winterPrefabs;
                    break;
                default:
                    hexPrefabs = basicHexPrefabs;
                    break;
            }
            
            if (prefabIndex == -1)
            {
                prefabIndex = UnityEngine.Random.Range(0, hexPrefabs.Count);
            }
           
            else if (prefabIndex >= hexPrefabs.Count)
            {
                Debug.LogWarning($"Prefab index {prefabIndex} is out of bounds for {eventType} prefabs. Resetting to 0.");
                prefabIndex = 0;
            }
            
            var hexPrefab = hexPrefabs[prefabIndex];
            GameObject newHex = Instantiate(hexPrefab, worldPosition, Quaternion.identity, hexParent);
            
            var hexInfo = newHex.AddComponent<HexInfo>();
            hexInfo.HexCoordinates = hexCoordinates;
            hexInfo.EventType = eventType;
            hexInfo.PrefabIndex = prefabIndex;
            
            // Use new Dictionary
            _takenHexes.Add(hexCoordinates, newHex);
            
            LastPlacedHex = newHex;
            _addedHexes.Add(newHex);
            Debug.Log($"Hex instantiated at {hexCoordinates}.");
        }
        
        public void SaveData(ref GameData gameData)
        {
            gameData.placedHexes.Clear();
            gameData.currentLevel = _currentLevel;
            foreach (var hexObject in _addedHexes)
            {
                var hexInfo = hexObject.GetComponent<HexInfo>();
                if (hexInfo != null)
                {
                    gameData.placedHexes.Add(new HexData
                    {
                        q = hexInfo.HexCoordinates.x,
                        r = hexInfo.HexCoordinates.y,
                        eventType = hexInfo.EventType,
                        prefabIndex = hexInfo.PrefabIndex
                    });
                }
            }
        }

        // --- MODIFIED METHOD ---
        public void LoadData(GameData gameData)
        {
            // Load level and set theme BEFORE loading hexes
            _currentLevel = gameData.currentLevel;
            SetEventTypeForLevel(_currentLevel);

            // Clear the board
            _takenHexes.Clear(); // Use new Dictionary
            ClearPlaceholders();
            UpdateInitialHexVisuals();
            for (var i = _addedHexes.Count - 1; i >= 0; i--)
            {
                Destroy(_addedHexes[i]);
            }
            _addedHexes.Clear();

            // Destroy any pending hex from a previous session
            if (_pendingHex != null)
            {
                Destroy(_pendingHex);
                _pendingHex = null;
            }

            // Add the initial hex
            if (initialHex != null)
            {
                Vector3 initialPosition = initialHex.transform.position;
                Vector2Int initialCoordinates = HexGrid.Instance.WorldToAxial(new Vector2(initialPosition.x, initialPosition.y));
                
                // Use new Dictionary
                if (!_takenHexes.ContainsKey(initialCoordinates))
                {
                    _takenHexes.Add(initialCoordinates, initialHex);
                }
            }

            // Load saved hexes from GameData
            foreach (var hexData in gameData.placedHexes)
            {
                Vector2Int coordinates = new Vector2Int(hexData.q, hexData.r);
                Vector3 worldPosition = HexGrid.Instance.AxialToWorld(coordinates.x, coordinates.y);
                InstantiateHex(coordinates, worldPosition, hexData.eventType, hexData.prefabIndex);
            }
            GenerateNewPreview();
            ShowPlaceholdersForAllHexes(HexGrid.Instance);
            
            // Fire event to notify UI etc. that the level is loaded
            _eventBus.Fire(new Events.OnLevelStarted { Level = _currentLevel });
        }

        // --- MODIFIED METHOD ---
        private void ClearAllHexes()
        {
            _takenHexes.Clear(); // Use new Dictionary
            _spawnedStarPairs.Clear(); // Clear star spawn tracking (legacy, kept for compatibility)
            HexTrigger.ClearSpawnedPairs(); // Clear trigger-based star spawn tracking
            _triggerMatches.Clear(); // Clear debug visualization
            for (var i = _activePlaceholders.Count - 1; i >= 0; i--)
            {
                var activePlaceholder = _activePlaceholders[i];
                Destroy(activePlaceholder);
            }
            
            for (var i = _addedHexes.Count - 1; i >= 0; i--)
            {
                var addedHex = _addedHexes[i];
                Debug.Log("DELETE HEX IN COOR: " + HexGrid.Instance.WorldToAxial(addedHex.transform.position));
                Destroy(addedHex);
            }

            if (_pendingHex != null)
            {
                Destroy(_pendingHex);
                _pendingHex = null;
            }
            
            _activePlaceholders.Clear();
            _addedHexes.Clear();
            Initialize(); // This will re-add the initial hex to _takenHexes
            GenerateNewPreview();
        }

        // --- MODIFIED METHOD ---
        public void ShowPlaceholdersForAllHexes(HexGrid hexGrid)
        {
            ClearPlaceholders();
            
            // Use new Dictionary's Keys
            foreach (var hex in _takenHexes.Keys)
            {
                ShowPlaceholders(hex, hexGrid);
            }
        }

        // --- MODIFIED METHOD ---
        public void ShowPlaceholders(Vector2Int hexCoordinates, HexGrid hexGrid)
        {
            // We must pass the Keys collection to GetAvailableNeighbors.
            // To be safe, we convert it to a new HashSet, as the original code used a HashSet.
            var takenKeys = new HashSet<Vector2Int>(_takenHexes.Keys);
            List<Vector2Int> availableNeighbors = hexGrid.GetAvailableNeighbors(hexCoordinates, takenKeys);

            foreach (var neighbor in availableNeighbors)
            {
                Vector3 worldPosition = hexGrid.AxialToWorld(neighbor.x, neighbor.y);
                
                GameObject placeholder = Instantiate(placeholderPrefab, new Vector3(worldPosition.x, worldPosition.y, worldPosition.z), Quaternion.identity, placeholderParent);
                
                placeholder.GetComponent<HexPlaceholder>().Initialize(neighbor);

                _activePlaceholders.Add(placeholder);
            }
        }

        // --- MODIFIED METHOD ---
        public void PlaceHexFromPlaceholder(Vector2Int hexCoordinates, Vector3 worldPosition)
        {
            // Use new Dictionary
            if (_takenHexes.ContainsKey(hexCoordinates)) // Check just in case
            {
                return;
            }

            // If we're placing a new one, destroy the old pending one
            if (_pendingHex != null)
            {
                Destroy(_pendingHex);
            }

            _pendingCoords = hexCoordinates;

            // Get prefab from preview
            List<GameObject> hexPrefabs = GetPrefabListByEvent(_previewEventType);
            int prefabIndex = _previewPrefabIndex;

            if (hexPrefabs == null || hexPrefabs.Count == 0)
            {
                Debug.LogWarning($"No prefabs available for preview: {_previewEventType}. Using basic.");
                hexPrefabs = basicHexPrefabs;
                if (hexPrefabs.Count == 0)
                {
                     Debug.LogError("Basic hex prefab list is also empty. Cannot spawn hex.");
                     return;
                }
            }

            if (prefabIndex < 0 || prefabIndex >= hexPrefabs.Count)
            {
                Debug.LogWarning($"Invalid preview index {prefabIndex}. Resetting to 0.");
                prefabIndex = 0; 
            }
            
            var hexPrefab = hexPrefabs[prefabIndex];
            _pendingHex = Instantiate(hexPrefab, worldPosition, Quaternion.identity, hexParent);
        }
        
        // --- MODIFIED METHOD ---
        public void FinalizePendingHex()
        {
            if (_pendingHex == null)
            {
                Debug.LogWarning("FinalizePendingHex called, but there is no pending hex.");
                return;
            }
            
            // Use new Dictionary
            if (_takenHexes.ContainsKey(_pendingCoords))
            {
                Debug.LogWarning($"Finalize: Hex coordinate {_pendingCoords} already taken.");
                Destroy(_pendingHex);
                _pendingHex = null;
                return;
            }
            
            // 1. Add HexInfo to the pending hex
            var hexInfo = _pendingHex.AddComponent<HexInfo>();
            hexInfo.HexCoordinates = _pendingCoords;
            hexInfo.EventType = _previewEventType;
            hexInfo.PrefabIndex = _previewPrefabIndex;
        
            // 2. Register it
            _takenHexes.Add(_pendingCoords, _pendingHex); // Use new Dictionary
            _addedHexes.Add(_pendingHex);
            LastPlacedHex = _pendingHex; // This is now the last *finalized* hex
            
            Debug.Log($"Hex finalized at {_pendingCoords}.");

            // 3. Clear pending state
            _pendingHex = null;

            // 4. Star spawning is now handled by OnTriggerEnter in HexTrigger
            // No need to manually check for star spawns anymore

            // 5. Update placeholders
            ClearPlaceholders();
            ShowPlaceholdersForAllHexes(HexGrid.Instance);
        
            // 6. Generate the next preview
            GenerateNewPreview();
        }

        // --- OLD METHOD - NO LONGER USED ---
        // Star spawning is now handled by OnTriggerEnter in HexTrigger
        // Keeping this for reference but it's disabled
        private void CheckForStarSpawns_OLD(GameObject finalizedHex, Vector2Int hexCoords)
        {
            // We need a way to get neighbors. Assuming HexGrid has this method.
            // If not, you'll need to add it to HexGrid.cs
            if (HexGrid.Instance == null) return;
            List<Vector2Int> neighbors = HexGrid.Instance.GetNeighbors(hexCoords); 

            // 1. Get all triggers on the hex we just placed
            HexTrigger[] myTriggers = finalizedHex.GetComponentsInChildren<HexTrigger>();
            if (myTriggers.Length == 0) return; // No triggers on this new hex, nothing to do

            // 2. Loop through all its neighbor coordinates
            foreach (Vector2Int neighborCoord in neighbors)
            {
                // 3. Check if a finalized hex exists at that neighbor coordinate
                if (_takenHexes.TryGetValue(neighborCoord, out GameObject neighborHex))
                {
                    // 4. It does. Get all triggers on that neighbor.
                    HexTrigger[] neighborTriggers = neighborHex.GetComponentsInChildren<HexTrigger>();
                    if (neighborTriggers.Length == 0) continue; // Neighbor has no triggers

                    // Create a normalized key for this hex pair (order-independent)
                    string pairKey = GetHexPairKey(hexCoords, neighborCoord);
                    
                    // Check if we've already spawned a star for this hex pair
                    if (_spawnedStarPairs.Contains(pairKey))
                    {
                        continue; // Already spawned a star for this pair, skip
                    }

                    // 5. Check if there's any matching trigger pair between the two hexes
                    // Since triggers are on hexagon edges, only one matching pair can exist per hex pair
                    // We need to verify that the triggers are actually touching/adjacent to each other
                    
                    // Get hex center positions to calculate the direction between hexes
                    Vector3 myHexCenter = finalizedHex.transform.position;
                    Vector3 neighborHexCenter = neighborHex.transform.position;
                    Vector3 hexToNeighborDir = (neighborHexCenter - myHexCenter).normalized;
                    
                    bool foundMatch = false;
                    foreach (HexTrigger myTrigger in myTriggers)
                    {
                        if (!myTrigger.isSpawnStar) continue; // My trigger isn't set to spawn
                        if (foundMatch) break; // Already found a match, no need to check more

                        // Check if this trigger is on the edge facing the neighbor
                        Vector3 myTriggerPos = myTrigger.transform.position;
                        Vector3 myTriggerToHexCenter = (myHexCenter - myTriggerPos).normalized;
                        Vector3 myTriggerToNeighbor = (neighborHexCenter - myTriggerPos).normalized;
                        
                        // The trigger should be roughly in the direction of the neighbor hex
                        // Dot product should be positive (trigger is between center and neighbor)
                        float myTriggerAlignment = Vector3.Dot(myTriggerToNeighbor, hexToNeighborDir);
                        
                        // Only consider triggers that are facing towards the neighbor (threshold: > 0.5 means roughly 60 degrees or closer)
                        if (myTriggerAlignment < 0.5f) continue;

                        foreach (HexTrigger neighborTrigger in neighborTriggers)
                        {
                            if (!neighborTrigger.isSpawnStar) continue; // Neighbor's trigger isn't set to spawn
                            
                            // Check if neighbor trigger is on the edge facing back to my hex
                            Vector3 neighborTriggerPos = neighborTrigger.transform.position;
                            Vector3 neighborTriggerToHexCenter = (neighborHexCenter - neighborTriggerPos).normalized;
                            Vector3 neighborTriggerToMyHex = (myHexCenter - neighborTriggerPos).normalized;
                            Vector3 neighborToMyHexDir = -hexToNeighborDir; // Opposite direction
                            
                            // The neighbor trigger should be roughly in the direction of my hex
                            float neighborTriggerAlignment = Vector3.Dot(neighborTriggerToMyHex, neighborToMyHexDir);
                            
                            // Only consider triggers that are facing towards each other
                            if (neighborTriggerAlignment < 0.5f) continue;
                            
                            // Check if the two triggers are actually close to each other (within reasonable distance)
                            float distanceBetweenTriggers = Vector3.Distance(myTriggerPos, neighborTriggerPos);
                            float expectedEdgeDistance = Vector3.Distance(myHexCenter, neighborHexCenter) * 0.3f; // Triggers should be on edge, roughly 30% of hex distance from center
                            float maxDistance = expectedEdgeDistance * 2f; // Allow some tolerance
                            
                            if (distanceBetweenTriggers > maxDistance)
                            {
                                Debug.Log($"Triggers too far apart: {distanceBetweenTriggers} > {maxDistance}, skipping match");
                                continue;
                            }
                            
                            if (myTrigger.tileType == neighborTrigger.tileType || 
                                myTrigger.tileType == TileType.All || 
                                neighborTrigger.tileType == TileType.All)
                            {
                                // Mark this hex pair as having spawned a star
                                _spawnedStarPairs.Add(pairKey);
                                
                                // Spawn star at the average position of both matching triggers
                                // Both triggers are on the same edge but have different parents, so averaging gives the correct edge position
                                Vector3 triggerPosition = (myTriggerPos + neighborTriggerPos) / 2f;
                                
                                // Store for gizmo visualization
                                _triggerMatches.Add(new TriggerMatchDebug
                                {
                                    myTriggerPos = myTriggerPos,
                                    neighborTriggerPos = neighborTriggerPos,
                                    starSpawnPos = triggerPosition,
                                    timestamp = Time.time
                                });
                                
                                // Keep only recent matches (last 30 seconds)
                                _triggerMatches.RemoveAll(m => Time.time - m.timestamp > 30f);
                                
                                Debug.Log($"Spawn Star --- MyType: {myTrigger.tileType}, NeighborType: {neighborTrigger.tileType}, Distance: {distanceBetweenTriggers}, MyAlign: {myTriggerAlignment}, NeighborAlign: {neighborTriggerAlignment}");
                                _eventBus.Fire(new Events.SpawnStar(triggerPosition));
                                foundMatch = true;
                                break; // Found a match, no need to check more triggers
                            }
                        }
                    }
                }
            }
        }
        
        // Helper method to create a normalized key for a hex pair (order-independent)
        private string GetHexPairKey(Vector2Int coord1, Vector2Int coord2)
        {
            // Ensure consistent ordering so (A, B) and (B, A) produce the same key
            if (coord1.x < coord2.x || (coord1.x == coord2.x && coord1.y < coord2.y))
            {
                return $"{coord1.x},{coord1.y}_{coord2.x},{coord2.y}";
            }
            else
            {
                return $"{coord2.x},{coord2.y}_{coord1.x},{coord1.y}";
            }
        }

        
        public void RotatePendingHex(bool clockwise)
        {
            if (_pendingHex != null)
            {
                float rotation = clockwise ? 60 : -60;
                _pendingHex.transform.Rotate(0, rotation, 0);
            }
        }

        private void ClearPlaceholders()
        {
            foreach (var placeholder in _activePlaceholders)
            {
                Destroy(placeholder);
            }

            _activePlaceholders.Clear();
        }
        
        // Gizmo visualization for debugging trigger matches
        private void OnDrawGizmos()
        {
            // Draw all recent trigger matches
            foreach (var match in _triggerMatches)
            {
                // Draw my trigger position (green sphere)
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(match.myTriggerPos, 0.1f);
                Gizmos.DrawWireSphere(match.myTriggerPos, 0.15f);
                
                // Draw neighbor trigger position (blue sphere)
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(match.neighborTriggerPos, 0.1f);
                Gizmos.DrawWireSphere(match.neighborTriggerPos, 0.15f);
                
                // Draw line between the two triggers (yellow)
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(match.myTriggerPos, match.neighborTriggerPos);
                
                // Draw star spawn position (red sphere)
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(match.starSpawnPos, 0.15f);
                Gizmos.DrawWireSphere(match.starSpawnPos, 0.2f);
                
                // Draw line from midpoint to spawn position (cyan)
                Vector3 midpoint = (match.myTriggerPos + match.neighborTriggerPos) / 2f;
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(midpoint, match.starSpawnPos);
            }
        }
    }
}