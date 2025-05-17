using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using effectServiceAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace auctionServiceAPI.Pages.Effects
{
    public class AllEffectsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AllEffectsModel> _logger;

        public AllEffectsModel(IHttpClientFactory httpClientFactory, ILogger<AllEffectsModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = "InStock";

        public List<Effect> Effects { get; set; } = new List<Effect>();
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string statusFilter = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    StatusFilter = statusFilter;
                }

                var client = _httpClientFactory.CreateClient("gateway");
                string apiPath = string.IsNullOrEmpty(StatusFilter) || StatusFilter == "All" 
                    ? "effect" 
                    : $"effect/status/{StatusFilter}";

                _logger.LogInformation("Requesting effects with URL: {Path}", apiPath);
                var response = await client.GetAsync(apiPath);

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Fejl ved hentning af effekter: {response.ReasonPhrase}";
                    _logger.LogWarning("Failed to fetch effects from API: {StatusCode} {Reason}",
                        response.StatusCode, response.ReasonPhrase);
                    return Page();
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API Response content received");

                Effects = await response.Content.ReadFromJsonAsync<List<Effect>>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                });

                if (Effects == null)
                {
                    Effects = new List<Effect>();
                }

                _logger.LogInformation("Found {Count} effects with status {Status}", 
                    Effects.Count, StatusFilter);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af effekter");
                ErrorMessage = "Der opstod en fejl ved indlæsning af siden. Prøv igen senere.";
                return Page();
            }
        }
    }
}