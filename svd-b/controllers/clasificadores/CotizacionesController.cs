using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using SVD.Models;
using System;

namespace SVD.Controllers
{
    [Route("svd/api/[Controller]")]
    public class CotizacionesController : Controller
    {
        private IDbConnection con = BdPG.instancia();

        [HttpGet("ultimas")]
        public object ultimas()
        {
            IEnumerable<object> cotizacionesList = con.Query<object>(@"SELECT DISTINCT ""fTipoCambio"", ""mCompra"", ""mVenta"",
                                                                        ""mDEG"", ""mEuroBS"", ""mEuroDA"", ""mUFV"" , fecha_6, fecha_8                                                                         
                                                                        FROM ""cmtTipoCambio""
                                                                        WHERE not eliminado
                                                                        order by  ""fTipoCambio"" desc limit 12");
            con.Close();
            return new
            {
                status = "success",
                mensaje = "encontrados",
                codigo = "200",
                numero = cotizacionesList.Count(),
                data = cotizacionesList
            };
        }

        [HttpGet("ultima")]
        public object ultima()
        {
            object cotizacionesList = con.Query<object>(@"SELECT *  FROM ""cmtTipoCambio""
                                                            WHERE not eliminado
                                                            order by  ""fTipoCambio"" desc limit 1").FirstOrDefault();
            con.Close();
            return new
            {
                status = "success",
                mensaje = "encontrados",
                codigo = "200",
                data = cotizacionesList
            };
        }

        [HttpGet("{fecha}")]
        public dynamic cotizacionPorFecha(DateTime fecha)
        {
            dynamic cotizacionItem = this.cotizacionFecha(fecha); // con.Query<object>(@"SELECT * FROM ""cmtTipoCambio"" WHERE ""fTipoCambio""  = @fecha ", new { fecha = fecha }).FirstOrDefault();
            con.Close();
            string status, mensaje, codigo;
            if (cotizacionItem == null)
            {
                status = "no content";
                mensaje = "no encontrado";
                codigo = "204";
            }
            else
            {
                status = "ok";
                mensaje = "encontrado";
                codigo = "200";
            }

            return new
            {
                status = status,
                mensaje = mensaje,
                codigo = codigo,
                data = cotizacionItem
            };
        }

        public dynamic cotizacionFecha(DateTime fecha)
        {
            dynamic cotizacionItem = con.Query<dynamic>(@"SELECT * FROM ""cmtTipoCambio"" WHERE ""fTipoCambio""  = @fecha ", new { fecha = fecha }).FirstOrDefault();
            con.Close();            
            return cotizacionItem;            
        }



        [HttpPost()]
        public object insertar([FromBody]Cotizacion cotizacion)
        {
            cotizacion.eliminado = false;
            cotizacion.creado_en = DateTime.Now;
            cotizacion.creado_por = 999;

            con.Execute(@"INSERT INTO ""cmtTipoCambio""  (
                    ""fTipoCambio"", ""mCompra"", ""mVenta"", ""mDEG"", ""mEuroBS"", ""mEuroDA"", 
                    ""mUFV"", fecha_6, fecha_8, 
                    eliminado, creado_por, creado_en, 
                    modificado_por, modificado_en)
                    VALUES ( @fTipoCambio, @mCompra, @mVenta, @mDEG, @mEuroBS, 
                    @mEuroDA, @mUFV,  @fecha_6,  @fecha_8, @eliminado,
                    @creado_por, @creado_en, @modificado_por, @modificado_en) ", cotizacion);
            con.Close();
            return new
            {
                status = "created",
                mensaje = "insertado",
                codigo = "201",
                data = cotizacion
            };
        }

        [HttpPut("{id}")]
        public object actualizar([FromBody]Cotizacion cotizacion, int id)
        {
            cotizacion.modificado_en = DateTime.Now;
            cotizacion.modificado_por = 999;
            con.Execute(@"UPDATE ""cmtTipoCambio"" SET 
                                   ""mCompra"" = @mCompra , ""mVenta"" = @mVenta, 
                                    ""mDEG"" =  @mDEG, ""mEuroBS"" = @mEuroBS, ""mEuroDA"" = @mEuroDA, ""mUFV"" = @mUFV,
                                    --fecha_6 = @fecha_6, fecha_8 = @fecha_8,
                                    eliminado = @eliminado, modificado_por = @modificado_por, modificado_en =  @modificado_en
                                    WHERE  ""fTipoCambio"" = @fTipoCambio ", cotizacion);
            con.Close();
            return new
            {
                status = "success",
                mensaje = "actualizado",
                codigo = "200",
                data = cotizacion
            };
        }

        // [HttpDelete("{id}")]
        // public object eliminar(int id)
        // {
        //     Cotizacion cotizacionOld = con.Query<Cotizacion>("SELECT * FROM cotizaciones WHERE id_cotizacion = @id_cotizacion and vigente = true", new { id_cotizacion = id }).FirstOrDefault();
        //     cotizacionOld.vigente = false;
        //     funcionUpdate(cotizacionOld);

        //     Cotizacion cotizacionEliminado = cotizacionOld;
        //     cotizacionEliminado.eliminado = true;
        //     cotizacionEliminado.vigente = true;
        //     cotizacionEliminado.modificado_en = DateTime.Now;
        //     cotizacionEliminado.modificado_por = 999;

        //     funcionInsert(cotizacionEliminado);
        //     return new
        //     {
        //         status = "success",
        //         mensaje = "eliminado",
        //         codigo = "200",
        //         data = cotizacionEliminado
        //     };
        // }


    }
}