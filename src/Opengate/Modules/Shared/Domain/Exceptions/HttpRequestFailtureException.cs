namespace Opengate.Modules.Shared.Domain.Exceptions;

public class HttpRequestFailtureException(
    string msg,
    string? requestBody,
    string? responseBody,
    string path,
    int statusCode
    ) : Exception(msg)
{
    public string? RequestBody { get; } = requestBody;
    public string Path { get; } = path;
    public int StatusCode { get; } = statusCode;
    public string? ResponseBody { get; } = responseBody;
}

public static class HttpRequestFailtureExceptionExtension
{
    public static async Task EnsureSuccess(this HttpResponseMessage response, string? msg = null)
    => (await response.EnsureSuccessFin(msg)).ThrowIfFail();

    public static async Task<T> EnsureSuccess<T>(this HttpResponseMessage response, string? msg = null)
    => (await response.EnsureSuccessFin<T>(msg)).ThrowIfFail();

    public static async Task<T> EnsureSuccess<T>(this Task<HttpResponseMessage> response, string? msg = null)
    => await EnsureSuccess<T>(await response, msg);

    public static async Task<Fin<Unit>> EnsureSuccessFin(this HttpResponseMessage response, string? msg = null)
    {
        try
        {
            if (response.IsSuccessStatusCode)
            {
                return new Unit();
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            var requestBodyTask = response.RequestMessage?.Content?.ReadAsStringAsync();

            var requetBody = requestBodyTask is null ? null : await requestBodyTask;

            var path = response.RequestMessage?.RequestUri?.ToString() ?? "";
            var statusCode = (int)response.StatusCode;

            var ex = new HttpRequestFailtureException(
                msg ?? $"Request to {path} failed with status code {statusCode}",
                requestBody: requetBody,
                responseBody: responseBody,
                path: path,
                statusCode: statusCode
            );

            return (Error)ex;
        }
        catch (Exception e)
        {
            return (Error)e;
        }
    }

    public static async Task<Fin<T>> EnsureSuccessFin<T>(this Task<HttpResponseMessage> response, string? msg = null)
    => await EnsureSuccessFin<T>(await response, msg);

    /// <summary>
    /// Ensures that the response is a success. And parse the result
    /// </summary>
    public static async Task<Fin<T>> EnsureSuccessFin<T>(this HttpResponseMessage response, string? msg = null)
    {
        try
        {
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine(new { responseBody });

            if (response.IsSuccessStatusCode)
            {
                var item = await response.Content.ReadFromJsonAsync<T>();
                return item!;
            }


            var requestBodyTask = response.RequestMessage?.Content?.ReadAsStringAsync();

            var requetBody = requestBodyTask is null ? null : await requestBodyTask;

            var path = response.RequestMessage?.RequestUri?.ToString() ?? "";
            var statusCode = (int)response.StatusCode;

            var ex = new HttpRequestFailtureException(
                msg ?? $"Request to {path} failed with status code {statusCode}",
                requestBody: requetBody,
                responseBody: responseBody,
                path: path,
                statusCode: statusCode
            );

            return (Error)ex;
        }
        catch (Exception e)
        {
            return (Error)e;
        }
    }
}