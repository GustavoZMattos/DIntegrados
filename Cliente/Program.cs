using System;
using System.Threading.Tasks;

namespace Cliente
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var imageReconstruction = new ImageReconstruction();
            var signals = await imageReconstruction.GenerateSignalsAsync(5, 1000, 5000);
            var reconstructedImages = await imageReconstruction.ReconstructImagesAsync(signals, "http://localhost:5000/reconstruct");
            var serverPerformance = await imageReconstruction.MonitorServerAsync("http://localhost:5000/performance");
            imageReconstruction.PrintReport(reconstructedImages);
            imageReconstruction.PrintServerPerformance(serverPerformance);
        }
    }
}
