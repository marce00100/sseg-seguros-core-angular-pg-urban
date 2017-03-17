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
    public class ModalidadesController : Controller
    {
        private IDbConnection con = BdPG.instancia();

        [HttpGet()]
        public object todos()
        {
            IEnumerable<object> lista = con.Query<object>(@"SELECT *  FROM ""iftModalidad""
                                                                   ORDER BY ""cModalidad"" ");
            con.Close();
            return new
            {
                status = "success",
                mensaje = "encontrados",
                codigo = "200",
                numero = lista.Count(),
                data = lista
            };
        }

        [HttpGet("{id}")]
        public object objetoId(string id)
        {
            object item = con.Query<object>(@"SELECT * FROM ""iftModalidad"" WHERE ""cModalidad""  = @id", new { id = id }).FirstOrDefault();
            con.Close();
            string status, mensaje, codigo;
            if (item == null)
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
                data = item
            };
        }

        [HttpPost()]
        public object insertar([FromBody]Modalidad objeto)
        {
            dynamic enCod = con.Query<dynamic>(@"SELECT  * FROM ""iftModalidad"" WHERE ""cModalidad"" = @codigo", new { codigo = (string)objeto.cModalidad }).FirstOrDefault();
            con.Close();
            string status, mensaje, codigo;
            object data;
            if (enCod == null)
            {
                objeto.creado_en = DateTime.Now;
                objeto.creado_por = 999;
                con.Execute(@"INSERT INTO ""iftModalidad""(
                                        ""cModalidad"", ""tDescripcion"", 
                                        creado_por, creado_en)
                                        VALUES ( @cModalidad, @tDescripcion,  
                                            @creado_por, @creado_en)", objeto);
                //  new
                //  {
                //      cModalidad = (string)objeto.cModalidad,
                //      tDescripcion = (string)objeto.tDescripcion,
                //      creado_por = (int)objeto.creado_por,
                //      creado_en = (DateTime)objeto.creado_en
                //  });
                con.Close();

                status = "created";
                mensaje = "creado";
                codigo = "201";
                data = objeto;
            }
            else
            {
                status = "not created";
                mensaje = "codigo duplicado";
                codigo = "202";
                data = enCod;
            }
            return new
            {
                status = status,
                mensaje = mensaje,
                codigo = codigo,
                data = data
            };
        }


        [HttpPut("{id}")]
        public object actualizar([FromBody]Modalidad elem, int id)
        {
            elem.modificado_en = DateTime.Now;
            elem.modificado_por = 999;
            con.Execute(@"UPDATE ""iftModalidad"" SET 
                                ""tDescripcion"" = @tDescripcion,
                                modificado_por = @modificado_por, modificado_en =  @modificado_en
                                WHERE ""cModalidad"" = @cModalidad", elem);

            con.Close();
            return new
            {
                status = "success",
                mensaje = "actualizado",
                codigo = "200",
                data = elem
            };
        }
    }
}