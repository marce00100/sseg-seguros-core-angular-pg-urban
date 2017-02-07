using System;

namespace SVD.Models
{
    public class TipoEntidad
    {
        public int cTipoEntidad { get; set; }
        public string tDescripcion { get; set; }
        public string tDescripcionCorta { get; set; }
        public string tCodigo { get; set; }
        public int? creado_por { get; set; }
        public DateTime? creado_en { get; set; }
        public int? modificado_por { get; set; }
        public DateTime? modificado_en { get; set; }

    }
}