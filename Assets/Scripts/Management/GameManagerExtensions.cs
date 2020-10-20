namespace Aspekt.Hex
{
    public static class GameManagerExtensions
    {
        public static bool IsRunning(this GameManager mgr)
        {
            return mgr.Data != null && mgr.Data.IsRunning;
        }
    }
}