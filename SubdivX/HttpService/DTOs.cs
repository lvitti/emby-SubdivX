// Minimal DTOs for FlareSolverr
public sealed class FlareSolverrResponse
{
    public string status { get; set; }
    public string message { get; set; }
    public FlareSolverrSolution solution { get; set; }
}

public sealed class FlareSolverrSolution
{
    public string url { get; set; }
    public int status { get; set; }
    public FlareSolverrCookie[] cookies { get; set; }
    public string userAgent { get; set; }
    public System.Collections.Generic.Dictionary<string, string> headers { get; set; }
    public string response { get; set; } // important! comes with <html><body>…</body></html>
}

public sealed class FlareSolverrCookie
{
    public string domain { get; set; }
    public bool httpOnly { get; set; }
    public string name { get; set; }
    public string path { get; set; }
    public string sameSite { get; set; }
    public bool secure { get; set; }
    public string value { get; set; }
}

// For gt.php inner JSON
public sealed class SubdivxTokenEnvelope
{
    public string token { get; set; }
}

// For ajax.php (you can model only what's used)
public sealed class SubdivxAjaxResponse
{
    public string sEcho { get; set; }
    public string estrategia { get; set; }
    public int iTotalRecords { get; set; }
    public int iTotalDisplayRecords { get; set; }
    public System.Collections.Generic.List<SubdivxItem> aaData { get; set; }
}

public sealed class SubdivxItem
{
    public int id { get; set; }
    public string titulo { get; set; }
    public string descripcion { get; set; }
    public int cds { get; set; }
    public int? descargas { get; set; }
    public string nick { get; set; }
    public string formato { get; set; }
    public string fecha_subida { get; set; }
    public string promedio { get; set; }
}
