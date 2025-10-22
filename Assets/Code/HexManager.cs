using System;
using System.Collections.Generic;
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
        public GameObject placeholderPrefab; // Prefab for the placeholder tile
        public Transform hexParent; // Parent object for hexes
        public Transform placeholderParent; // Parent object for placeholders

        public GameObject initialHex; // Reference to the initial hex on the scene

        private HashSet<Vector2Int> _takenHexes = new HashSet<Vector2Int>(); // Tracks all taken hexes
        private List<GameObject> _activePlaceholders = new List<GameObject>(); // Tracks active placeholders
        [NonSerialized]public GameObject LastPlacedHex;
        private EventBus _eventBus;
        private EventType _currentEventType = EventType.None;
        private List<GameObject> _addedHexes = new List<GameObject>();
        private int _currentLevel = 0;
        private DataManager _dataManager;
        private void Awake()
        {
            MainContainer.instance.Register(this);
            
            _dataManager = MainContainer.instance.Resolve<DataManager>();
            _dataManager.AddToPersistable(this);
            
            _eventBus = MainContainer.instance.Resolve<EventBus>();
            _eventBus.Subscribe<OnSpringButtonClickEvent>(OnSpringButtonClick);
            _eventBus.Subscribe<OnSummerButtonClickEvent>(OnSummerButtonClick);
            _eventBus.Subscribe<OnFallButtonClickEvent>(OnFallButtonClick);
            _eventBus.Subscribe<OnWinterButtonClickEvent>(OnWinterButtonClick);
            _eventBus.Subscribe<Events.RestartButtonClicked>(OnGameRestart);
            _eventBus.Subscribe<Events.RequestNextLevel>(OnRequestNextLevel);
        }

        private void OnRequestNextLevel(Events.RequestNextLevel obj)
        {
            StartLevel(_currentLevel + 1);
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
            ClearAllHexes();
            UpdateInitialHexVisuals();
            
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

        private void Initialize()
        {
            if (initialHex != null)
            {
                Vector3 initialPosition = initialHex.transform.position;
                Vector2Int initialCoordinates = HexGrid.Instance.WorldToAxial(new Vector2(initialPosition.x, initialPosition.y));
                _takenHexes.Add(initialCoordinates);
                ShowPlaceholdersForAllHexes(HexGrid.Instance);
            }
        }

        private void OnGameRestart()
        {
            StartLevel(_currentLevel);
        }

        private void OnFallButtonClick(OnFallButtonClickEvent obj)
        {
            _currentEventType = EventType.Fall;
        }
        
        private void OnWinterButtonClick(OnWinterButtonClickEvent obj)
        {
            _currentEventType = EventType.Winter;
        }

        private void OnSummerButtonClick(OnSummerButtonClickEvent obj)
        {
            _currentEventType = EventType.Summer;
        }

        private void OnSpringButtonClick(OnSpringButtonClickEvent obj)
        {
            _currentEventType = EventType.Spring;
        }

        public void InstantiateHex(Vector2Int hexCoordinates, Vector3 worldPosition, EventType eventType = EventType.None, int prefabIndex = -1)
        {
            if (_takenHexes.Contains(hexCoordinates))
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
            
            _takenHexes.Add(hexCoordinates);
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

        public void LoadData(GameData gameData)
        {
            // Load level and set theme BEFORE loading hexes
            _currentLevel = gameData.currentLevel;
            SetEventTypeForLevel(_currentLevel);

            // Clear the board
            _takenHexes.Clear();
            ClearPlaceholders();
            UpdateInitialHexVisuals();
            for (var i = _addedHexes.Count - 1; i >= 0; i--)
            {
                Destroy(_addedHexes[i]);
            }
            _addedHexes.Clear();

            // Add the initial hex
            if (initialHex != null)
            {
                Vector3 initialPosition = initialHex.transform.position;
                Vector2Int initialCoordinates = HexGrid.Instance.WorldToAxial(new Vector2(initialPosition.x, initialPosition.y));
                _takenHexes.Add(initialCoordinates);
            }

            // Load saved hexes from GameData
            foreach (var hexData in gameData.placedHexes)
            {
                Vector2Int coordinates = new Vector2Int(hexData.q, hexData.r);
                Vector3 worldPosition = HexGrid.Instance.AxialToWorld(coordinates.x, coordinates.y);
                InstantiateHex(coordinates, worldPosition, hexData.eventType, hexData.prefabIndex);
            }

            ShowPlaceholdersForAllHexes(HexGrid.Instance);
            
            // Fire event to notify UI etc. that the level is loaded
            _eventBus.Fire(new Events.OnLevelStarted { Level = _currentLevel });
        }

        private void ClearAllHexes()
        {
            _takenHexes.Clear();
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

            _activePlaceholders.Clear();
            _addedHexes.Clear();
            Initialize();
        }

        public void ShowPlaceholdersForAllHexes(HexGrid hexGrid)
        {
            ClearPlaceholders();
            foreach (var hex in _takenHexes)
            {
                ShowPlaceholders(hex, hexGrid);
            }
        }

        public void ShowPlaceholders(Vector2Int hexCoordinates, HexGrid hexGrid)
        {
            List<Vector2Int> availableNeighbors = hexGrid.GetAvailableNeighbors(hexCoordinates, _takenHexes);

            foreach (var neighbor in availableNeighbors)
            {
                Vector3 worldPosition = hexGrid.AxialToWorld(neighbor.x, neighbor.y);
                
                GameObject placeholder = Instantiate(placeholderPrefab, new Vector3(worldPosition.x, worldPosition.y, worldPosition.z), Quaternion.identity, placeholderParent);
                
                placeholder.GetComponent<HexPlaceholder>().Initialize(neighbor);

                _activePlaceholders.Add(placeholder);
            }
        }

        public void PlaceHexFromPlaceholder(Vector2Int hexCoordinates, Vector3 worldPosition)
        {
            InstantiateHex(hexCoordinates, worldPosition,_currentEventType);
            
            ClearPlaceholders();
            
            ShowPlaceholdersForAllHexes(HexGrid.Instance);
        }

        private void ClearPlaceholders()
        {
            foreach (var placeholder in _activePlaceholders)
            {
                Destroy(placeholder);
            }

            _activePlaceholders.Clear();
        }
    }
}
