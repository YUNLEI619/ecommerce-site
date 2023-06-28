namespace ShoppingCart_T8.Models
{
    public class LogStats //Largely taken from SA ASP .NET Workshop, modified slightly - as an Easter Egg entity, a thank you for the Guidance :~)
    {
        //Properties
        private readonly Dictionary<string, int> stats;

        //Constructors
        public LogStats()
        {
            stats = new Dictionary<string, int>();
        }

        //Methods
        public void Add(string path)
        {
            if (stats.ContainsKey(path)) { stats[path]++; }
            else { stats[path] = 1; }
        }

        public Dictionary<string, int> Get() => stats;
    }
}
