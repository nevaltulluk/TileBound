namespace Code
{
    public enum EventType
    {
        None,
        Spring,
        Summer,
        Fall,
        Winter,
    }
    
    [System.Serializable]
    public class HexData
    {
        public int q;
        public int r;
        public EventType eventType;
        public int prefabIndex;
        public SpecialTiles? specialTileType; // null if not a special tile
    }
    
}