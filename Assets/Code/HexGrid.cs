using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    public static HexGrid Instance { get; private set; }
    
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
    
    public float hexSize = 1.0f; // Size of the hexagon

    public Vector3 AxialToWorld(int q, int r)
    {
        float x = hexSize * Mathf.Sqrt(3) * (q + r / 2f);
        float z = hexSize * 1.5f * r;
        return new Vector3(x, 0, z);
    }

    // Convert world position (x, z) to axial coordinates (q, r)
    public Vector2Int WorldToAxial(Vector2 position)
    {
        float q = (Mathf.Sqrt(3) / 3 * position.x - 1f / 3 * position.y) / hexSize;
        float r = (2f / 3 * position.y) / hexSize;

        return new Vector2Int(Mathf.RoundToInt(q), Mathf.RoundToInt(r));
    }

    // Find all neighboring grids of a given grid (q, r), excluding already taken ones
    public List<Vector2Int> GetAvailableNeighbors(Vector2Int currentHex, HashSet<Vector2Int> takenHexes)
    {
        // Define the six possible neighbor directions in axial coordinates
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),  // East
            new Vector2Int(-1, 0), // West
            new Vector2Int(0, 1),  // Northeast
            new Vector2Int(0, -1), // Southwest
            new Vector2Int(1, -1), // Southeast
            new Vector2Int(-1, 1)  // Northwest
        };

        List<Vector2Int> availableNeighbors = new List<Vector2Int>();

        // Check each neighbor
        foreach (var direction in directions)
        {
            Vector2Int neighbor = currentHex + direction;

            // Only add the neighbor if it's not in the taken set
            if (!takenHexes.Contains(neighbor))
            {
                availableNeighbors.Add(neighbor);
            }
        }

        return availableNeighbors;
    }
}