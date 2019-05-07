using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMLC2019.Models
{
    public class Partito
    {
        [PrimaryKey]
        public int id { get; set; }
        public string nome { get; set; }
        public string sindaco { get; set; }
        public string sindaco_foto { get; set; }
        public string logo { get; set; }
        public int ordine { get; set; }
    }
}
