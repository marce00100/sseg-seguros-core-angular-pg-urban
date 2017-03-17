using System;

namespace SVD.Models
{
    public class Configuracion
    {

        public int id_configuracion { get; set; }
        public bool usa_factor_tc { get; set; }
        public string moneda_factor_tc { get; set; }
        public decimal margen_partes_ef { get; set; }
        public decimal margen_validacion_soat { get; set; }
        public bool activo { get; set; }
        public int? modificado_por { get; set; }
        public DateTime? modificado_en { get; set; }


    }

}