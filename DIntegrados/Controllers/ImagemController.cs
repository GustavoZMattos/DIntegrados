using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using DotNumerics.LinearAlgebra;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DIntegrados.Controllers
{
    public class ImagemController : Controller
    {
        private Matrix H, g, x;
        private readonly IWebHostEnvironment _env;

        public ImagemController(IWebHostEnvironment env)
        {
            _env = env;
        }

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

        [HttpPost]
        public IActionResult ReconstructImage([FromBody] ModeloRecebeServer data)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            ReconstructImageData(data.Sinal, data.Agoritimo.Split(' ')[0], out Matrix reconstructedImage, out int count); // Reconstrói a imagem
            stopwatch.Stop();
            SaveImage(reconstructedImage, data.Agoritimo, stopwatch.Elapsed.TotalSeconds, count); // Salva a imagem
            return Ok();
        }

        private void ReconstructImageData(List<float> data, string alg, out Matrix x, out int count)
        {
            int lin, col, s, n;
            if (data.Count == 50816)
            {
                lin = 50816;
                col = 3600;
                s = 794;
                n = 64;
            }
            else
            {
                lin = 27904;
                col = 900;
                s = 436;
                n = 64;
            }
            H = new Matrix(lin, col);
            string path = $"{_env.ContentRootPath}\\Data\\Arquivos\\H-1.csv";
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
            for (int i = 0; i < lin; i++)//constroi matrix do vetor de entrada
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
        }

        private void SaveImage(Matrix x, string alg, double tempo, int count)
        {
            //Parte que atribui valor de 0 até 255 para a imagem
            int tam;
            if (x.RowCount == 3600)
                tam = 60;
            else
                tam = 30;
            Bitmap bmp = new Bitmap(tam, tam);
            double max = double.NegativeInfinity, min = double.PositiveInfinity;
            for (int i = 0; i < x.RowCount; i++)//Calcula valor máximo e mínimo da imagem
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
        //serve pra multiplicar com transposta sem precisar alocar nova matrix na memória, por que da out of memory exception
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

        /*H é a matrix gigante de tramanho(50816, 3600), 
         * g é o vetor de ultrasom passado de tamanho (50816, 1)
         * f é a saida de tamanho (3600,1) que vai ser iniciada em 0
         * O tamanho 50816 vem do tamannho do vetor de entrada
         * O tamanho 3600 vem do tamanho da imagem final (60X60)
         */
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
        }

        public class ModeloRecebeServer
        {
            public List<float> Sinal { get; set; }
            public string Agoritimo { get; set; }
        }
    }
}