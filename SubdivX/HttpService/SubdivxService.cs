using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Serialization;

public sealed class SubdivxServiceNetStd
{
    private const string HOME = "https://subdivx.com/";
    private const string GT = "https://subdivx.com/inc/gt.php?gt=1";
    private const string AJAX = "https://subdivx.com/inc/ajax.php";

    private static readonly Regex BodyRegex =
        new Regex(@"<body[^>]*>(?<inner>.*)</body>", RegexOptions.Singleline | RegexOptions.IgnoreCase);

    private readonly FlareSolverrClientNetStd _fs;
    private readonly IJsonSerializer _json;
    
    private string _searchField;
    
    private static readonly Regex VsRegex =
        new Regex(@"id\s*=\s*[""']vs[""']\s*>\s*v(?<maj>\d+)\.(?<min>\d+)\s*<",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
    
    public SubdivxServiceNetStd(FlareSolverrClientNetStd fs, IJsonSerializer json)
    {
        _fs = fs ?? throw new ArgumentNullException(nameof(fs));
        _json = json ?? throw new ArgumentNullException(nameof(json));
    }

    public async Task<string> WarmupAsync(CancellationToken ct)
    {
        var sol = await _fs.RequestGetAsync(HOME, null, 60000, ct).ConfigureAwait(false);
        var html = sol.response ?? string.Empty;

        var m = VsRegex.Match(html);
        if (!m.Success)
            throw new Exception("Could not find #vs (e.g., v4.12) on HOME.");

        var maj = m.Groups["maj"].Value; // "4"
        var min = m.Groups["min"].Value; // "12"
        _searchField = "buscar" + maj + min; // "buscar412"

        return _searchField;
    }
    
    public async Task<string> GetTokenAsync(CancellationToken ct)
    {
        var sol = await _fs.RequestGetAsync(GT, new
        {
            Referer = HOME,
            X_Requested_With = "XMLHttpRequest",
            Accept = "*/*"
        }, 60000, ct).ConfigureAwait(false);

        var html = sol.response ?? string.Empty;
        var inner = ExtractInnerJson(html);
        var env = _json.DeserializeFromString<SubdivxTokenEnvelope>(inner);
        if (env == null || string.IsNullOrEmpty(env.token))
            throw new Exception("Failed to obtain token from gt.php");
        return env.token;
    }

    public async Task<SubdivxAjaxResponse> AjaxAsync(string query, string token, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(_searchField))
            throw new InvalidOperationException("Warmup not executed: cached 'buscarXYZ' field missing.");
        
        var form = new StringBuilder()
            .Append("tabla=resultados")
            .Append("&filtros=")
            .Append("&").Append(_searchField).Append("=")
            .Append(Uri.EscapeDataString(query))
            .Append("&token=").Append(token) // token is hex; sent raw
            .ToString();

        var sol = await _fs.RequestPostAsync(AJAX, form, new Dictionary<string,string>
        {
            { "X-Requested-With", "XMLHttpRequest" },
            { "Content-Type", "application/x-www-form-urlencoded; charset=UTF-8" },
            { "Accept", "application/json, text/javascript, */*; q=0.01" },
            { "Referer", "https://subdivx.com/" }
        }, 60000, ct).ConfigureAwait(false);

        var html = sol.response ?? string.Empty;
        var innerJson = ExtractInnerJson(html);
        if (string.IsNullOrWhiteSpace(innerJson)) throw new Exception("Empty response.");
        var obj = _json.DeserializeFromString<SubdivxAjaxResponse>(innerJson);
        if (obj == null) throw new Exception("Failed to deserialize ajax.php response");
        return obj;
    }

    public async Task<SubdivxAjaxResponse> AjaxWithRecoveryAsync(string query, int maxAttempts, CancellationToken ct)
    {
        if (maxAttempts < 1) maxAttempts = 1;
        Exception last = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                try { await _fs.DestroySessionAsync(ct).ConfigureAwait(false); } catch { /* ignore */ }
                await _fs.CreateSessionAsync(ct).ConfigureAwait(false);
                await WarmupAsync(ct).ConfigureAwait(false);

                var token = await GetTokenAsync(ct).ConfigureAwait(false);
                var result = await AjaxAsync(query, token, ct).ConfigureAwait(false);

                if (result.aaData == null)
                    throw new Exception("ajax.php missing aaData");
                return result;
            }
            catch (Exception ex)
            {
                last = ex;
                try { await _fs.DestroySessionAsync(ct).ConfigureAwait(false); } catch { /* ignore */ }
                if (attempt == maxAttempts) break;
                await Task.Delay(800, ct).ConfigureAwait(false);
            }
        }

        throw new Exception("AjaxWithRecoveryAsync failed", last);
    }

    private static string ExtractInnerJson(string html)
    {
        var m = BodyRegex.Match(html ?? string.Empty);
        if (!m.Success) throw new Exception("Failed to extract content inside <body>…</body>.");
        var inner = (m.Groups["inner"].Value ?? string.Empty).Trim();
        if (inner.Length >= 2 && inner[0] == '"' && inner[inner.Length - 1] == '"')
            inner = inner.Substring(1, inner.Length - 2);
        return inner;
    }
}
