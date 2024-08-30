using System;
using System.Collections.Generic;

namespace SubdivX
{
    public class SearchResponse
    {
        public string sEcho { get; set; }
        public int iTotalRecords { get; set; }
        public int iTotalDisplayRecords { get; set; }
        public List<aaDatum> aaData { get; set; }
    }

    public class aaDatum
    {
        public int id { get; set; }
        public string titulo { get; set; }
        public string descripcion { get; set; }
        public int cds { get; set; }
        public int descargas { get; set; }
        public int idmoderador { get; set; }
        public int eliminado { get; set; }
        public string fotos { get; set; }
        public int id_subido_por { get; set; }
        public string framerate { get; set; }
        public int comentarios { get; set; }
        public string formato { get; set; }
        public string fecha_subida { get; set; }
        public string promedio { get; set; }
        public string nick { get; set; }
        public int pais { get; set; }
    }
}