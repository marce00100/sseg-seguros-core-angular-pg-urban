using System;


namespace SVD.Models
{
    public class Archivo
    {
        public int id_archivo { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public string periodos_envio { get; set; }
        public string cods_tipo_entidad { get; set; }
        public string validacion { get; set; }
        public string codigo { get; set; }
        // public string formato { get; set; }
        public bool activo { get; set; }
        public string id_registro { get; set; }
        public bool vigente { get; set; }
        public bool eliminado { get; set; }
        public int? creado_por { get; set; }
        public DateTime? creado_en { get; set; }
        public int? modificado_por { get; set; }
        public DateTime? modificado_en { get; set; }

    }
}