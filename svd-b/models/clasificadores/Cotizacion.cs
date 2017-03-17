using System;

namespace SVD.Models
{
    public class Cotizacion
    {
        // public int id_cotizacion { get; set; }
        // public int cod_cla_moneda { get; set; }
        // public DateTime fecha { get; set; }
        // public decimal? compra { get; set; }
        // public decimal? venta { get; set; }
        // public decimal? id_cla_grupo_entidad { get; set; }
        // public string comentario { get; set; }
        // public string id_registro { get; set; }
        // public bool vigente { get; set; }
        // public bool eliminado { get; set; }
        // public int creado_por { get; set; }
        // public DateTime creado_en { get; set; }
        // public int modificado_por { get; set; }
        // public DateTime modificado_en { get; set; }


        public DateTime fTipoCambio { get; set; }
        public decimal mCompra { get; set; }
        public decimal? mVenta { get; set; }
        public decimal? mDEG { get; set; }
        public decimal? mEuroBS { get; set; }
        public decimal? mEuroDA { get; set; }
        public decimal? mUFV { get; set; }
        public int fecha_6 { get; set; }
        public int fecha_8 { get; set; }
        public bool eliminado { get; set; }
        public int creado_por { get; set; }
        public DateTime creado_en { get; set; }
        public int modificado_por { get; set; }
        public DateTime modificado_en { get; set; }

    }
}