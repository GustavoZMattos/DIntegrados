using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cliente
{
    class ImageReconstruction
    {
        private readonly Random _random = new Random();

        public async Task<List<Signal>> GenerateSignalsAsync(int numSignals, int minDelayMs, int maxDelayMs)
        {
            var signals = new List<Signal>();
            for (int i = 0; i < numSignals; i++)
            {
                var user = $"User{i}";
                var gain = _random.NextDouble() * 10;
                var imageModel = $"Model{i}";
                var signal = new Signal(user, gain, imageModel);
                signals.Add(signal);

                var delayMs = _random.Next(minDelayMs, maxDelayMs);
                await Task.Delay(delayMs);
            }

            return signals;
        }

        public async Task<List<ReconstructedImage>> ReconstructImagesAsync(List<Signal> signals, string serverUrl)
        {
            var stopwatch = new Stopwatch();
            var reconstructedImages = new List<ReconstructedImage>();

            foreach (var signal in signals)
            {
                stopwatch.Start();

                var json = JsonConvert.SerializeObject(signal);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsync(serverUrl, content);
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var reconstructedImage = JsonConvert.DeserializeObject<ReconstructedImage>(responseJson);
                    reconstructedImage.User = signal.User;
                    reconstructedImage.NumIterations = reconstructedImage.ImageData.Length;
                    reconstructedImage.ReconstructionTimeMs = (int)stopwatch.ElapsedMilliseconds;
                    reconstructedImages.Add(reconstructedImage);
                }

                stopwatch.Reset();
            }

            return reconstructedImages;
        }

        public async Task<ServerPerformance> MonitorServerAsync(string serverUrl)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(serverUrl);
                var responseJson = await response.Content.ReadAsStringAsync();
                var serverPerformance = JsonConvert.DeserializeObject<ServerPerformance>(responseJson);
                serverPerformance.TotalTimeMs = (int)stopwatch.ElapsedMilliseconds;
                return serverPerformance;
            }
        }

        public void PrintReport(List<ReconstructedImage> reconstructedImages)
        {
            Console.WriteLine("========== Reconstructed Images Report ==========");
            foreach (var reconstructedImage in reconstructedImages)
            {
                Console.WriteLine($"User: {reconstructedImage.User}");
                Console.WriteLine($"Num Iterations: {reconstructedImage.NumIterations}");
                Console.WriteLine($"Reconstruction Time (ms): {reconstructedImage.ReconstructionTimeMs}");
                Console.WriteLine($"Image Data: {reconstructedImage.ImageData}");
                Console.WriteLine();
            }
        }

        public void PrintServerPerformance(ServerPerformance serverPerformance)
        {
            Console.WriteLine("========== Server Performance Report ==========");
            Console.WriteLine($"Memory Usage (MB): {serverPerformance.MemoryUsageMb}");
            Console.WriteLine($"CPU Usage (%): {serverPerformance.CpuUsagePercent}");
            Console.WriteLine($"Total Time (ms): {serverPerformance.TotalTimeMs}");
        }
    }
}