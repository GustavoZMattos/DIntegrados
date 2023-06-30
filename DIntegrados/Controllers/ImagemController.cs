using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotNumerics.LinearAlgebra;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DIntegrados.Controllers
{
    public class ImagemController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private static FifoSemaphore fifoSemaphore = new FifoSemaphore(3);

        public ImagemController(IWebHostEnvironment env)
        {
            _env = env;
        }

        #region desnecessario
        // GET: Imagem
        public ActionResult Index()
        {
            return View();
        }

        // GET: Imagem/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Imagem/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Imagem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Imagem/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Imagem/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Imagem/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Imagem/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        #endregion desnecessario

        [HttpPost]
        public IActionResult ReconstructImage([FromBody] ModeloRecebeServer data)
        {
            while (PodeProcessar(data))
                ;
            return Ok();
        }

        private void ReconstructImageData(List<float> data, string alg, out Matrix x, out int count)
        {
            Matrix H, g;
            int lin, col, s, n, h;
            if (data.Count == 50816)
            {
                lin = 50816;
                col = 3600;
                s = 794;
                n = 64;
                h = 1;
            }
            else
            {
                lin = 27904;
                col = 900;
                s = 436;
                n = 64;
                h = 2;
            }
            H = new Matrix(lin, col);
            string path = $"{_env.ContentRootPath}\\Data\\Arquivos\\H-{h}.csv";
            System.IO.StreamReader file = new System.IO.StreamReader(path);
            for (int i = 0; i < lin; i++)
            {
                string[] bufferA = file.ReadLine().Split(',');
                for (int j = 0; j < col; j++)
                    if (!String.IsNullOrWhiteSpace(bufferA[j]))
                        H[i, j] = float.Parse(bufferA[j], CultureInfo.InvariantCulture);
            }
            file.Close();

            float[] ultravector = SoundGain(s, n, data);

            g = new Matrix(lin, 1);
            for (int i = 0; i < lin; i++)
            {
                g[i, 0] = ultravector[i];
            }

            if (alg == "CGNR")
                CGNR(H, g, out x, out count);
            else if (alg == "CGNE")
                CGNE(H, g, out x, out count);
            else
            {
                x = new Matrix(1, 1);
                count = 1;
            }
            GC.Collect();
        }

        private void SaveImage(Matrix x, string alg, double tempo, int count)
        {
            int tam;
            if (x.RowCount == 3600)
                tam = 60;
            else
                tam = 30;
            Bitmap bmp = new Bitmap(tam, tam);
            double max = double.NegativeInfinity, min = double.PositiveInfinity;
            for (int i = 0; i < x.RowCount; i++)
            {
                if (x[i, 0] > max)
                    max = x[i, 0];
                if (x[i, 0] < min)
                    min = x[i, 0];
            }
            int k = 0;
            int value;
            for (int i = 0; i < tam; i++)
                for (int j = 0; j < tam; j++)
                {
                    value = (int)((255 / (max - min)) * (x[k, 0] - min));
                    bmp.SetPixel(i, j, Color.FromArgb(value, value, value));
                    k++;
                }
            bmp.Save($"{_env.ContentRootPath}\\Data\\Imagens\\Img{alg} U{User.Identity.Name} Tam{tam} Temp{Math.Round(tempo)} I{count}.bmp");
            GC.Collect();
        }

        static float[] SoundGain(int l, int c, IList<float> sinal)
        {
            double ganhoNovo;
            float[] g = new float[sinal.Count];
            int aux = 0;

            foreach (float ganho in sinal)
            {
                g[aux] = ganho;
                aux++;
            }
            aux = 0;
            for (int i = 0; i < c; i++)
            {
                for (int j = 0; j < l; j++)
                {
                    ganhoNovo = 100 + 1 / 20 * j * Math.Sqrt(j);
                    g[aux] = g[aux] * (float)ganhoNovo;
                    aux++;
                }
            }

            return g;

        }

        static Matrix MatrixMultTranpose(Matrix a, Matrix b)
        {
            int col = b.ColumnCount, row = a.ColumnCount;
            int m = a.RowCount;
            Matrix c = new Matrix(row, col);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < m; k++)
                        sum += a[k, i] * b[k, j];
                    c[i, j] = sum;
                }
            }
            return c;

        }

        static double norm2(Matrix matrix)
        {
            double somatorio = 0;
            int size = matrix.RowCount;
            for (int i = 0; i < size; i++)
            {
                somatorio += Math.Pow(matrix[i, 0], 2);
            }
            return Math.Sqrt(somatorio);
        }

        static void CGNE(Matrix H, Matrix g, out Matrix f, out int count)
        {
            f = new Matrix(H.ColumnCount, 1);
            for (int i = 0; i < H.ColumnCount; i++)//f0=0
            {
                f[i, 0] = 0;
            }
            Matrix r = g - H * f;//r0 = g - Hf0
            Matrix p = MatrixMultTranpose(H, r); //p0 = HTr0
            double a, B, calculoErro;
            double rtXr = (r.Transpose() * r)[0, 0]; //=riT * ri serve pra não precisar calcular duas vezes
            double ritXri;//=ri+1T * ri+1 serve pra não precisar calcular duas vezes
            Matrix ri;// ri+1
            count = 0;
            do //Falta timer
            {
                a = rtXr / (p.Transpose() * p)[0, 0];//ai = riT * ri / piT * pi
                f = f + p.Multiply(a);//fi+1 = fi + ai * pi
                ri = r - (H * p).Multiply(a);//ri+1 = ri - ai * H * pi
                ritXri = (ri.Transpose() * ri)[0, 0];//=ri+1T * ri+1
                B = ritXri / rtXr;//Bi = ri+1T * ri+1 / riT * ri
                p = MatrixMultTranpose(H, ri) + p.Multiply(B);//pi = HT * ri+1 + Bi * pi
                calculoErro = Math.Abs(norm2(ri) - norm2(r));
                r = ri;// ri = ri+1
                rtXr = ritXri;
                count++;
            } while (calculoErro > 0.0003f && count < 15);
            GC.Collect();
        }

        static void CGNR(Matrix H, Matrix g, out Matrix f, out int count)
        {
            f = new Matrix(H.ColumnCount, 1);
            for (int i = 0; i < H.ColumnCount; i++)//f0=0
            {
                f[i, 0] = 0;
            }
            Matrix r = g - H * f;//r0 = g - Hf0
            Matrix zi = MatrixMultTranpose(H, r); //z0 = Ht * r0
            Matrix p = zi; //p0 = HTr0
            double a, B, calculoErro;
            count = 0;
            double rtXr = (r.Transpose() * r)[0, 0]; //=riT * ri serve pra não precisar calcular duas vezes
            Matrix ri, w, z;// ri+1
            do //Falta timer
            {
                w = H.Multiply(p);
                a = Math.Pow(norm2(zi), 2) / Math.Pow(norm2(w), 2);
                f = f + p.Multiply(a);//fi+1 = fi + ai * pi
                ri = r - (w).Multiply(a);//ri+1 = ri - ai * H * pi
                z = zi;
                zi = MatrixMultTranpose(H, ri);
                B = Math.Pow(norm2(zi), 2) / Math.Pow(norm2(z), 2);
                p = zi + B * p;
                calculoErro = Math.Abs(norm2(ri) - norm2(r));
                count++;
            } while (calculoErro > 0.0003f && count < 15);
            GC.Collect();
        }

        static double GetCurrentMemoryUsage()
        {
            var output = "";
            Process currentProcess = Process.GetCurrentProcess();
            var info = new ProcessStartInfo();
            info.FileName = "wmic";
            info.Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value";
            info.RedirectStandardOutput = true;
            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }
            var lines = output.Trim().Split("\n");
            var totalMemoryParts = lines[1].Split("=", StringSplitOptions.RemoveEmptyEntries);
            var memoriaTotal = Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0);
            var memoriaUtilizada = currentProcess.WorkingSet64 / 100000;
            double memoryUsagePercentage = (memoriaUtilizada / memoriaTotal) * 100;

            return memoryUsagePercentage;
        }

        public bool PodeProcessar(ModeloRecebeServer data)
        {
            if (GetCurrentMemoryUsage() < 25)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                ReconstructImageData(data.Sinal, data.Agoritimo.Split(' ')[0], out Matrix reconstructedImage, out int count); // Reconstrói a imagem
                stopwatch.Stop();
                SaveImage(reconstructedImage, data.Agoritimo, stopwatch.Elapsed.TotalSeconds, count); // Salva a imagem
                fifoSemaphore.Release();
                return false;
            }
            else
            {
                fifoSemaphore.WaitAsync().GetAwaiter().GetResult();
                return true;
            }
        }

        public class ModeloRecebeServer
        {
            public List<float> Sinal { get; set; }
            public string Agoritimo { get; set; }
        }
    }
}