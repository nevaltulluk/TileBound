using System;
using System.Collections.Generic;
using Code;
using UnityEngine;

public class HexTrigger : MonoBehaviour
{
    public TileType tileType;
    public bool isSpawnStar;
    
    private static HashSet<string> _spawnedStarPairs = new HashSet<string>();
    private static HashSet<(HexTrigger, HexTrigger)> _touchingPairs = new HashSet<(HexTrigger, HexTrigger)>();
    private static HashSet<(HexTrigger, HexTrigger)> _recentMatches = new HashSet<(HexTrigger, HexTrigger)>();
    private static bool _hasSubscribed = false;
    private static EventBus _eventBus;
    
    private void Awake()
    {
        // Subscribe to OK button click only once (using static flag)
        if (!_hasSubscribed)
        {
            _hasSubscribed = true;
            _eventBus = MainContainer.instance.Resolve<EventBus>();
            _eventBus.Subscribe<Events.OkButtonClicked>(OnOkButtonClicked);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to another HexTrigger
        HexTrigger otherTrigger = other.GetComponent<HexTrigger>();
        if (otherTrigger == null) return;
        
        // Check if both triggers are set to spawn stars
        if (!isSpawnStar || !otherTrigger.isSpawnStar) return;
        
        // Check if tile types match
        if (tileType != otherTrigger.tileType && 
            tileType != TileType.All && 
            otherTrigger.tileType != TileType.All)
        {
            return;
        }
        
        // Just track this touching pair - don't spawn stars yet
        // Store for later processing when OK button is pressed
        if (GetInstanceID() < otherTrigger.GetInstanceID())
        {
            _touchingPairs.Add((this, otherTrigger));
        }
        else
        {
            _touchingPairs.Add((otherTrigger, this));
        }
        
        Debug.Log($"OnTriggerEnter: Touching pair tracked --- MyType: {tileType}, OtherType: {otherTrigger.tileType}");
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Remove from touching pairs if triggers separate
        HexTrigger otherTrigger = other.GetComponent<HexTrigger>();
        if (otherTrigger == null) return;
        
        if (GetInstanceID() < otherTrigger.GetInstanceID())
        {
            _touchingPairs.Remove((this, otherTrigger));
        }
        else
        {
            _touchingPairs.Remove((otherTrigger, this));
        }
    }
    
    private static void OnOkButtonClicked(Events.OkButtonClicked obj)
    {
        // Process all touching pairs when OK button is pressed
        ProcessTouchingPairs();
    }
    
    private static void ProcessTouchingPairs()
    {
        if (_eventBus == null)
        {
            _eventBus = MainContainer.instance.Resolve<EventBus>();
        }
        
        // Process all touching pairs
        foreach (var (trigger1, trigger2) in _touchingPairs)
        {
            // Check if trigger pair is still valid
            if (trigger1 == null || trigger2 == null) continue;
            
            // Create a normalized key for this trigger pair
            string pairKey = GetTriggerPairKeyStatic(trigger1, trigger2);
            
            // Check if we've already spawned a star for this trigger pair
            if (_spawnedStarPairs.Contains(pairKey))
            {
                continue; // Already spawned a star for this pair
            }
            
            // Mark this pair as having spawned a star
            _spawnedStarPairs.Add(pairKey);
            
            // Store for gizmo visualization
            _recentMatches.Add((trigger1, trigger2));
            
            // Get world positions of both triggers
            Vector3 pos1 = trigger1.transform.position;
            Vector3 pos2 = trigger2.transform.position;
            
            // Spawn star at the average position of both triggers
            Vector3 spawnPosition = (pos1 + pos2) / 2f;
            
            // Fire event to spawn star
            _eventBus.Fire(new Events.SpawnStar(spawnPosition));
            
            Debug.Log($"OK Button: Star Match --- Type1: {trigger1.tileType}, Type2: {trigger2.tileType}, SpawnPos: {spawnPosition}");
        }
        
        // Clear touching pairs after processing (they'll be re-added if still touching)
        _touchingPairs.Clear();
    }
    
    // Helper method to create a normalized key for a trigger pair (order-independent)
    private string GetTriggerPairKey(HexTrigger trigger1, HexTrigger trigger2)
    {
        return GetTriggerPairKeyStatic(trigger1, trigger2);
    }
    
    private static string GetTriggerPairKeyStatic(HexTrigger trigger1, HexTrigger trigger2)
    {
        // Use instance IDs to create a unique, order-independent key
        int id1 = trigger1.GetInstanceID();
        int id2 = trigger2.GetInstanceID();
        
        // Ensure consistent ordering
        if (id1 < id2)
        {
            return $"{id1}_{id2}";
        }
        else
        {
            return $"{id2}_{id1}";
        }
    }
    
    // Method to clear spawned pairs (call this when restarting/clearing level)
    public static void ClearSpawnedPairs()
    {
        _spawnedStarPairs.Clear();
        _touchingPairs.Clear();
        _recentMatches.Clear();
    }
    
    // Gizmo visualization for debugging trigger matches
    private void OnDrawGizmos()
    {
        // Only draw gizmos for the first trigger in each pair to avoid duplicates
        foreach (var (trigger1, trigger2) in _recentMatches)
        {
            if (this == trigger1) // Only draw when we're the first trigger
            {
                Vector3 pos1 = trigger1.transform.position;
                Vector3 pos2 = trigger2.transform.position;
                Vector3 spawnPos = (pos1 + pos2) / 2f;
                
                // Draw trigger1 position (green sphere)
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(pos1, 0.1f);
                Gizmos.DrawWireSphere(pos1, 0.15f);
                
                // Draw trigger2 position (blue sphere)
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(pos2, 0.1f);
                Gizmos.DrawWireSphere(pos2, 0.15f);
                
                // Draw line between the two triggers (yellow)
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(pos1, pos2);
                
                // Draw star spawn position (red sphere)
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(spawnPos, 0.15f);
                Gizmos.DrawWireSphere(spawnPos, 0.2f);
            }
        }
    }
}
