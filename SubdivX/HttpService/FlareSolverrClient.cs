using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Serialization;

public sealed class FlareSolverrClientNetStd
{
    private readonly HttpClient _http;
    private readonly IJsonSerializer _json;
    public string BaseUrl { get; }
    public string Session { get; }

    public FlareSolverrClientNetStd(string baseUrl, string session, IJsonSerializer json, HttpClient http = null)
    {
        BaseUrl = baseUrl != null ? baseUrl.TrimEnd('/') : throw new ArgumentNullException(nameof(baseUrl));
        Session = session ?? throw new ArgumentNullException(nameof(session));
        _json = json ?? throw new ArgumentNullException(nameof(json));
        _http = http ?? new HttpClient();
        _http.Timeout = TimeSpan.FromSeconds(120);
    }

    public Task CreateSessionAsync(CancellationToken ct) =>
        PostJsonAsync<object>(new { cmd = "sessions.create", session = Session }, ct);

    public Task DestroySessionAsync(CancellationToken ct) =>
        PostJsonAsync<object>(new { cmd = "sessions.destroy", session = Session }, ct);

    public async Task<FlareSolverrSolution> RequestGetAsync(string url, object headers, int maxTimeoutMs, CancellationToken ct)
    {
        var payload = new
        {
            cmd = "request.get",
            session = Session,
            url = url,
            maxTimeout = maxTimeoutMs,
            headers = headers
        };
        var root = await PostJsonAsync<FlareSolverrResponse>(payload, ct).ConfigureAwait(false);
        EnsureOk(root);
        return root.solution;
    }

    public async Task<FlareSolverrSolution> RequestPostAsync(string url, string postData, object headers, int maxTimeoutMs, CancellationToken ct)
    {
        var payload = new
        {
            cmd = "request.post",
            session = Session,
            url = url,
            maxTimeout = maxTimeoutMs,
            postData = postData,
            headers = headers
        };
        var root = await PostJsonAsync<FlareSolverrResponse>(payload, ct).ConfigureAwait(false);
        EnsureOk(root);
        return root.solution;
    }

    private async Task<T> PostJsonAsync<T>(object payload, CancellationToken ct)
    {
        var json = _json.SerializeToString(payload);
        using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
        using (var resp = await _http.PostAsync(BaseUrl, content, ct).ConfigureAwait(false))
        {
            resp.EnsureSuccessStatusCode();
            var text = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            return _json.DeserializeFromString<T>(text);
        }
    }

    private static void EnsureOk(FlareSolverrResponse root)
    {
        if (root == null) throw new Exception("Null response from FlareSolverr.");
        if (!string.Equals(root.status, "ok", StringComparison.OrdinalIgnoreCase))
            throw new Exception("FlareSolverr status != ok: " + (root.message ?? "(no message)"));
        if (root.solution == null)
            throw new Exception("FlareSolverr: null solution.");
    }
}
