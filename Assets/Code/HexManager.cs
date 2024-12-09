using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code
{
    public class HexManager : MonoBehaviour
    {
        public static HexManager Instance { get; private set; } // Singleton instance

        public GameObject hexPrefab; // Prefab for the hex tile
        public GameObject placeholderPrefab; // Prefab for the placeholder tile
        public Transform hexParent; // Parent object for hexes
        public Transform placeholderParent; // Parent object for placeholders

        public GameObject initialHex; // Reference to the initial hex on the scene

        private HashSet<Vector2Int> takenHexes = new HashSet<Vector2Int>(); // Tracks all taken hexes
        private List<GameObject> activePlaceholders = new List<GameObject>(); // Tracks active placeholders
        [NonSerialized]public GameObject lastPlacedHex;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (initialHex != null)
            {
                Vector3 initialPosition = initialHex.transform.position;
                Vector2Int initialCoordinates = HexGrid.Instance.WorldToAxial(new Vector2(initialPosition.x, initialPosition.y));
                takenHexes.Add(initialCoordinates);

                ShowPlaceholdersForAllHexes(HexGrid.Instance);

                Debug.Log($"Initialized with first hex at {initialCoordinates}.");
            }
        }

        public void InstantiateHex(Vector2Int hexCoordinates, Vector3 worldPosition)
        {
            // Check if the hex is already taken
            if (takenHexes.Contains(hexCoordinates))
            {
                Debug.LogWarning($"Hex at {hexCoordinates} is already taken.");
                return;
            }

            // Instantiate the hex at the given position
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
            InstantiateHex(hexCoordinates, worldPosition);

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
