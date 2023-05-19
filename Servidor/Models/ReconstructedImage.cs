namespace Server.Models
{
    public class ReconstructedImage
    {
        public string User { get; set; }
        public int NumIterations { get; set; }
        public int ReconstructionTimeMs { get; set; }
        public string ImageData { get; set; }
    }
}