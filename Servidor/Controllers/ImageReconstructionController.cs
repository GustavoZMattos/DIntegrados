using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Server.Models;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageReconstructionController : ControllerBase
    {
        [HttpPost("reconstruct")]
        public async Task<ReconstructedImage> ReconstructImageAsync([FromBody] Signal signal)
        {
            // Load reconstruction model based on signal parameters
            ReconstructionModel model = LoadModel(signal.User, signal.Gain);

            // Reconstruct image using the model
            Stopwatch stopwatch = Stopwatch.StartNew();
            int numIterations = await Task.Run(() => model.ReconstructImage(signal));
            stopwatch.Stop();

            // Create reconstruction result object
            var reconstructedImage = new ReconstructedImage
            {
                User = signal.User,
                NumIterations = numIterations,
                ReconstructionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ImageData = Convert.ToBase64String(model.GetImageBytes())
            };

            return reconstructedImage;
        }

        [HttpGet("performance")]
        public async Task<ServerPerformance> GetServerPerformanceAsync()
        {
            // Measure memory usage and CPU usage for the current process
            Process currentProcess = Process.GetCurrentProcess();
            await Task.Delay(1000); // Wait for a short time to get more accurate CPU usage
            double cpuUsagePercent = currentProcess.TotalProcessorTime.Ticks == 0 ? 0 : (100.0 * currentProcess.ProcessorAffinity.Ticks / currentProcess.TotalProcessorTime.Ticks);
            int memoryUsageMb = (int)(currentProcess.PrivateMemorySize64 / (1024 * 1024));

            // Create performance result object
            var serverPerformance = new ServerPerformance
            {
                MemoryUsageMb = memoryUsageMb,
                CpuUsagePercent = cpuUsagePercent
            };

            return serverPerformance;
        }

        private ReconstructionModel LoadModel(string user, double gain)
        {
            // Load the appropriate reconstruction model based on the user and gain
            // This is just a placeholder method; the actual implementation would depend on the specific application requirements
            return new ReconstructionModel();
        }
    }
}