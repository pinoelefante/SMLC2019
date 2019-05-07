using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMLC2019.Models
{
    public class Candidato
    {
        [PrimaryKey]
        public int id { get; set; }
        public string nome { get; set; }
        public string cognome { get; set; }
        public string sesso { get; set; }
        public int partito { get; set; }
        public string foto { get; set; }

        public override string ToString()
        {
            return $"{cognome} {nome}";
        }
    }
}
