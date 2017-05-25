var util = {
    /**
     * function contiene(array, valor): comprueba   si una cadena o array contiene un valor, 
     * dicho de otro modo si un valor se encuantra en un array o cadena
     * @param {array o string} es el contenedor puede ser array o cadena
     * @param {string} es el valor que se quiere buscar si existe
     * @return {bool} si existe true 
     */
    contiene: function(array, valor)
    {
        if (array)
            return array.indexOf(valor) >= 0 ? true : false;
        else
            return false
    },
    /**
     * comprueba si una variable es cadena vacia o  variable nula
     * @return {bool} 
     */
    vacia: function(variable)
    {
        return (variable == null || variable.trim() == '') ? true : false;
    },
    /**
     * Verifica si un objeto o un valor se encuentra en una lista de objetos, comparando por sus ids o por el parametro, 
     * devuelve el objeto encontrado en la lista y su indice, si es -1, no se encontró
     * @param {type} obj es el obj o id que se desea buscar (se comparan por el id )
     * @param {type} lista es el Array o lista de objs
     * @param {type} palametro es el key de cada objeto del Array que se comparara 
     * @returns {util.encontrarElemento}  retorna un objeto compuesto por {item , indice} ,
     */
    encontrarElemento: function(obj, lista, parametro)
    {
        indice = -1;
        if (typeof (obj) === 'object')
        {
            id = Object.keys(obj)[0];
            for (iii = 0; iii < lista.length; iii++)
            {
                if (obj[id] == lista[iii][id])
                {
                    indice = iii;
                    break;
                }
            }
        }
        else
        {
            for (iii = 0; iii < lista.length; iii++)
            {
                if (obj == lista[iii][parametro])
                {
                    indice = iii;
                    break;
                }
            }
        }
        var objetoRetorno = {}
        objetoRetorno.item = lista[iii];
        objetoRetorno.indice = iii;
        return objetoRetorno
    },
    setFecha: function(vFecha)
    {
        var arr = vFecha.split('/');
        return arr[1] + '/' + arr[0] + '/' + arr[2];
    },
    /**
     * Devuelve la fecha en un formato string
     * @param {type} vFecha 
     * @param {type} op  por defecto 'dd/mm/yyyy' tambien podria ser 'yyyy-mm-dd'
     * @returns {String} 
     */
    getFecha: function(fecha, op)
    {
        if (fecha == null)
            return '';
        var arr = [], yyyy, MM, dd;
        if (fecha.getMonth)
        {
            yyyy = fecha.getFullYear();
            MM = fecha.getMonth() + 1;
            dd = fecha.getDate();
        }
        else
        {
            arr = fecha.toString().split('-');
            yyyy = arr[0];
            MM = arr[1];
            dd = arr[2].substring(0, 2);
        }


        var res = '';
        if (op == 'dd/MM/yyyy' || op == null)
            res = dd + '/' + MM + '/' + yyyy;
        if (op == 'yyyy-MM-dd')
            res = yyyy + '-' + MM + '-' + dd;
        if (op == 'yyyyMM')
            res = yyyy.toString() + MM.toString();
        if (op == 'MM')
            res = MM.toString();
        if (op == 'yyyy')
            res = yyyy.toString();
        if (op == 'dd')
            res = dd.toString();


        return res;
    },
    /**
     * Obtiene el ultimo dia habil de un mes de un año
     * @param {type} $anio 
     * @param {type} $mes
     * @returns {unresolved} 
     */
    getUltimoDiaHabilMesAnterior: function(fechaActual)
    {
        var ultimoDiaMesAnterior = new Date(fechaActual.getFullYear(), fechaActual.getMonth() + 1 - 1, 0);

        var ultimoDiaHabil = ultimoDiaMesAnterior;
        if (ultimoDiaMesAnterior.getDay() == 0) //si es domingo
            ultimoDiaHabil = new Date(fechaActual.getFullYear(), fechaActual.getMonth() + 1 - 1, 0 - 2);
        if (ultimoDiaMesAnterior.getDay() == 6) // si es sabado
            ultimoDiaHabil = new Date(fechaActual.getFullYear(), fechaActual.getMonth() + 1 - 1, 0 - 1);
        return ultimoDiaHabil;
    },
    getUltimoDiaMesAnterior: function(fechaActual)
    {
        var ultimoDiaMesAnterior = new Date(fechaActual.getFullYear(), fechaActual.getMonth() + 1 - 1, 0);

//        var ultimoDiaHabil = ultimoDiaMesAnterior;
//        if (ultimoDiaMesAnterior.getDay() == 0) //si es domingo
//            ultimoDiaHabil = new Date(fechaActual.getFullYear(), fechaActual.getMonth() + 1 - 1, 0 - 2);
//        if (ultimoDiaMesAnterior.getDay() == 6) // si es sabado
//            ultimoDiaHabil = new Date(fechaActual.getFullYear(), fechaActual.getMonth() + 1 - 1, 0 - 1);
        return ultimoDiaMesAnterior;
    },
    /**
     * Leer un archivo de tipo file mediante input type=file  
     * @param {type} file   el file seleccionado  
     * @returns {undefined}   modifica el file agregandole una propiedad texto 
     * donde esta todo el contenido en texto del archivo
     */
    leerArchivo: function(file) {
        var reader = new FileReader();
        reader.onload = function(e)
        {
            file.texto = e.target.result;
            return file;
        };
        reader.readAsText(file, "UTF-8");
    },
    pdfDesdeHtml: function(html, nombrePdf)
    {

        var pdf = new jsPDF('l', 'pt', 'letter');

        // source can be HTML-formatted string, or a reference
        // to an actual DOM element from which the text will be scraped.
        var source = html;

        // we support special element handlers. Register them with jQuery-style 
        // ID selector for either ID or node name. ("#iAmID", "div", "span" etc.)
        // There is no support for any other type of selectors 
        // (class, of compound) at this time.
        var specialElementHandlers = {
            // element with id of "bypass" - jQuery style selector
            '#bypassme': function(element, renderer) {
                // true = "handled elsewhere, bypass text extraction"
                return true
            }
        };

        margins = {
            top: 30,
            bottom: 30,
            left: 40,
            width: 545
        };
        // all coords and widths are in jsPDF instance's declared units
        // 'inches' in this case

//        pdf.setFontSize(6);
        pdf.fromHTML(
            source // HTML string or DOM elem ref.
            , margins.left // x coord
            , margins.top // y coord
            , {
                'width': margins.width // max width of content on PDF
                , 'elementHandlers': specialElementHandlers
            },
            function(dispose) {
                // dispose: object with X, Y of the last line add to the PDF 
                //          this allow the insertion of new lines after html
                pdf.save(nombrePdf);
            },
            margins
            );
    }
}




