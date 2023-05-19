namespace Server.Models
{
    public class Signal
    {
        public string User { get; set; }
        public double Gain { get; set; }
        public byte[] Data { get; set; }
    }
}