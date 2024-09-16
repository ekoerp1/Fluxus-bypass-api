using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}

[ApiController]
[Route("[controller]")]
public class XtzclwarController : ControllerBase
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private static readonly Regex _regex = new Regex(@"let content = \(""([^""]+)""\);", RegexOptions.Compiled);

    [HttpGet("/fluxus")]
    public async Task<IActionResult> GetFluxusKey()
    {
        var url = Request.Query["url"].ToString();
        if (!string.IsNullOrEmpty(url) && url.StartsWith("https://flux.li/android/external/start.php?HWID="))
        {
            try
            {
                var hwid = url.Split("HWID=")[1];
                var startTime = DateTime.Now;
                var requests = new[]
                {
                    new { url = $"https://flux.li/android/external/start.php?HWID={hwid}", referer = "" },
                    new { url = "https://flux.li/android/external/check1.php?hash={hash}", referer = "https://linkvertise.com" },
                    new { url = "https://flux.li/android/external/main.php?hash={hash}", referer = "https://linkvertise.com" }
                };

                foreach (var request in requests)
                {
                    var httpRequest = new HttpRequestMessage(HttpMethod.Get, request.url);
                    httpRequest.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                    httpRequest.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                    httpRequest.Headers.Add("Connection", "close");
                    httpRequest.Headers.Add("Referer", request.referer);
                    httpRequest.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");

                    var response = await _httpClient.SendAsync(httpRequest);
                    var content = await response.Content.ReadAsStringAsync();

                    if (request == requests[^1])
                    {
                        var match = _regex.Match(content);
                        if (match.Success)
                        {
                            var endTime = DateTime.Now;
                            var timeTaken = (endTime - startTime).TotalSeconds;
                            return Ok(new { key = match.Groups[1].Value, time_taken = timeTaken });
                        }
                        else
                        {
                            return StatusCode(500, "Failed to find content key");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to bypass link. Error: {ex.Message}");
            }
        }
        else
        {
            return Ok(new { message = "Please Enter Fluxus Link!" });
        }

        return BadRequest("Invalid Request");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
