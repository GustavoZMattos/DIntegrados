using System;
using System.Collections.Generic;
using System.Text;

namespace Cliente
{
    class ServerPerformance
    {
        public int MemoryUsageMb { get; set; }
        public double CpuUsagePercent { get; set; }
        public int TotalTimeMs { get; set; }
    }
}
