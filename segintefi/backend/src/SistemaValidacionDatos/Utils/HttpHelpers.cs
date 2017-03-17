namespace SVD.Utils
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;
    using System;
    using System.Linq;

    public class HttpHelpers
    {
        /// <summary>
        /// Obtiene el valor de la cabecera Authorization, retorna sólo la cadena con el token en formato Json Web Token (JWT) sin el tipo de autorización. Retorna "" en caso de que no exista la cabecera.
        /// </summary>
        /// <param name="httpContext">Objeto de la clase HttpContext</param>
        /// <returns></returns>
        public static string GetTokenFromHeader(HttpContext httpContext)
        {
            string header = null;
            StringValues headerValues = new StringValues();
            if (httpContext.Request.Headers.TryGetValue("Authorization", out headerValues))
                header = headerValues.First();

            if (header == null)
                return "";

            var arrHeader = header.Split(' ');
            if (arrHeader.Length != 2)
                return "";

            if (arrHeader[1] == "null" || arrHeader[1] == "undefined")
                return "";

            return arrHeader[1];
        }
    }
}
