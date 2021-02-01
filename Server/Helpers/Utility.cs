namespace TrafficManager.Helpers
{
    public static class Utility
    {
        private static int UUID { get; set; } = 0;

        public static int GetNextIdentifier()
        {
            return UUID++;
        }
    }
}
