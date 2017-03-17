using System;

namespace SVD.Models
{
    public class Ramo
    {
        public string cModalidad { get; set; }
        public string cRamo { get; set; }
        public string tDescripcion { get; set; }
        public int idBoletin { get; set; }
        public string tSigla { get; set; }
        public int? creado_por { get; set; }
        public DateTime? creado_en { get; set; }
        public int? modificado_por { get; set; }
        public DateTime? modificado_en { get; set; }

    }
}