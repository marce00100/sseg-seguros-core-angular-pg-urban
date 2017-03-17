using System;

namespace SVD.Models
{
    public class val_partes_produccion
    {
        public string entidad { get; set; }
        public string tipo_parte { get; set; }
        public string departamento { get; set; }
        public string sector { get; set; }
        public string moneda { get; set; }
        public DateTime fecha { get; set; }
        public string modalidad { get; set; }
        public string ramo { get; set; }
        public string poliza { get; set; }
        public decimal capital_asegurado { get; set; }
        public int polizas_nuevas { get; set; }
        public int polizas_renovadas { get; set; }
        public decimal prima_directa_m { get; set; }
        public decimal prima_directa { get; set; }
        public decimal capital_anulado { get; set; }
        public int polizas_anuladas { get; set; }
        public decimal prima_anulada_m { get; set; }
        public decimal prima_anulada { get; set; }
        public decimal capital_asegurado_neto { get; set; }
        public int polizas_netas { get; set; }
        public decimal prima_neta_m { get; set; }
        public decimal prima_neta { get; set; }
        public decimal prima_aceptada_reaseguro_m { get; set; }
        public decimal prima_aceptada_reaseguro { get; set; }
        public decimal prima_cedida_reaseguro_m { get; set; }
        public decimal prima_cedida_reaseguro { get; set; }
        public decimal anulacion_prima_cedida_reaseguro_m { get; set; }
        public decimal anulacion_prima_cedida_reaseguro { get; set; }
        public int index_fila { get; set; }
    }

    public class val_partes_siniestros
    {
        public string entidad { get; set; }
        public string tipo_parte { get; set; }
        public string departamento { get; set; }
        public string sector { get; set; }
        public string moneda { get; set; }
        public DateTime fecha { get; set; }
        public string modalidad { get; set; }
        public string ramo { get; set; }
        public string poliza { get; set; }
        public int num_sins_denunciados { get; set; }
        public decimal sins_denunciados { get; set; }
        public int num_sins_liquidados { get; set; }
        public decimal sins_liquidados_m { get; set; }
        public decimal sins_liquidados { get; set; }
        public decimal sins_reaseguro_aceptado_m { get; set; }
        public decimal sins_reaseguro_aceptado { get; set; }
        public decimal sins_reaseguro_cedido_m { get; set; }
        public decimal sins_reaseguro_cedido { get; set; }
        public int index_fila { get; set; }
    }

    public class val_balance_estado_resultados
    {
        public string entidad { get; set; }
        public DateTime fecha { get; set; }
        public string cuenta_financiera { get; set; }
        public string moneda { get; set; }
        public string cuenta_tecnica { get; set; }
        public decimal saldo_debe_anterior { get; set; }
        public decimal saldo_haber_anterior { get; set; }
        public decimal movimiento_debe { get; set; }
        public decimal movimiento_haber { get; set; }
        public decimal saldo_debe_actual { get; set; }
        public decimal saldo_haber_actual { get; set; }
        public int index_fila { get; set; }
    }
    public class val_produccion_corredoras
    {
        public string es_corredora { get; set; }
        public string entidad { get; set; }
        public string corredora { get; set; }
        public string moneda { get; set; }
        public DateTime fecha { get; set; }
        public string modalidad { get; set; }
        public string ramo { get; set; }
        public string poliza { get; set; }
        public decimal produccion { get; set; }
        public int index_fila { get; set; }
    }

    public class val_partes_produccion_ramos
    {
        public string entidad { get; set; }
        public DateTime fecha { get; set; }
        public string modalidad { get; set; }
        public string ramo { get; set; }
        public decimal primas_directas { get; set; }
        public decimal anulado_primas_directas { get; set; }
        public decimal primas_netas_anulaciones { get; set; }
        public decimal primas_aceptadas_reaseguro_nacional { get; set; }
        public decimal primas_aceptadas_reaseguro_extranjero { get; set; }
        public decimal total_primas_aceptadas_reaseguro { get; set; }
        public decimal total_primas_netas { get; set; }
        public decimal primas_cedidas_reaseguro_nacional { get; set; }
        public decimal primas_cedidas_reaseguro_extranjero { get; set; }
        public decimal total_primas_cedidas { get; set; }
        public decimal total_primas_netas_retenidas { get; set; }
        public decimal index_fila { get; set; }
    }

    public class val_partes_siniestros_ramos
    {
        public string entidad { get; set; }
        public DateTime fecha { get; set; }
        public string modalidad { get; set; }
        public string ramo { get; set; }
        public decimal sins_directos { get; set; }
        public decimal sins_reaseguro_aceptado_nacional { get; set; }
        public decimal sins_reaseguro_aceptado_extranjero { get; set; }
        public decimal total_sins_reaseguro_aceptado { get; set; }
        public decimal sins_totales { get; set; }
        public decimal sins_reembolsados_nacional { get; set; }
        public decimal sins_reembolsados_extranjero { get; set; }
        public decimal total_sins_reembolsados { get; set; }
        public decimal total_sins_retenidos { get; set; }
        public decimal index_fila { get; set; }
    }


    public class val_partes_seguros_largo_plazo
    {
        public string entidad { get; set; }
        public DateTime fecha { get; set; }
        public string modalidad { get; set; }
        public string ramo { get; set; }
        public decimal capital_asegurado_total { get; set; }
        public decimal capital_asegurado_cedido { get; set; }
        public decimal capital_asegurado_retenido { get; set; }
        public decimal reserva_matematica_total { get; set; }
        public decimal reserva_matematica_reasegurador { get; set; }
        public decimal reserva_matematica_retenida { get; set; }
        public decimal index_fila { get; set; }
    }


    public class val_partes_produccion_totales
    {
        public string entidad { get; set; }
        public DateTime fecha { get; set; }
        public decimal capital_asegurado { get; set; }
        public decimal prima_directa_m { get; set; }
        public decimal prima_directa { get; set; }
        public decimal capital_anulado { get; set; }
        public decimal prima_anulada_m { get; set; }
        public decimal prima_anulada { get; set; }
        public decimal capital_asegurado_neto { get; set; }
        public decimal prima_neta_m { get; set; }
        public decimal prima_neta { get; set; }
        public decimal prima_aceptada_reaseguro_m { get; set; }
        public decimal prima_aceptada_reaseguro { get; set; }
        public decimal prima_cedida_reaseguro_m { get; set; }
        public decimal prima_cedida_reaseguro { get; set; }
        public decimal anulacion_prima_cedida_reaseguro_m { get; set; }
        public decimal anulacion_prima_cedida_reaseguro { get; set; }

    }


}