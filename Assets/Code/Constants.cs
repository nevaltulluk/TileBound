namespace Code
{
    public static class Constants
    {
        public static readonly int TotalTime = 60;
        public static readonly int ChangeShiftTimer = 10;
        public static readonly int RequiredStarCount = 20;
    }
    
    public enum SpecialTiles
    {
        Mountain,
        Market,
        Honeycomb,
        Fountain,
        FerrisWheel,
        WindMill
    }

    public enum Booster
    {
        Coin,
        Time,
        Magnet
    }
}