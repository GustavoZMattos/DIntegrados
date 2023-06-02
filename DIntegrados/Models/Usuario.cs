using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DIntegrados.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        public string Nome { get; set; }

        public string tamanho { get; set; }

        public DateTime dataInicio { get; set; }

        public DateTime dataFim { get; set; }

        public int iteracoes { get; set; }

        public string Email { get; set; }

        public string Senha { get; set; }

        //public IList<float> Imagem { get; set; }
    }
}
