using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Code
{
    public class HexManager : MonoBehaviour, IService
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
        private void Awake()
        {
            MainContainer.instance.Register(this);
        }

        private void Start()
        {
            _eventBus = MainContainer.instance.Resolve<EventBus>();
            _eventBus.Subscribe<OnSpringButtonClickEvent>(OnSpringButtonClick);
            _eventBus.Subscribe<OnSummerButtonClickEvent>(OnSummerButtonClick);
            _eventBus.Subscribe<OnFallButtonClickEvent>(OnFallButtonClick);
            _eventBus.Subscribe<OnWinterButtonClickEvent>(OnWinterButtonClick);
            _eventBus.Subscribe<Events.RestartButtonClicked>(OnGameRestart);
            Initialize();
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
            ClearAllHexes();
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

        public void InstantiateHex(Vector2Int hexCoordinates, Vector3 worldPosition, EventType eventType = EventType.None)
        {
            // Check if the hex is already taken
            if (_takenHexes.Contains(hexCoordinates))
            {
                //Debug.LogWarning($"Hex at {hexCoordinates} is already taken.");
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
            var hexPrefab = hexPrefabs[UnityEngine.Random.Range(0, hexPrefabs.Count)];
            GameObject newHex = Instantiate(hexPrefab, worldPosition, Quaternion.identity, hexParent);
            _takenHexes.Add(hexCoordinates);
            LastPlacedHex = newHex;
            _addedHexes.Add(newHex);
            Debug.Log($"Hex instantiated at {hexCoordinates}.");
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
            // Clear existing placeholders
            ClearPlaceholders();

            // Show placeholders for all existing hexes
            foreach (var hex in _takenHexes)
            {
                ShowPlaceholders(hex, hexGrid);
            }
        }

        public void ShowPlaceholders(Vector2Int hexCoordinates, HexGrid hexGrid)
        {
            // Get available neighbors from HexGrid
            List<Vector2Int> availableNeighbors = hexGrid.GetAvailableNeighbors(hexCoordinates, _takenHexes);

            foreach (var neighbor in availableNeighbors)
            {
                // Calculate the world position for each neighbor
                Vector3 worldPosition = hexGrid.AxialToWorld(neighbor.x, neighbor.y);

                // Instantiate placeholder at the world position
                GameObject placeholder = Instantiate(placeholderPrefab, new Vector3(worldPosition.x, worldPosition.y, worldPosition.z), Quaternion.identity, placeholderParent);

                // Assign the neighbor coordinates to the placeholder for later use
                placeholder.GetComponent<HexPlaceholder>().Initialize(neighbor);

                _activePlaceholders.Add(placeholder);
            }
        }

        public void PlaceHexFromPlaceholder(Vector2Int hexCoordinates, Vector3 worldPosition)
        {
            // Instantiate the hex at the placeholder's position
            InstantiateHex(hexCoordinates, worldPosition,_currentEventType);

            // Clear placeholders after placement
            ClearPlaceholders();

            // Show placeholders for all existing hexes
            ShowPlaceholdersForAllHexes(HexGrid.Instance);
        }

        private void ClearPlaceholders()
        {
            // Destroy all active placeholders
            foreach (var placeholder in _activePlaceholders)
            {
                Destroy(placeholder);
            }

            _activePlaceholders.Clear();
        }
    }
}
