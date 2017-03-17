using System;

namespace SVD.Models
{
    public class Entidad
    {

        public string cEmpresa { get; set; }
        public string tSigla { get; set; }
        public string tNombre { get; set; }
        public int cTipoEntidad { get; set; }
        public char bHabilitado { get; set; }
        public string tNombreCorto { get; set; }
        public string tFuncionamiento { get; set; }
        public string tAdecuacion { get; set; }
        public int? creado_por { get; set; }
        public DateTime? creado_en { get; set; }
        public int? modificado_por { get; set; }
        public DateTime? modificado_en { get; set; }

    }
}