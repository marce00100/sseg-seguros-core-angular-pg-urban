using System.Collections.Generic;
using System.Linq;
// using Microsoft.AspNetCore.Mvc;
// using System.Data;
using Dapper;
using SVD.Models;
using System;
using Newtonsoft.Json;
using System.Dynamic;



namespace SVD.Controllers
{
    public partial class ValidacionesController
    {

        /////////////////////////////  FUNCIONES  ////////.//////////////////////////////////////
        /// Verifica si el codigo de entidad pertenece a la lista de entidades vigentes activas y no eliminadas
        private bool validarEntidad()
        {
            bool existe = true;
            Entidad entidad = con.Query<Entidad>(@"SELECT * FROM ""rstEmpresas"" 
                                                    WHERE ""bHabilitado"" = 'S'  
                                                    AND ""cEmpresa"" = @codigo_entidad ", new { codigo_entidad = this.codigoEntidad }).FirstOrDefault();
            con.Close();
            if (entidad == null)
            {
                // EE-01  "La entidad a la que pertenece el usuario no existe o ha sido inactivada.",
                this.errores.Add(err("001", "", "", 0, 0));
                existe = false;
            }
            return existe;
        }
        /// Construye el nombre del archivo segun el formato
        private string nombreArchivo(string codigoEnt, int mes, int ano, string codigo)
        {
            string prefijo = "S" + codigo;
            string mm = mes.ToString().Length < 2 ? "0" + mes.ToString() : mes.ToString();
            return prefijo + ano.ToString() + mm + "." + codigoEnt;
        }
        /// Verifica si los archivos enviados son los que se deberian enviar segun el mes y el tipo de entidad
        private bool validarArchivosEnviadosNombres(List<ArchivoCargado> archivosEnv)
        {
            bool correcto = true;

            // se obtiene la lita de archivos que corresponden a la compañia segun el mes 
            List<Archivo> archivosValidos = ArchivosController.getArchivosDeEntidadMes(this.codigoEntidad, this.mesControl);
            if (archivosValidos.Count != archivosEnv.Count)
            {
                correcto = false;
                // EE-02  "La cantidad de archivos enviados no corresponde
                this.errores.Add(err("002", "", "deberían ser " + archivosValidos.Count + " archivos y se cargaron:  " + archivosEnv.Count + " .", 0, 0));
            }

            foreach (Archivo arch in archivosValidos)
            {
                string nombreArchivo = this.nombreArchivo(codigoEntidad, mesControl, anoControl, arch.codigo);
                ArchivoCargado carg = (from i in archivosEnv where i.nombre.ToUpper() == nombreArchivo.ToUpper() select i).FirstOrDefault();

                if (carg == null)
                {
                    correcto = false;
                    // EE-03 "Falta el archivo "
                    this.errores.Add(err("003", nombreArchivo, nombreArchivo, 0, 0));
                }
                else
                {
                    carg.codigo = arch.codigo;
                    carg.validacionTexto = arch.validacion;
                }
            }
            return correcto;
        }

        /// transforma los datos del campo validaciones a objeto JSON, para leerlo mas facil, y convierte el contenido del texto en una matriz de : lista de filas de vectores 
        private void transformaDatos(List<ArchivoCargado> archivosEnv)
        {
            foreach (var item in archivosEnv)
            {
                //conviete a objeto JSON el campo validacion
                item.validacionObj = JsonConvert.DeserializeObject(item.validacionTexto);
                // Array bidimensional
                item.textoFilas = new List<string>(item.texto.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries));
                if (item.textoFilas[item.textoFilas.Count - 1].Substring(0, 1) != "\"")
                    item.textoFilas.RemoveAt(item.textoFilas.Count - 1);
                var matriz = new List<List<string>>();

                foreach (var fila in item.textoFilas)
                {
                    List<string> vectorFila = new List<string>(fila.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                    for (int i = 0; i < vectorFila.Count; i++)
                        vectorFila[i] = vectorFila[i].Replace("\"", "").Trim();
                    matriz.Add(vectorFila);
                }
                item.textoMatriz = matriz;
            }
        }

        /// Reliza la validacion del numero de columnas por fila y los tipos de datos contenidos en cada columna, segun la configuracion de l campo JSON validacion
        private string validarFormatoTexto(List<ArchivoCargado> archivosEnv)
        {
            this.departamentos = (List<string>)con.Query<string>(@"SELECT ""tSigla"" FROM ""iftDepartamento"" ");
            this.monedasConIntegrador = (List<string>)con.Query<string>(@"SELECT ""cMoneda"" FROM ""iftTipoMoneda"" ");
            this.monedas = (List<string>)con.Query<string>(@"SELECT ""cMoneda"" FROM ""iftTipoMoneda"" where ""cMoneda"" != '0' ");
            this.modalidades = (List<string>)con.Query<string>(@"SELECT ""cModalidad"" FROM ""iftModalidad"" ");
            this.modalidadesRamos = (List<string>)con.Query<string>(@"SELECT  (""cModalidad"" || ""cRamo"") FROM ""iftRamo"" ");
            this.modalidadesRamosPolizas = (List<string>)con.Query<string>(@"SELECT  (""cModalidad"" || ""cRamo"" || ""cPoliza"") FROM ""iftTipoSeguro"" ");
            con.Close();
            bool noExisteError = true;
            bool noExisteAdvertencia = true;
            foreach (var arch in archivosEnv)//por cada archivo
            {
                int fila = 0;
                int numColumnas = (int)arch.validacionObj.numero_columnas;
                arch.objetosFila = new List<dynamic>();
                foreach (List<string> vectorFila in arch.textoMatriz) //recorre cada fila que viene a ser un vector de sctrings
                {
                    dynamic filaObj = JsonConvert.DeserializeObject("{}"); //  esto para que pueda ser tratado como un objeto JSON puro y se puedan agregar elementos de la manera filaObj["nombreDeCampoNuevo"] = algo
                    if (vectorFila.Count != numColumnas)
                    {
                        noExisteError = false;
                        // EE-04  "El número de columnas no es igual al especificado."
                        this.errores.Add(err("004", arch.nombre, "", (fila + 1), 0));
                    }
                    else
                    {
                        for (int col = 0; col < vectorFila.Count; col++) //recorre cada columna del vector fila
                        {
                            string tipoDato = arch.validacionObj.columnas[col].tipo.ToString();
                            string condicion = arch.validacionObj.columnas[col].condicion.ToString();
                            string nombreColumna = arch.validacionObj.columnas[col].nombre.ToString();
                            //verifica el formato  de cada columna
                            bool campoCorrecto = validaFormatoTextoColumna(tipoDato, nombreColumna, vectorFila[col], filaObj);
                            if (!campoCorrecto)
                            {
                                // EE-05  "El tipo de dato no es correcto. "
                                this.errores.Add(err("005", arch.nombre, "Debería ser " + tipoDato + ", campo: " + nombreColumna + ".", fila + 1, col + 1));
                                noExisteError = false;
                            }
                            else
                            {
                                //verifica el formato la condicion de cada columna
                                dynamic error = validaFormatoContenidoColumna(condicion, nombreColumna, filaObj);
                                if (error.tipoError == "EE")
                                {
                                    this.errores.Add(err(error.codError, arch.nombre, "", fila + 1, col + 1));
                                    noExisteError = false;
                                }
                                else if (error.tipoError == "A")
                                {
                                    this.errores.Add(err(error.codError, arch.nombre, "", fila + 1, col + 1));
                                    noExisteAdvertencia = false;
                                }
                            }
                        }
                    }
                    filaObj["index_fila"] = fila + 1;
                    arch.objetosFila.Add(filaObj);
                    fila++;
                }
            }
            string resp = "V";
            resp = noExisteAdvertencia ? resp : "A";
            resp = noExisteError ? resp : "EE";

            return resp;
        }


        /// Funcion que valida el formato, el tipo de dato  cada campo de los archivos.
        // segun la definicion en la tabla Archivos el campo Validacion JSON 
        private bool validaFormatoTextoColumna(string tipoDato, string nombreColumna, string campo, dynamic filaObj)
        {
            bool campoCorrecto = true;
            switch (tipoDato)
            {
                case "char(1)":
                    campoCorrecto = (campo.Length == 1);
                    filaObj[nombreColumna] = campo.Trim();
                    break;
                case "char(2)":
                    campoCorrecto = (campo.Length == 2);
                    filaObj[nombreColumna] = campo.Trim();
                    break;
                case "char(3)":
                    campoCorrecto = (campo.Length == 3);
                    filaObj[nombreColumna] = campo.Trim();
                    break;
                case "char(4)":
                    campoCorrecto = (campo.Length <= 4);
                    filaObj[nombreColumna] = campo.Trim();
                    break;
                case "char(5)":
                    campoCorrecto = (campo.Length <= 5);
                    filaObj[nombreColumna] = campo.Trim();
                    break;
                case "char(6)":
                    campoCorrecto = (campo.Length <= 6);
                    filaObj[nombreColumna] = campo.Trim();
                    break;
                case "decimal(16,2)":
                    try
                    {
                        filaObj[nombreColumna] = Convert.ToDecimal(campo.Replace('.', ','));
                    }
                    catch
                    {
                        campoCorrecto = false;
                    }
                    break;
                case "integer":
                    try
                    {
                        filaObj[nombreColumna] = Convert.ToInt32(Convert.ToDecimal(campo.Replace('.', ',')));
                    }
                    catch
                    {
                        campoCorrecto = false;
                    }
                    break;
                case "date":
                    try
                    {
                        filaObj[nombreColumna] = Convert.ToDateTime(campo);
                    }
                    catch
                    {
                        campoCorrecto = false;
                    }
                    break;
                default:
                    filaObj[nombreColumna] = campo;
                    break;
            }
            return campoCorrecto;
        }

        /// Funcion para definir condiciones de cada campo
        /// condicion es un string con la condicion descrita en el Json
        /// nombreColumna  Es la columan a ser validada , corresponde al nombre de columna en la base de datos
        /// filaObj Es el objeto que representa a la fila que se esta revizando
        ///
        /// se tienen las siguientes variables ya cargadas en la definicion de la clase
        /// List<string> error = ntiene la lista de los error = i se deben agregar los error = ntrados
        /// string codigoEntidad : contiene el codigo  de la entidad ej. "101","109"
        ///   List<ArchivoCargado> archivosEnv  donde se encuantran la lista de archivos enviados como objetos ArchivoCargado ,
        private dynamic validaFormatoContenidoColumna(string condicion, string nombreColumna, dynamic filaObj)
        {
            // bool tipoError = true;
            string codError = "", tipoError = "";
            switch (condicion)
            {
                case "=codigoEntidad":
                    if (filaObj[nombreColumna] != codigoEntidad)
                    {
                        // "Código de entidad incorrecto. ";
                        codError = "006";
                        tipoError = "EE";
                    }
                    break;
                case "=fechaCierre":
                    if (filaObj[nombreColumna] != fechaCorte.Date)
                    {
                        // "La fecha de cierre no coincide con la ingresada. ";
                        codError = "007";
                        tipoError = "EE";
                    }
                    break;
                case "=P":
                    if (filaObj[nombreColumna].ToString().ToUpper() != "P")
                    {
                        // "Código de Tipo Parte incorrecto, debe ser P. ";
                        codError = "008";
                        tipoError = "EE";
                    }
                    break;
                case "=S":
                    if (filaObj[nombreColumna].ToString().ToUpper() != "S")
                    {
                        // "Código de Tipo Parte incorrecto, debe ser S. ";
                        codError = "009";
                        tipoError = "EE";
                    }
                    break;
                case "departamentos":
                    if (!departamentos.Contains(filaObj[nombreColumna].ToString()))
                    {
                        // "Código de departamento incorrecto. ";
                        codError = "010";
                        tipoError = "EE";
                    }
                    break;
                case "P,E":
                    List<string> sector = new List<string> { "P", "E" };
                    if (!sector.Contains(filaObj[nombreColumna].ToString().ToUpper()))
                    {
                        // "Código de sector incorrecto, debe ser P o E. ";
                        codError = "011";
                        tipoError = "EE";
                    }
                    break;
                case "monedas":
                    //incluye al 0 tambien
                    if (!monedas.Contains(filaObj[nombreColumna].ToString()))
                    {
                        // "Código de moneda incorrecto. ";
                        codError = "012";
                        tipoError = "EE";
                    }
                    break;
                case "monedasConIntegrador":
                    //incluye al 0 tambien
                    if (!monedasConIntegrador.Contains(filaObj[nombreColumna].ToString()))
                    {
                        // "Código de moneda incorrecto. ";
                        codError = "013";
                        tipoError = "EE";
                    }
                    break;

                case "modalidades":
                    if (!modalidades.Contains(filaObj[nombreColumna].ToString()))
                    {
                        // "Código de Modalidad incorrecto. ";
                        codError = "014";
                        tipoError = "EE";
                    }
                    break;
                case "modalidad+ramo":
                    string modalidadRamo = filaObj.modalidad.ToString() + filaObj.ramo.ToString();
                    if (!modalidadesRamos.Contains(modalidadRamo))
                    {
                        // "Código de Modalidad o Ramo incorrecto. ";
                        codError = "015";
                        tipoError = "EE";
                    }
                    break;
                case "modalidad+ramo+poliza":
                    string modalidadRamoPoliza = filaObj.modalidad.ToString() + filaObj.ramo.ToString() + filaObj.poliza.ToString();
                    if (!modalidadesRamosPolizas.Contains(modalidadRamoPoliza))
                    {
                        // "Código de poliza incorrecto. ";
                        codError = "016";
                        tipoError = "EE";
                    }
                    break;
                case ">=0":
                    if (filaObj[nombreColumna] < 0)
                    {
                        // "Campo negativo: " + nombreColumna + ". ";
                        codError = "017";
                        tipoError = "EE";
                    }
                    break;
                case ">=0?":
                    if (filaObj[nombreColumna] < 0)
                    {
                        // "Campo negativo: " + nombreColumna + ". ";
                        codError = "018";
                        tipoError = "A";
                    }
                    break;

            }
            return new
            {
                codError = codError,
                tipoError = tipoError
            };
        }

        private void insertarITF(string tipoArchivo, dynamic reg)  // reg contiene la fila o registor en objeto
        {
            string query = "";
            string regString = JsonConvert.SerializeObject(reg);  // se convierte el objeto JSON en cadena para luego volver a convertirlo en objeto pero transformandolo a  la entidad correspondiente
            if (tipoArchivo == "A")
            {
                val_partes_produccion registro = JsonConvert.DeserializeObject<val_partes_produccion>(regString); // se convierte la cadena JSON en objeto del tipo entidad correspondiente
                query = @"INSERT INTO val_partes_produccion(
                            entidad, tipo_parte, departamento, sector, moneda, fecha, modalidad, 
                            ramo, poliza, capital_asegurado, polizas_nuevas, polizas_renovadas, 
                            prima_directa_m, prima_directa, capital_anulado, polizas_anuladas, 
                            prima_anulada_m, prima_anulada, capital_asegurado_neto, polizas_netas, 
                            prima_neta_m, prima_neta, prima_aceptada_reaseguro_m, prima_aceptada_reaseguro, 
                            prima_cedida_reaseguro_m, prima_cedida_reaseguro, anulacion_prima_cedida_reaseguro_m, 
                            anulacion_prima_cedida_reaseguro, index_fila)
                    VALUES (@entidad, @tipo_parte,  @departamento, @sector, @moneda, @fecha, @modalidad, 
                            @ramo, @poliza, @capital_asegurado, @polizas_nuevas, @polizas_renovadas, 
                            @prima_directa_m, @prima_directa, @capital_anulado, @polizas_anuladas, 
                            @prima_anulada_m, @prima_anulada, @capital_asegurado_neto, @polizas_netas, 
                            @prima_neta_m, @prima_neta, @prima_aceptada_reaseguro_m, @prima_aceptada_reaseguro, 
                            @prima_cedida_reaseguro_m, @prima_cedida_reaseguro, @anulacion_prima_cedida_reaseguro_m, 
                            @anulacion_prima_cedida_reaseguro, @index_fila)";
                con.Execute(query, registro);

            }
            else if (tipoArchivo == "B")
            {
                val_partes_siniestros registro = JsonConvert.DeserializeObject<val_partes_siniestros>(regString);
                query = @"INSERT INTO val_partes_siniestros(
                                entidad, tipo_parte, departamento, sector, moneda, fecha, modalidad, 
                                ramo, poliza, num_sins_denunciados, sins_denunciados, num_sins_liquidados, 
                                sins_liquidados_m, sins_liquidados, sins_reaseguro_aceptado_m, 
                                sins_reaseguro_aceptado, sins_reaseguro_cedido_m, sins_reaseguro_cedido, index_fila)
                        VALUES (@entidad, @tipo_parte, @departamento, @sector, @moneda, @fecha, @modalidad, 
                                @ramo, @poliza, @num_sins_denunciados, @sins_denunciados, @num_sins_liquidados, 
                                @sins_liquidados_m, @sins_liquidados, @sins_reaseguro_aceptado_m, 
                                @sins_reaseguro_aceptado, @sins_reaseguro_cedido_m, @sins_reaseguro_cedido, @index_fila)";
                con.Execute(query, registro);
            }
            else if (tipoArchivo == "C" || tipoArchivo == "D")
            {
                val_balance_estado_resultados registro = JsonConvert.DeserializeObject<val_balance_estado_resultados>(regString);
                query = @"INSERT INTO val_balance_estado_resultados(
                                entidad, fecha, cuenta_financiera, moneda, 
                                cuenta_tecnica, saldo_debe_anterior, saldo_haber_anterior, movimiento_debe, 
                                movimiento_haber, saldo_debe_actual, saldo_haber_actual, index_fila)
                        VALUES (@entidad, @fecha, @cuenta_financiera, @moneda, 
                                @cuenta_tecnica, @saldo_debe_anterior, @saldo_haber_anterior, @movimiento_debe, 
                                @movimiento_haber, @saldo_debe_actual, @saldo_haber_actual, @index_fila)";
                con.Execute(query, registro);
            }
            else if (tipoArchivo == "E")
            {
                val_produccion_corredoras registro = JsonConvert.DeserializeObject<val_produccion_corredoras>(regString);
                query = @"INSERT INTO val_partes_produccion_corredoras(
                                es_corredora, entidad, corredora, moneda, 
                                fecha, modalidad, ramo, poliza, produccion, index_fila)
                        VALUES (@es_corredora, @entidad, @corredora, @moneda, 
                                @fecha, @modalidad, @ramo, @poliza, @produccion, @index_fila)";
                con.Execute(query, registro);
            }
            else if (tipoArchivo == "F")
            {
                val_partes_produccion_ramos registro = JsonConvert.DeserializeObject<val_partes_produccion_ramos>(regString);
                query = @"INSERT INTO val_partes_produccion_ramos(
                               entidad, fecha, modalidad, ramo, primas_directas, anulado_primas_directas, 
                                primas_netas_anulaciones, primas_aceptadas_reaseguro_nacional, 
                                primas_aceptadas_reaseguro_extranjero, total_primas_aceptadas_reaseguro, 
                                total_primas_netas, primas_cedidas_reaseguro_nacional, primas_cedidas_reaseguro_extranjero, 
                                total_primas_cedidas, total_primas_netas_retenidas, index_fila)
                        VALUES ( @entidad, @fecha, @modalidad,  @ramo, @primas_directas, @anulado_primas_directas, 
                                @primas_netas_anulaciones, @primas_aceptadas_reaseguro_nacional, 
                                @primas_aceptadas_reaseguro_extranjero, @total_primas_aceptadas_reaseguro,
                                 @total_primas_netas, @primas_cedidas_reaseguro_nacional, @primas_cedidas_reaseguro_extranjero,
                                  @total_primas_cedidas, @total_primas_netas_retenidas, @index_fila)";
                con.Execute(query, registro);
            }
            else if (tipoArchivo == "G")
            {
                val_partes_siniestros_ramos registro = JsonConvert.DeserializeObject<val_partes_siniestros_ramos>(regString);
                query = @"INSERT INTO val_partes_siniestros_ramos(
                               entidad, fecha, modalidad, ramo, sins_directos, sins_reaseguro_aceptado_nacional, 
                                sins_reaseguro_aceptado_extranjero, total_sins_reaseguro_aceptado, 
                                sins_totales, sins_reembolsados_nacional, sins_reembolsados_extranjero, 
                                total_sins_reembolsados, total_sins_retenidos, index_fila)
                        VALUES ( @entidad, @fecha, @modalidad,  @ramo, @sins_directos, @sins_reaseguro_aceptado_nacional, 
                                @sins_reaseguro_aceptado_extranjero, @total_sins_reaseguro_aceptado, 
                                @sins_totales, @sins_reembolsados_nacional, @sins_reembolsados_extranjero,
                                 @total_sins_reembolsados, @total_sins_retenidos,  @index_fila)";
                con.Execute(query, registro);
            }
            else if (tipoArchivo == "H")
            {
                val_partes_seguros_largo_plazo registro = JsonConvert.DeserializeObject<val_partes_seguros_largo_plazo>(regString);
                query = @"INSERT INTO val_partes_seguros_largo_plazo(
                               entidad, fecha, modalidad, ramo, capital_asegurado_total, capital_asegurado_cedido, 
                                capital_asegurado_retenido, reserva_matematica_total, reserva_matematica_reasegurador, 
                                reserva_matematica_retenida, index_fila)
                        VALUES ( @entidad, @fecha, @modalidad,  @ramo, @capital_asegurado_total, @capital_asegurado_cedido, 
                                @capital_asegurado_retenido, @reserva_matematica_total, 
                                @reserva_matematica_reasegurador, @reserva_matematica_retenida,  @index_fila)";
                con.Execute(query, registro);
            }
            con.Close();
        }
        private void eliminarDeTablaVal(string tipoArchivo, string codEntidad, DateTime fecha)
        {
            string tabla = "";
            if (tipoArchivo == "A")
                tabla = "val_partes_produccion";
            else if (tipoArchivo == "B")
                tabla = "val_partes_siniestros";
            else if (tipoArchivo == "C" || tipoArchivo == "D")
                tabla = "val_balance_estado_resultados";
            else if (tipoArchivo == "E")
                tabla = "val_partes_produccion_corredoras";
            else if (tipoArchivo == "F")
                tabla = "val_partes_produccion_ramos";
            else if (tipoArchivo == "G")
                tabla = "val_partes_siniestros_ramos";
            else if (tipoArchivo == "H")
                tabla = "val_partes_seguros_largo_plazo";

            con.Execute("DELETE FROM " + tabla + " where entidad = @entidad AND fecha = @fecha ", new { entidad = codEntidad, fecha = fecha });
            con.Close();
        }
    }


}