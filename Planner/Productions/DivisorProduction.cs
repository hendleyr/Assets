namespace Assets.Planner.Productions
{
    public class DivisorProduction
    {
        public float Magnitude { get; set; }
        public bool IsAbsolute { get; set; }
        public string SnapPlaneKey { get; set; }
        public RegistrarProduction DividendProduction { get; set; }
    }
}
