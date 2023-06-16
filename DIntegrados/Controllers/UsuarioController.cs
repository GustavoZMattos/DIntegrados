using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DIntegrados.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly HttpClient _httpClient;

        // GET: Usuario
        public ActionResult Index()
        {
            return View();
        }

        // GET: Usuario/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Usuario/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Usuario/Create
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

        // GET: Usuario/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Usuario/Edit/5
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

        // GET: Usuario/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Usuario/Delete/5
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
        public async Task<IActionResult> SendData([FromBody] string g)
        {
            List<float> data = CarregarImagem(g);
            await SendDataToServer(data);
            return Ok();
        }

        private List<float> CarregarImagem(string g)
        {
            var ultrasound = new List<float>();
            string path = @"C:\Users\a1762680\Downloads\" + g + ".csv";
            string[] buffer = System.IO.File.ReadAllText(path).Split('\n');
            ultrasound = new List<float>();
            foreach (string number in buffer)
            {
                if (!String.IsNullOrWhiteSpace(number))
                    ultrasound.Add(float.Parse(number, CultureInfo.InvariantCulture));
            }
            Console.WriteLine(ultrasound.Count.ToString());
            Console.WriteLine("Carreguei");
            return ultrasound;
        }

        private async Task SendDataToServer(List<float> data)
        {
            string apiUrl = "https://localhost:44321" + Url.Action("ReconstructImage", "Imagem"); // URL do servidor de reconstrução
            await _httpClient.PostAsJsonAsync(apiUrl, data);
        }
    }
}