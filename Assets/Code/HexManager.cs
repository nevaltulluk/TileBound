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

        private HashSet<Vector2Int> takenHexes = new HashSet<Vector2Int>(); // Tracks all taken hexes
        private List<GameObject> activePlaceholders = new List<GameObject>(); // Tracks active placeholders
        [NonSerialized]public GameObject lastPlacedHex;
        private EventBus eventBus;
        private EventType currentEventType = EventType.None;
        private void Awake()
        {
            MainContainer.Instance.Register(this);
        }

        private void Start()
        {
            eventBus = MainContainer.Instance.Resolve<EventBus>();
            eventBus.Subscribe<OnSpringButtonClickEvent>(OnSpringButtonClick);
            eventBus.Subscribe<OnSummerButtonClickEvent>(OnSummerButtonClick);
            eventBus.Subscribe<OnFallButtonClickEvent>(OnFallButtonClick);
            eventBus.Subscribe<OnWinterButtonClickEvent>(OnWinterButtonClick);
            if (initialHex != null)
            {
                Vector3 initialPosition = initialHex.transform.position;
                Vector2Int initialCoordinates = HexGrid.Instance.WorldToAxial(new Vector2(initialPosition.x, initialPosition.y));
                takenHexes.Add(initialCoordinates);

                ShowPlaceholdersForAllHexes(HexGrid.Instance);

                Debug.Log($"Initialized with first hex at {initialCoordinates}.");
            }
        }

        private void OnFallButtonClick(OnFallButtonClickEvent obj)
        {
            currentEventType = EventType.Fall;
        }
        
        private void OnWinterButtonClick(OnWinterButtonClickEvent obj)
        {
            currentEventType = EventType.Winter;
        }

        private void OnSummerButtonClick(OnSummerButtonClickEvent obj)
        {
            currentEventType = EventType.Summer;
        }

        private void OnSpringButtonClick(OnSpringButtonClickEvent obj)
        {
            currentEventType = EventType.Spring;
        }

        public void InstantiateHex(Vector2Int hexCoordinates, Vector3 worldPosition, EventType eventType = EventType.None)
        {
            // Check if the hex is already taken
            if (takenHexes.Contains(hexCoordinates))
            {
                Debug.LogWarning($"Hex at {hexCoordinates} is already taken.");
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
            takenHexes.Add(hexCoordinates);
            lastPlacedHex = newHex;

            Debug.Log($"Hex instantiated at {hexCoordinates}.");
        }

        public void ShowPlaceholdersForAllHexes(HexGrid hexGrid)
        {
            // Clear existing placeholders
            ClearPlaceholders();

            // Show placeholders for all existing hexes
            foreach (var hex in takenHexes)
            {
                ShowPlaceholders(hex, hexGrid);
            }
        }

        public void ShowPlaceholders(Vector2Int hexCoordinates, HexGrid hexGrid)
        {
            // Get available neighbors from HexGrid
            List<Vector2Int> availableNeighbors = hexGrid.GetAvailableNeighbors(hexCoordinates, takenHexes);

            foreach (var neighbor in availableNeighbors)
            {
                // Calculate the world position for each neighbor
                Vector3 worldPosition = hexGrid.AxialToWorld(neighbor.x, neighbor.y);

                // Instantiate placeholder at the world position
                GameObject placeholder = Instantiate(placeholderPrefab, new Vector3(worldPosition.x, worldPosition.y, worldPosition.z), Quaternion.identity, placeholderParent);

                // Assign the neighbor coordinates to the placeholder for later use
                placeholder.GetComponent<HexPlaceholder>().Initialize(neighbor);

                activePlaceholders.Add(placeholder);
            }
        }

        public void PlaceHexFromPlaceholder(Vector2Int hexCoordinates, Vector3 worldPosition)
        {
            // Instantiate the hex at the placeholder's position
            InstantiateHex(hexCoordinates, worldPosition,currentEventType);

            // Clear placeholders after placement
            ClearPlaceholders();

            // Show placeholders for all existing hexes
            ShowPlaceholdersForAllHexes(HexGrid.Instance);
        }

        private void ClearPlaceholders()
        {
            // Destroy all active placeholders
            foreach (var placeholder in activePlaceholders)
            {
                Destroy(placeholder);
            }

            activePlaceholders.Clear();
        }
    }
}
