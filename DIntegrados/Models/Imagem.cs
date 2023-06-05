using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DIntegrados.Models
{
    public class Imagem
    {
        public int id_imagem { get; set; }

        public int Id_User { get; set; }

        public DateTime dataInicio { get; set; }

        public DateTime dataFim { get; set; }

        public int tamanho { get; set; }

        public int iteracoes { get; set; }

        //public Bitmap reconstruida { get; set; }  
    }
}
