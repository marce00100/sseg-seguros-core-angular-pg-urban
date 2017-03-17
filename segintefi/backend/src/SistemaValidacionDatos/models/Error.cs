using System;

namespace SVD.Models
{
    public class Error
    {
        public int id_error_envio { get; set; }
        public int id_seguimiento_envio { get; set; }
        public string cod_error { get; set; }
        public string archivo { get; set; }
        public int fila { get; set; }
        public int columna { get; set; }
        public string descripcion_puntual { get; set; }
        public bool valido { get; set; }
        public string observaciones { get; set; }

        public int? creado_por { get; set; }
        public DateTime? creado_en { get; set; }
        public int? modificado_por { get; set; }
        public DateTime? modificado_en { get; set; }

        public string error { get; set; }
        public string desc_error { get; set; }
        public string nombre_error { get; set; }
        public int estadoValidez { get; set; }

    }

}