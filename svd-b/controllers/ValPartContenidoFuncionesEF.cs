using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using SVD.Models;
using System;
using Newtonsoft.Json;
using System.Dynamic;



namespace SVD.Controllers
{
    public partial class ValidacionesController
    {
        private val_partes_produccion_totales obtenerPartesProduccioTotales(decimal cambio_2, decimal cambio_4, decimal cambio_5, bool redondeo = true)
        {
            val_partes_produccion_totales partes_produccion_totales = con.Query<val_partes_produccion_totales>(@"SELECT 
                                                                            sum(CASE  moneda WHEN '1' THEN Coalesce(capital_asegurado,0)
                                                                                    WHEN '2' THEN Coalesce (capital_asegurado,0)*@cambio_2
                                                                                    WHEN '4' THEN Coalesce (capital_asegurado,0)*@cambio_4
                                                                                    WHEN '5' THEN Coalesce (capital_asegurado,0)*@cambio_5
                                                                                    END) AS capital_asegurado,
                                                                            sum(CASE  moneda WHEN '1' THEN coalesce (prima_directa_m,0)
                                                                                    WHEN '2' THEN coalesce (prima_directa_m,0)*@cambio_2
                                                                                    WHEN '4' THEN coalesce (prima_directa_m,0)*@cambio_4
                                                                                    WHEN '5' THEN coalesce (prima_directa_m,0)*@cambio_5
                                                                                    END) AS prima_directa_m,
                                                                            SUM(coalesce(prima_directa,0)*1) AS prima_directa,
                                                                            sum(CASE  moneda WHEN '1' THEN coalesce (capital_anulado,0)
                                                                                    WHEN '2' THEN coalesce (capital_anulado,0)*@cambio_2
                                                                                    WHEN '4' THEN coalesce (capital_anulado,0)*@cambio_4
                                                                                    WHEN '5' THEN coalesce (capital_anulado,0)*@cambio_5
                                                                                    END) AS capital_anulado,
                                                                            sum(CASE  moneda WHEN '1' THEN coalesce (prima_anulada_m,0)
                                                                                    WHEN '2' THEN coalesce (prima_anulada_m,0)*@cambio_2
                                                                                    WHEN '4' THEN coalesce (prima_anulada_m,0)*@cambio_4
                                                                                    WHEN '5' THEN coalesce (prima_anulada_m,0)*@cambio_5
                                                                                    END) AS prima_anulada_m,
                                                                            sum(coalesce(prima_anulada,0)*1) AS prima_anulada,
                                                                            sum(CASE  moneda WHEN '1' THEN coalesce (capital_asegurado_neto,0)
                                                                                    WHEN '2' THEN coalesce (capital_asegurado_neto,0)*@cambio_2
                                                                                    WHEN '4' THEN coalesce (capital_asegurado_neto,0)*@cambio_4
                                                                                    WHEN '5' THEN coalesce (capital_asegurado_neto,0)*@cambio_5
                                                                                    END) AS capital_asegurado_neto,
                                                                            sum(CASE  moneda WHEN '1' THEN coalesce (prima_neta_m,0)
                                                                                    WHEN '2' THEN coalesce (prima_neta_m,0)*@cambio_2
                                                                                    WHEN '4' THEN coalesce (prima_neta_m,0)*@cambio_4
                                                                                    WHEN '5' THEN coalesce (prima_neta_m,0)*@cambio_5
                                                                                    END) AS prima_neta_m,
                                                                            sum(coalesce(prima_neta,0)*1) AS prima_neta,
                                                                            sum(CASE  moneda WHEN '1' THEN coalesce (prima_aceptada_reaseguro_m,0)
                                                                                    WHEN '2' THEN coalesce (prima_aceptada_reaseguro_m,0)*@cambio_2
                                                                                    WHEN '4' THEN coalesce (prima_aceptada_reaseguro_m,0)*@cambio_4
                                                                                    WHEN '5' THEN coalesce (prima_aceptada_reaseguro_m,0)*@cambio_5
                                                                                    END) AS prima_aceptada_reaseguro_m,
                                                                            sum(coalesce(prima_aceptada_reaseguro,0)*1) As prima_aceptada_reaseguro,
                                                                            sum(CASE  moneda WHEN '1' THEN coalesce (prima_cedida_reaseguro_m,0)
                                                                                    WHEN '2' THEN coalesce (prima_cedida_reaseguro_m,0)*@cambio_2
                                                                                    WHEN '4' THEN coalesce (prima_cedida_reaseguro_m,0)*@cambio_4
                                                                                    WHEN '5' THEN coalesce (prima_cedida_reaseguro_m,0)*@cambio_5
                                                                                    END) AS prima_cedida_reaseguro_m,
                                                                            sum(coalesce(prima_cedida_reaseguro,0)*1) AS prima_cedida_reaseguro,
                                                                            sum(CASE  moneda WHEN '1' THEN coalesce (anulacion_prima_cedida_reaseguro_m,0)
                                                                                    WHEN '2' THEN coalesce (anulacion_prima_cedida_reaseguro_m,0)*@cambio_2
                                                                                    WHEN '4' THEN coalesce (anulacion_prima_cedida_reaseguro_m,0)*@cambio_4
                                                                                    WHEN '5' THEN coalesce (anulacion_prima_cedida_reaseguro_m,0)*@cambio_5
                                                                                    END) AS anulacion_prima_cedida_reaseguro_m,
                                                                            sum(coalesce(anulacion_prima_cedida_reaseguro,0)*1) AS anulacion_prima_cedida_reaseguro
                                                                                FROM val_partes_produccion 
                                                                                WHERE entidad = @codEntidad AND fecha = @fecha AND moneda != '0'",
                                                                               new { codEntidad = codigoEntidad, fecha = fechaCorte, cambio_2 = cambio_2, cambio_4 = cambio_4, cambio_5 = cambio_5 }).FirstOrDefault();

            con.Close();
            con.Execute("DELETE FROM val_partes_produccion_totales where fecha = @fecha ANd entidad = @entidad", new { fecha = this.fechaCorte, Entidad = this.codigoEntidad });
            con.Close();
            partes_produccion_totales.fecha = fechaCorte;
            partes_produccion_totales.entidad = codigoEntidad;
            string query = @"INSERT INTO val_partes_produccion_totales(
                                                entidad, fecha, capital_asegurado, 
                                                prima_directa_m, prima_directa, capital_anulado, prima_anulada_m, 
                                                prima_anulada, capital_asegurado_neto, prima_neta_m, prima_neta, 
                                                prima_aceptada_reaseguro_m, prima_aceptada_reaseguro, prima_cedida_reaseguro_m, 
                                                prima_cedida_reaseguro, anulacion_prima_cedida_reaseguro_m, anulacion_prima_cedida_reaseguro)
                                        VALUES (@entidad, @fecha, @capital_asegurado, 
                                                @prima_directa_m, @prima_directa, @capital_anulado, @prima_anulada_m, 
                                                @prima_anulada, @capital_asegurado_neto, @prima_neta_m, @prima_neta, 
                                                @prima_aceptada_reaseguro_m, @prima_aceptada_reaseguro, @prima_cedida_reaseguro_m, 
                                                @prima_cedida_reaseguro, @anulacion_prima_cedida_reaseguro_m, @anulacion_prima_cedida_reaseguro)";
            con.Execute(query, partes_produccion_totales);
            con.Close();
            if (redondeo)
                partes_produccion_totales = con.Query<val_partes_produccion_totales>("SELECT * FROM val_partes_produccion_totales where entidad = @entidad AND fecha = @fecha ",
                                                                                        new { entidad = codigoEntidad, fecha = fechaCorte }).FirstOrDefault();
            con.Close();
            return partes_produccion_totales;
        }

