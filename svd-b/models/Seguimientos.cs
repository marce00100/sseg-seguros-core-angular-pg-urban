using System;

namespace SVD.Models
{

    public class Apertura
    {
        public int id_apertura { get; set; }
        public DateTime fecha_corte { get; set; }
        public bool iniciado { get; set; }
        public DateTime? fecha_inicio_envios { get; set; }
        public DateTime? fecha_detiene_envios { get; set; }
        public bool activo { get; set; }
        public string estado { get; set; }   /// pueden ser [ 1 = iniciado, 2 = detenido, 3 = concluido ]
        public int id_configuracion { get; set; }
        public decimal factor_tc { get; set; }
        public int? creado_por { get; set; }
        public DateTime? creado_en { get; set; }
        public int? modificado_por { get; set; }
        public DateTime? modificado_en { get; set; }
    }

    public class SeguimientoEnvio
    {
        public int id_seguimiento_envio { get; set; }
        public int id_apertura { get; set; }
        public string cod_entidad { get; set; }
        public DateTime fecha_envio { get; set; }
        public string estado { get; set; }  /// pueden ser [ 0 = NE : NoEnviado, 1 = EF: ErrorFormato, 2 = EC: ErrorContenido, 3 = V : valido ]
        public bool activo { get; set; }

        public bool valido { get; set; }
        public string observaciones { get; set; }
        public int? id_consolidacion { get; set; }

        public int? creado_por { get; set; }
        public DateTime? creado_en { get; set; }
        public int? modificado_por { get; set; }
        public DateTime? modificado_en { get; set; }
        public string estado_cierre { get; set; }


    }


}