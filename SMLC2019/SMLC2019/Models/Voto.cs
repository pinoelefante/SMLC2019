using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMLC2019.Models
{
    public class Voto
    {
        public int partito { get; set; }
        public int? preferenza1 { get; set; }
        public int? preferenza2 { get; set; }
        public int seggio { get; set; }
        [PrimaryKey]
        public long tempo { get; set; }
    }
}