        private dynamic obtenerPartesSiniestrosTotales(decimal cambio_2, decimal cambio_4, decimal cambio_5, bool redondeo = false)
        {
            dynamic partesSiniestrosTotales = con.Query<dynamic>(@"SELECT SUM(CASE  moneda WHEN '1' THEN Coalesce(sins_liquidados_m,0)
                                                                                        WHEN '2' THEN Coalesce(sins_liquidados_m,0)*@cambio_2
                                                                                        WHEN '4' THEN Coalesce(sins_liquidados_m,0)*@cambio_4
                                                                                        WHEN '5' THEN Coalesce(sins_liquidados_m,0)*@cambio_5
                                                                                        END) AS sins_liquidados_m,
                                                                        SUM(Coalesce(sins_liquidados,0)*1) AS sins_liquidados,
                                                                        SUM(CASE  moneda WHEN '1' THEN Coalesce(sins_reaseguro_aceptado_m,0)
                                                                                        WHEN '2' THEN Coalesce(sins_reaseguro_aceptado_m,0)*@cambio_2
                                                                                        WHEN '4' THEN Coalesce(sins_reaseguro_aceptado_m,0)*@cambio_4
                                                                                        WHEN '5' THEN Coalesce(sins_reaseguro_aceptado_m,0)*@cambio_5
                                                                                        END) AS sins_reaseguro_aceptado_m,
                                                                        SUM(Coalesce(sins_reaseguro_aceptado,0)*1) AS sins_reaseguro_aceptado,
                                                                        SUM(CASE  moneda WHEN '1' THEN Coalesce(sins_reaseguro_cedido_m,0)
                                                                                        WHEN '2' THEN Coalesce(sins_reaseguro_cedido_m,0)*@cambio_2
                                                                                        WHEN '4' THEN Coalesce(sins_reaseguro_cedido_m,0)*@cambio_4
                                                                                        WHEN '5' THEN Coalesce(sins_reaseguro_cedido_m,0)*@cambio_5
                                                                                        END) AS sins_reaseguro_cedido_m,
                                                                        SUM(Coalesce(sins_reaseguro_cedido,0)*1) AS sins_reaseguro_cedido
                                                                                FROM val_partes_siniestros
                                                                                WHERE entidad=@entidad AND fecha=@fecha AND moneda<>'0' ",
                                                                                new { entidad = this.codigoEntidad, fecha = this.fechaCorte, cambio_2 = this.cambio_2, cambio_4 = this.cambio_4, cambio_5 = this.cambio_5 }).FirstOrDefault();
            con.Close();
            // se realiza el redondeo como lo haria la BD al insertar
            partesSiniestrosTotales.sins_liquidados_m = Math.Round(partesSiniestrosTotales.sins_liquidados_m, 2, MidpointRounding.AwayFromZero);
            partesSiniestrosTotales.sins_liquidados = Math.Round(partesSiniestrosTotales.sins_liquidados, 2, MidpointRounding.AwayFromZero);
            partesSiniestrosTotales.sins_reaseguro_aceptado_m = Math.Round(partesSiniestrosTotales.sins_reaseguro_aceptado_m, 2, MidpointRounding.AwayFromZero);
            partesSiniestrosTotales.sins_reaseguro_aceptado = Math.Round(partesSiniestrosTotales.sins_reaseguro_aceptado, 2, MidpointRounding.AwayFromZero);
            partesSiniestrosTotales.sins_reaseguro_cedido_m = Math.Round(partesSiniestrosTotales.sins_reaseguro_cedido_m, 2, MidpointRounding.AwayFromZero);
            partesSiniestrosTotales.sins_reaseguro_cedido = Math.Round(partesSiniestrosTotales.sins_reaseguro_cedido, 2, MidpointRounding.AwayFromZero);
            return partesSiniestrosTotales;

        }

        // 1. Valida Capital asegurado neto en cuenta 70101
        private void capitalAseguadoNeto()
        {
            decimal capAseg_EF = con.Query<decimal>(@"select coalesce( movimiento_haber - movimiento_debe -  saldo_haber_anterior * @factorTC , 0) from val_balance_estado_resultados 
                                                        where entidad = @entidad and fecha=@fecha and cuenta_financiera || cuenta_tecnica =  '70101'  and moneda='0' ",
                                                        new { entidad = this.codigoEntidad, fecha = this.fechaCorte, factorTC = this.factorTC }).FirstOrDefault();
            con.Close();
            dynamic valEF = new ExpandoObject();
            valEF.data = con.Query<dynamic>(@"SELECT entidad, fecha, capital_asegurado_neto as monto_parte, @capAseg_EF AS monto_EF, capital_asegurado_neto-@capAseg_EF AS diferencia 
                                                                    FROM val_partes_produccion_totales WHERE entidad=@entidad AND fecha=@fecha 
                                                                   -- AND (capital_asegurado_neto - @capAseg_EF > @margen OR capital_asegurado_neto - @capAseg_EF < -@margen) ",
                                                                      new { entidad = codigoEntidad, fecha = fechaCorte, capAseg_EF = capAseg_EF, margen = margenEF }).FirstOrDefault();
            con.Close();
            valEF.valido = Math.Abs(this.partes_produccion_totales.capital_asegurado_neto - capAseg_EF) > this.margenEF ? false : true;
            //"201  "El Capital Asegurado Neto en el Parte no corresponde al monto en la Cuenta de Orden 70101
            Error error = err("201", "A o C", "", 0, 0);        //  error genérico para este tipo de validacion
            valEF.validacion = error.nombre_error;
            valEF.estadoValidez = valEF.valido ? 4 : error.estadoValidez;
            valEF.descripcionError = valEF.valido ? "sin error" : error.error + " " + error.desc_error;
            if (!valEF.valido)
                this.errores.Add(error);
            this.listaValEF.Add(valEF);

        }

        // 2. Valida Produccion (Prima directa). Calcula Prima Directa con las cuentas 40101, 40102, 40103
        public void primaDirecta()
        {
            decimal primaDir_EF = con.Query<decimal>(@"select  coalesce(sum(movimiento_haber - movimiento_debe  -  saldo_haber_anterior * @factorTC),0) FROM val_balance_estado_resultados 
                                                        where entidad = @entidad and fecha= @fecha  and cuenta_financiera || cuenta_tecnica in  ('40101','40102','40103')   and moneda='0' ",
                                                        new { entidad = codigoEntidad, fecha = fechaCorte, factorTC = this.factorTC }).FirstOrDefault();
            con.Close();
            dynamic valEF = new ExpandoObject();
            valEF.data = con.Query<dynamic>(@"SELECT entidad, fecha, prima_directa_m AS monto_parte, prima_directa AS monto_parte_bs, @primaDir_EF AS monto_ef, prima_directa_m - @primaDir_EF AS diferencia, prima_directa-@primaDir_EF AS diferencia_con_bs 
                                                            FROM val_partes_produccion_totales WHERE entidad=@entidad AND fecha=@fecha 
                                                             -- AND (prima_directa_m - @primaDir_EF > @margen OR prima_directa_m - @primaDir_EF< -@margen OR prima_directa - @primaDir_EF > @margen or prima_directa-@primaDir_EF < -@margen) ",
                                                        new { entidad = codigoEntidad, fecha = fechaCorte, primaDir_EF = primaDir_EF, margen = margenEF }).FirstOrDefault();
            con.Close();
            valEF.valido = (Math.Abs(partes_produccion_totales.prima_directa_m - primaDir_EF) > margenEF || Math.Abs(partes_produccion_totales.prima_directa - primaDir_EF) > margenEF) ? false : true;

            //"202  "La Producción Directa en el Parte no corresponde al monto reportado en Estados Financieros, cuentas 40101','40102','40103" 
            Error error = err("202", "A o C", "", 0, 0);        //  error genérico para este tipo de validacion
            valEF.validacion = error.nombre_error;
            valEF.estadoValidez = valEF.valido ? 4 : error.estadoValidez;
            valEF.descripcionError = valEF.valido ? "sin error" : error.error + " " + error.desc_error;
            if (!valEF.valido)
                this.errores.Add(error);
            this.listaValEF.Add(valEF);
        }

        // 3. Prima Anulada 
        private void primaAnulada()
        {
            //Calcula Prima Anulada con las cuentas 50101, 50102, 50103
            decimal primaAnul_EF = con.Query<decimal>(@"SELECT coalesce(sum(movimiento_debe - movimiento_haber -  saldo_debe_anterior * @factorTC), 0) FROM  val_balance_estado_resultados 
                                                        WHERE entidad = @entidad AND fecha= @fecha AND cuenta_financiera || cuenta_tecnica IN ('50101','50102','50103') AND moneda='0' ",
                                        new { entidad = codigoEntidad, fecha = fechaCorte, factorTC = this.factorTC }).FirstOrDefault();
            con.Close();
            dynamic valEF = new ExpandoObject();
            valEF.data = con.Query<dynamic>(@"SELECT entidad, fecha, prima_anulada_m AS monto_parte, prima_anulada AS monto_parte_bs, @primaAnul_EF AS monto_ef, prima_anulada_m-@primaAnul_EF AS diferencia, prima_anulada-@primaAnul_EF AS diferencia_con_bs 
                                                            FROM val_partes_produccion_totales WHERE entidad=@entidad AND fecha=@fecha 
                                                             -- AND (prima_anulada_m-@primaAnul_EF> @margen OR prima_anulada_m-@primaAnul_EF<-@margen OR prima_anulada-@primaAnul_EF>@margen OR prima_anulada-@primaAnul_EF<-@margen) ",
                                                        new { entidad = codigoEntidad, fecha = fechaCorte, primaAnul_EF = primaAnul_EF, margen = margenEF }).FirstOrDefault();
            con.Close();
            valEF.valido = (Math.Abs(partes_produccion_totales.prima_anulada_m - primaAnul_EF) > margenEF || Math.Abs(partes_produccion_totales.prima_anulada - primaAnul_EF) > margenEF) ? false : true;

            //"203  "La Producción Anulada en el Parte no corresponde al monto reportado en Estados Financieros 50101','50102','50103" 
            Error error = err("203", "A o C", "", 0, 0);        //  error genérico para este tipo de validacion
            valEF.validacion = error.nombre_error;
            valEF.estadoValidez = valEF.valido ? 4 : error.estadoValidez;
            valEF.descripcionError = valEF.valido ? "sin error" : error.error + " " + error.desc_error;
            if (!valEF.valido)
                this.errores.Add(error);
            this.listaValEF.Add(valEF);
        }

        // 4 Valida Produccion Neta de Anulaciones
        private void primaNeta()
        {
            // Calcula Prima Neta con las cuentas 40101, 40102, 40103, 50101, 50102, 50103
            decimal primaNeta_EF = con.Query<decimal>(@"SELECT c1.suma1 - c2.suma2 FROM 
                                                        (SELECT  coalesce(sum(movimiento_haber  -movimiento_debe -  saldo_haber_anterior * @factorTC),0) AS suma1 FROM val_balance_estado_resultados 
                                                            where entidad = @entidad and fecha= @fecha  and cuenta_financiera || cuenta_tecnica in  ('40101','40102','40103')   and moneda='0') c1, 
                                                        (SELECT coalesce(sum(movimiento_debe - movimiento_haber -  saldo_debe_anterior * @factorTC),0) AS suma2 FROM  val_balance_estado_resultados 
                                                            WHERE entidad = @entidad AND fecha= @fecha AND cuenta_financiera || cuenta_tecnica IN ('50101','50102','50103') AND moneda='0') c2 ",
                                        new { entidad = codigoEntidad, fecha = fechaCorte, factorTC = this.factorTC }).FirstOrDefault();
            con.Close();
            dynamic valEF = new ExpandoObject();
            valEF.data = con.Query<dynamic>(@"SELECT entidad, fecha, prima_neta_m AS monto_parte, prima_neta AS monto_parte_bs, @primaNeta_EF AS monto_ef, prima_neta_m-@primaNeta_EF AS diferencia, prima_neta-@primaNeta_EF AS diferencia_con_bs 
                                                            FROM val_partes_produccion_totales WHERE entidad=@entidad AND fecha=@fecha 
                                                             -- AND (prima_neta_m-@primaNeta_EF > @margen OR prima_neta_m-@primaNeta_EF<-@margen OR prima_neta-@primaNeta_EF>@margen OR prima_neta-@primaNeta_EF<-@margen) ",
                                                       new { entidad = codigoEntidad, fecha = fechaCorte, primaNeta_EF = primaNeta_EF, margen = margenEF }).FirstOrDefault();
            con.Close();
            valEF.valido = (Math.Abs(partes_produccion_totales.prima_neta_m - primaNeta_EF) > margenEF || Math.Abs(partes_produccion_totales.prima_neta - primaNeta_EF) > margenEF) ? false : true;

            //"204  "La Producción Neta en el Parte no corresponde al monto reportado en Estados Financieros, cuentas 40101, 40102, 40103, 50101, 50102, 50103" 
            Error error = err("204", "A o C", "", 0, 0);        //  error genérico para este tipo de validacion
            valEF.validacion = error.nombre_error;
            valEF.estadoValidez = valEF.valido ? 4 : error.estadoValidez;
            valEF.descripcionError = valEF.valido ? "sin error" : error.error + " " + error.desc_error;
            if (!valEF.valido)
                this.errores.Add(error);
            this.listaValEF.Add(valEF);
        }

        // 5 Valida Primas Aceptadas en Reaseguro Nacional y Extranjero
        private void primaAceptadaREA()
        {
            // Calcula Prima Aceptada en Reaseguro con las cuentas 40201, 40202, 40301, 40302, 41801, 42001, 50201, 50202, 50301, 50302
            decimal priAcepRea_EF = con.Query<decimal>(@"SELECT c1.suma1 - c2.suma2 FROM
                                                            (SELECT coalesce( sum(movimiento_haber - movimiento_debe - saldo_haber_anterior * @factorTC), 0) AS suma1 FROM val_balance_estado_resultados
                                                                WHERE entidad = @entidad and fecha=@fecha and cuenta_financiera || cuenta_tecnica in  ('40201', '40202', '40301', '40302', '41801', '42001') AND moneda='0') c1,
                                                            (SELECT coalesce(sum(movimiento_debe - movimiento_haber - saldo_debe_anterior * @factorTC),0) AS suma2 FROM  val_balance_estado_resultados 
                                                                WHERE entidad = @entidad AND fecha=@fecha AND cuenta_financiera || cuenta_tecnica IN ('50201', '50202', '50301', '50302') AND moneda='0') c2",
                                                new { entidad = codigoEntidad, fecha = fechaCorte, factorTC = this.factorTC }).FirstOrDefault();
            con.Close();
            dynamic valEF = new ExpandoObject();
            valEF.data = con.Query<dynamic>(@"SELECT entidad, fecha, prima_aceptada_reaseguro_m AS monto_parte, prima_aceptada_reaseguro AS monto_parte_bs, @priAcepRea_EF AS monto_ef, prima_aceptada_reaseguro_m-@priAcepRea_EF AS diferencia, 
                                                                    prima_aceptada_reaseguro-@priAcepRea_EF AS diferencia_con_bs 
                                                            FROM val_partes_produccion_totales WHERE entidad=@entidad AND fecha=@fecha 
                                                             -- AND (prima_aceptada_reaseguro_m-@priAcepRea_EF > @margen OR prima_aceptada_reaseguro_m-@priAcepRea_EF<-@margen OR prima_aceptada_reaseguro-@priAcepRea_EF>@margen OR prima_aceptada_reaseguro-@priAcepRea_EF<-@margen) ",
                                                       new
                                                       {
                                                           entidad = codigoEntidad,
                                                           fecha = fechaCorte,
                                                           priAcepRea_EF = priAcepRea_EF,
                                                           margen = margenEF
                                                       }).FirstOrDefault();
            con.Close();
            valEF.valido = (Math.Abs(partes_produccion_totales.prima_aceptada_reaseguro_m - priAcepRea_EF) > margenEF || Math.Abs(partes_produccion_totales.prima_aceptada_reaseguro - priAcepRea_EF) > margenEF) ? false : true;

            //"205  "Las Primas Aceptadas en Reaseguro Nacional y Extranjero en el Parte no corresponden al monto reportado en Estados Financieross, cuentas 40201, 40202, 40301, 40302, 41801, 42001, 50201, 50202, 50301, 50302
            Error error = err("205", "A o C", "", 0, 0);        //  error genérico para este tipo de validacion
            valEF.validacion = error.nombre_error;
            valEF.estadoValidez = valEF.valido ? 4 : error.estadoValidez;
            valEF.descripcionError = valEF.valido ? "sin error" : error.error + " " + error.desc_error;
            if (!valEF.valido)
                this.errores.Add(error);
            this.listaValEF.Add(valEF);
        }

        // 6 Valida Primas Cedidas en Reaseguro Nacional y Extranjero
        private void primaCedidaREA()
        {
            // --Calcula Prima Cedida en Reaseguro con las cuentas 50601, 50602, 50701, 50702
            decimal priCedRea_EF = con.Query<decimal>(@"SELECT coalesce( sum(movimiento_debe - movimiento_haber - saldo_debe_anterior * @factorTC), 0) FROM val_balance_estado_resultados
                                                        WHERE entidad = @entidad and fecha=@fecha AND cuenta_financiera || cuenta_tecnica IN  ('50601','50602','50701', '50702') AND moneda='0' ",
                                                                   new { entidad = codigoEntidad, fecha = fechaCorte, factorTC = this.factorTC }).FirstOrDefault();
            con.Close();
            dynamic valEF = new ExpandoObject();
            valEF.data = con.Query<dynamic>(@"SELECT entidad, fecha, prima_cedida_reaseguro_m AS monto_parte, prima_cedida_reaseguro AS monto_parte_bs, @priCedRea_EF AS monto_ef, prima_cedida_reaseguro_m-@priCedRea_EF AS diferencia, 
                                                                    prima_cedida_reaseguro-@priCedRea_EF AS diferencia_con_bs 
                                                            FROM val_partes_produccion_totales WHERE entidad=@entidad AND fecha=@fecha 
                                                             -- AND (prima_cedida_reaseguro_m-@priCedRea_EF > @margen OR prima_cedida_reaseguro_m-@priCedRea_EF<-@margen OR prima_cedida_reaseguro-@priCedRea_EF>@margen OR prima_cedida_reaseguro-@priCedRea_EF<-@margen) ",
                                                       new { entidad = codigoEntidad, fecha = fechaCorte, priCedRea_EF = priCedRea_EF, margen = margenEF }).FirstOrDefault();
            con.Close();
            valEF.valido = (Math.Abs(partes_produccion_totales.prima_cedida_reaseguro_m - priCedRea_EF) > margenEF || Math.Abs(partes_produccion_totales.prima_cedida_reaseguro - priCedRea_EF) > margenEF) ? false : true;

            //"206  "Las Primas Cedidas en Reaseguro Nacional y Extranjero en el Parte no corresponde al monto reportado en Estados Financieros, cuentas50601, 50602, 50701, 50702
            Error error = err("206", "A o C", "", 0, 0);        //  error genérico para este tipo de validacion
            valEF.validacion = error.nombre_error;
            valEF.estadoValidez = valEF.valido ? 4 : error.estadoValidez;
            valEF.descripcionError = valEF.valido ? "sin error" : error.error + " " + error.desc_error;
            if (!valEF.valido)
                this.errores.Add(error);
            this.listaValEF.Add(valEF);
        }

        // 7 Valida Anulacion Primas Cedidas en Reaseguro Nacional y Extranjero
        private void anulacionPrimaCedidaREA()
        {
            // Calcula Anulacion Primas Cedidas en Reaseguro con las cuentas 40601, 40602, 40701, 40702
            decimal anuPriCedRea_EF = con.Query<decimal>(@"SELECT coalesce(sum(movimiento_haber - movimiento_debe  - saldo_haber_anterior * @factorTC), 0) FROM val_balance_estado_resultados
                                                            WHERE entidad = @entidad and fecha=@fecha AND cuenta_financiera || cuenta_tecnica IN ('40601','40602','40701', '40702') AND moneda='0' ",
                                                      new { entidad = codigoEntidad, fecha = fechaCorte, factorTC = this.factorTC }).FirstOrDefault();
            con.Close();
            dynamic valEF = new ExpandoObject();
            valEF.data = con.Query<dynamic>(@"SELECT entidad, fecha, anulacion_prima_cedida_reaseguro_m AS monto_parte, anulacion_prima_cedida_reaseguro AS monto_parte_bs, @anuPriCedRea_EF AS monto_ef, anulacion_prima_cedida_reaseguro_m-@anuPriCedRea_EF AS diferencia, 
                                                                    anulacion_prima_cedida_reaseguro-@anuPriCedRea_EF AS diferencia_con_bs 
                                                            FROM val_partes_produccion_totales WHERE entidad=@entidad AND fecha=@fecha 
                                                             -- AND (anulacion_prima_cedida_reaseguro_m-@anuPriCedRea_EF > @margen OR anulacion_prima_cedida_reaseguro_m-@anuPriCedRea_EF<-@margen 
                                                             -- OR anulacion_prima_cedida_reaseguro-@anuPriCedRea_EF>@margen OR anulacion_prima_cedida_reaseguro-@anuPriCedRea_EF<-@margen) ",
                                                       new { entidad = codigoEntidad, fecha = fechaCorte, anuPriCedRea_EF = anuPriCedRea_EF, margen = margenEF }).FirstOrDefault();
            con.Close();
            valEF.valido = (Math.Abs(partes_produccion_totales.anulacion_prima_cedida_reaseguro_m - anuPriCedRea_EF) > margenEF || Math.Abs(partes_produccion_totales.anulacion_prima_cedida_reaseguro - anuPriCedRea_EF) > margenEF) ? false : true;

            //"207  "La Anulación de Primas Cedidas en Reaseguro Nacional y Extranjero en el Parte no corresponde al monto reportado en Estados Financieros, 40601, 40602, 40701, 40702
            Error error = err("207", "A o C", "", 0, 0);        //  error genérico para este tipo de validacion
            valEF.validacion = error.nombre_error;
            valEF.estadoValidez = valEF.valido ? 4 : error.estadoValidez;
            valEF.descripcionError = valEF.valido ? "sin error" : error.error + " " + error.desc_error;
            if (!valEF.valido)
                this.errores.Add(error);
            this.listaValEF.Add(valEF);
        }
        ///////////////////////////////////////////////////////////////////////// SINIESTROS   ////////////////////////////////////
        // 8 Valida Siniestros y Rentas
        private void siniestrosLiquidados()
        {
            // --Calcula Siniestros Liquidados con las cuentas 51101, 51102, 51103, 41101, 41102, 41103
            decimal sinLiq_EF = con.Query<decimal>(@"SELECT c1.suma1 - c2.suma2 FROM
                                                        (SELECT coalesce(sum(movimiento_debe  -movimiento_haber   - saldo_debe_anterior * @factorTC), 0) AS suma1 FROM val_balance_estado_resultados
                                                            WHERE entidad = @entidad and fecha=@fecha AND cuenta_financiera || cuenta_tecnica IN  ('51101', '51102', '51103') AND moneda='0') c1,
                                                        (SELECT coalesce(sum(movimiento_haber - movimiento_debe   - saldo_haber_anterior * @factorTC), 0) AS suma2 FROM val_balance_estado_resultados
                                                            WHERE entidad = @entidad and fecha=@fecha AND cuenta_financiera || cuenta_tecnica IN   ('41101', '41102', '41103') AND moneda='0') c2",
                                                                    new { entidad = codigoEntidad, fecha = fechaCorte, factorTC = this.factorTC }).FirstOrDefault();
            con.Close();
            dynamic valEF = new ExpandoObject();
            dynamic data = new ExpandoObject();
            data.entidad = codigoEntidad;
            data.fecha = fechaCorte;
            data.monto_parte = partes_sins_totales.sins_liquidados_m;
            data.monto_parte_bs = partes_sins_totales.sins_liquidados;
            data.monto_ef = sinLiq_EF;
            data.diferencia = partes_sins_totales.sins_liquidados_m - sinLiq_EF;
            data.diferencia_con_bs = partes_sins_totales.sins_liquidados - sinLiq_EF;
            valEF.data = data;

            valEF.valido = (Math.Abs((decimal)partes_sins_totales.sins_liquidados_m - sinLiq_EF) > margenEF || Math.Abs((decimal)partes_sins_totales.sins_liquidados - sinLiq_EF) > margenEF) ? false : true;

            //"208  "Los Siniestros y Rentas en el Parte no corresponden al monto reportado en Estados Financieros, cuentas 51101, 51102, 51103, 41101, 41102, 41103
            Error error = err("208", "A o C", "", 0, 0);        //  error genérico para este tipo de validacion
            valEF.validacion = error.nombre_error;
            valEF.estadoValidez = valEF.valido ? 4 : error.estadoValidez;
            valEF.descripcionError = valEF.valido ? "sin error" : error.error + " " + error.desc_error;
            if (!valEF.valido)
                this.errores.Add(error);
            this.listaValEF.Add(valEF);
        }

        // 9 Valida Participacion de Siniestros Aceptados en Reaseguro Nacional y Extranjero
        private void sinsPagadosREAAceptado()
        {
            // --Calcula Siniestros Pagados Reaseguro con las cuentas 51701, 51702, 51703, 51901, 51902, 51903
            decimal sinPagReaAcep_EF = con.Query<decimal>(@"SELECT coalesce(sum(movimiento_debe - movimiento_haber - saldo_debe_anterior * @factorTC), 0) FROM val_balance_estado_resultados
                                                        WHERE entidad = @entidad and fecha=@fecha AND cuenta_financiera || cuenta_tecnica IN  ('51701', '51702', '51703', '51901', '51902', '51903') AND moneda='0' ",
                                                            new { entidad = codigoEntidad, fecha = fechaCorte, factorTC = this.factorTC }).FirstOrDefault();
            con.Close();
            dynamic valEF = new ExpandoObject();

            dynamic data = new ExpandoObject();
            data.entidad = codigoEntidad;
            data.fecha = fechaCorte;
            data.monto_parte = partes_sins_totales.sins_reaseguro_aceptado_m;
            data.monto_parte_bs = partes_sins_totales.sins_reaseguro_aceptado;
            data.monto_ef = sinPagReaAcep_EF;
            data.diferencia = partes_sins_totales.sins_reaseguro_aceptado_m - sinPagReaAcep_EF;
            data.diferencia_con_bs = partes_sins_totales.sins_reaseguro_aceptado - sinPagReaAcep_EF;

            valEF.data = data;
            valEF.valido = (Math.Abs((decimal)partes_sins_totales.sins_reaseguro_aceptado_m - sinPagReaAcep_EF) > margenEF || Math.Abs((decimal)partes_sins_totales.sins_reaseguro_aceptado - sinPagReaAcep_EF) > margenEF) ? false : true;

            //"209  "La Participación de Siniestros Aceptados en Reaseguro Nacional y Extranjero en el Parte no corresponden al monto reportado en Estados Financieros, cuentas 51701, 51702, 51703, 51901, 51902, 51903
            Error error = err("209", "A o C", "", 0, 0);        //  error genérico para este tipo de validacion
            valEF.validacion = error.nombre_error;
            valEF.estadoValidez = valEF.valido ? 4 : error.estadoValidez;
            valEF.descripcionError = valEF.valido ? "sin error" : error.error + " " + error.desc_error;
            if (!valEF.valido)
                this.errores.Add(error);
            this.listaValEF.Add(valEF);

        }
        // 10 Valida Siniestros Reembolsados por Cesiones en Reaseguro Nacional y Extranjero
        private void sinsReembolsadoREAAcpetado()
        {
            // Calcula Siniestros Liquidados con las cuentas 51101, 51102, 51103, 41101, 41102, 41103
            decimal sinReemReaCed_EF = con.Query<decimal>(@"SELECT coalesce(sum(movimiento_haber - movimiento_debe  - saldo_haber_anterior * @factorTC),0) FROM val_balance_estado_resultados 
                                                           WHERE entidad = @entidad and fecha=@fecha AND 
                                                            cuenta_financiera || cuenta_tecnica IN ('41301','41302','41303','41304','41305','41306','41501','41502','41503','41504','41505','41506') AND moneda='0' ",
                                                            new { entidad = codigoEntidad, fecha = fechaCorte, factorTC = this.factorTC }).FirstOrDefault();
            con.Close();
            dynamic valEF = new ExpandoObject();

            dynamic data = new ExpandoObject();
            data.entidad = codigoEntidad;
            data.fecha = fechaCorte;
            data.monto_parte = partes_sins_totales.sins_reaseguro_cedido_m;
            data.monto_parte_bs = partes_sins_totales.sins_reaseguro_cedido;
            data.monto_ef = sinReemReaCed_EF;
            data.diferencia = partes_sins_totales.sins_reaseguro_cedido_m - sinReemReaCed_EF;
            data.diferencia_con_bs = partes_sins_totales.sins_reaseguro_cedido - sinReemReaCed_EF;

            valEF.data = data;
            valEF.valido = (Math.Abs((decimal)partes_sins_totales.sins_reaseguro_cedido_m - sinReemReaCed_EF) > margenEF || Math.Abs((decimal)partes_sins_totales.sins_reaseguro_cedido - sinReemReaCed_EF) > margenEF) ? false : true;

            //"210  "Los Siniestros Reembolsados por Cesiones en Reaseguro Nacional y Extranjero en el Parte no corresponden al monto reportado en Estados Financieros, cuentas 51101, 51102, 51103, 41101, 41102, 41103
            Error error = err("201", "A o C", "", 0, 0);        //  error genérico para este tipo de validacion
            valEF.validacion = error.nombre_error;
            valEF.estadoValidez = valEF.valido ? 4 : error.estadoValidez;
            valEF.descripcionError = valEF.valido ? "sin error" : error.error + " " + error.desc_error;
            if (!valEF.valido)
                this.errores.Add(error);
            this.listaValEF.Add(valEF);
        }

        /////////////////////////////// Validacion de Partes de PRODUCCION Y SINIESTROS  con Estados Financieros ///////////////////////////////////////////
        private int validaPartesEF()
        {
            //////////// Validacion PRODUCICON EF //////////////////////////////////
            this.partes_produccion_totales = this.obtenerPartesProduccioTotales(this.cambio_2, this.cambio_4, this.cambio_5, true);

            // 1. Valida Capital asegurado neto
            if (this.archivosEntidadCadena.Contains("A") && this.archivosEntidadCadena.Contains("CD"))
                this.capitalAseguadoNeto();

            // 2. Valida Produccion (Prima directa)
            if (this.archivosEntidadCadena.Contains("A") && this.archivosEntidadCadena.Contains("CD"))
                this.primaDirecta();

            // 3. Valida Anulacion Produccion (prima anulada)
            if (this.archivosEntidadCadena.Contains("A") && this.archivosEntidadCadena.Contains("CD"))
                this.primaAnulada();

            // 4. Valida Produccion Neta de Anulaciones - (Prima neta)
            //Calcula Prima Neta con las cuentas 40101, 40102, 40103, 50101, 50102, 50103
            if (this.archivosEntidadCadena.Contains("A") && this.archivosEntidadCadena.Contains("CD"))
                this.primaNeta();

            // 5. Valida Primas Aceptadas en Reaseguro Nacional y Extranjero
            if (this.archivosEntidadCadena.Contains("A") && this.archivosEntidadCadena.Contains("CD"))
                this.primaAceptadaREA();

            // 6. Valida Primas Cedidas en Reaseguro Nacional y Extranjero
            if (this.archivosEntidadCadena.Contains("A") && this.archivosEntidadCadena.Contains("CD"))
                this.primaCedidaREA();

            // 7. Valida Anulacion Primas Cedidas en Reaseguro Nacional y Extranjero
            if (this.archivosEntidadCadena.Contains("A") && this.archivosEntidadCadena.Contains("CD"))
                this.anulacionPrimaCedidaREA();

            /////////////////////////////// Validacion  Partes de SINIESTROS  ////////////////////////////////////////

            this.partes_sins_totales = obtenerPartesSiniestrosTotales(cambio_2, cambio_4, cambio_5, false);

            // 8. Valida Siniestros y Rentas  (siniestros liquidados)
            //Calcula Siniestros Liquidados con las cuentas 51101, 51102, 51103, 41101, 41102, 41103
            if (this.archivosEntidadCadena.Contains("B") && this.archivosEntidadCadena.Contains("CD"))
                this.siniestrosLiquidados();

            // 9. Valida Participacion de Siniestros Aceptados en Reaseguro Nacional y Extranjero (siniestros Pagados Reaseguro Aceptado)
            //Calcula Siniestros Pagados Reaseguro con las cuentas 51701, 51702, 51703, 51901, 51902, 51903
            if (this.archivosEntidadCadena.Contains("B") && this.archivosEntidadCadena.Contains("CD"))
                this.sinsPagadosREAAceptado();

            // 10. Valida Siniestros Reembolsados por Cesiones en Reaseguro Nacional y Extranjero (siniestros reembolsados reaseguro cedido )
            //Calcula Siniestros Pagados Reaseguro con las cuentas 51701, 51702, 51703, 51901, 51902, 51903
            if (this.archivosEntidadCadena.Contains("B") && this.archivosEntidadCadena.Contains("CD"))
                this.sinsReembolsadoREAAcpetado();

            int estadoValidezEF = 4;
            foreach (dynamic item in this.listaValEF)
            {
                // item.tipo_parte = "produccion";
                if (item.estadoValidez < estadoValidezEF)
                    estadoValidezEF = item.estadoValidez;
                // this.listaValEF.Add(item);
            }
            // foreach (dynamic item in this.validacionesSinEF)
            // {
            //     item.tipo_parte = "siniestros";
            //     if (!item.valido)
            //         estadoValidez = 2;
            //     this.listaValEF.Add(item);
            // }

            return estadoValidezEF;
        }
    }


}