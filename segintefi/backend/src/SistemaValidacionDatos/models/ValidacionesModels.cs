using System;
using System.Collections.Generic;
// using System.Linq;

namespace SVD.Models
{
    public class ArchivoCargado
    {
        public string nombre { get; set; }
        public string texto { get; set; }  // texto del archivo enviado
        public string validacionTexto { get; set; }  // la validacion de la tabla archivos 
        public string codigo { get; set; }  // codigo del archiv A,B,C,D,E,F,G,H
        public dynamic validacionObj { get; set; }  // la validacion de la tabla archivos convertida a objeto JSON {}
        public List<string> textoFilas { get; set; }  // lista de las string filas 
        public List<List<string>> textoMatriz { get; set; } // Lista de la lista de las columnas en una fila  todos son string
        public List<dynamic> objetosFila { get; set; } // lista de las filas, cada fila es un pbjeto con sus propiedades segun la tabla y tipo de archivo
    }




}