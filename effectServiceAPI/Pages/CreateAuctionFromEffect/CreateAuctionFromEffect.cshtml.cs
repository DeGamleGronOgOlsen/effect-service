using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using auctionServiceAPI.Model;
using auctionServiceAPI.Model.auctionServiceAPI.Model;
using effectServiceAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace auctionServiceAPI.Pages.Effects
{
    public class CreateAuctionFromEffectModel : PageModel
    {
        private readonly ILogger<CreateAuctionFromEffectModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public Guid EffectId { get; set; }

        [BindProperty]
        public Auction AuctionToCreate { get; set; } = new Auction();

        public Effect? Effect { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;
        
       

        public CreateAuctionFromEffectModel(ILogger<CreateAuctionFromEffectModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync(Guid effectId)
        {
            if (effectId == Guid.Empty)
            {
                ErrorMessage = "Du skal vælge en effekt for at oprette en auktion.";
                return Page();
            }

            EffectId = effectId;

            try
            {
                // Hent effekt detaljer fra effect service
                var client = _httpClientFactory.CreateClient("gateway");
                var response = await client.GetAsync($"effect/{effectId}");

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var content = await response.Content.ReadAsStringAsync();
                    Effect = JsonSerializer.Deserialize<Effect>(content, options);

                    if (Effect == null)
                    {
                        ErrorMessage = "Kunne ikke finde effekten.";
                        return Page();
                    }

                    // Pre-fill auction details from effect
                    AuctionToCreate = new Auction
                    {
                        AuctionId = Guid.NewGuid(),
                        EffectId = Effect.EffectId,
                        AuctionTitle = Effect.Title,
                        Description = Effect.Description,
                        MinimumPrice = Effect.MinimumPrice,
                        StartingPrice = Effect.MinimumPrice,
                        Image = Effect.Image,
                        AppraisalId = Effect.AppraisalId,
                        UserId = Effect.Seller,
                        StartDate = DateTime.Now.AddDays(1),
                        EndDate = DateTime.Now.AddDays(8),
                        AuctionStatus = AuctionStatus.OnGoing
                    };
                    
                    return Page();
                }
                else
                {
                    ErrorMessage = "Kunne ikke hente effekt detaljer. Prøv igen senere.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af effekt med ID: {EffectId}", effectId);
                ErrorMessage = "Der opstod en fejl. Prøv igen senere.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("gateway");

                // Læs formdata manuelt, hvis det ikke er databundet
                var form = await Request.ReadFormAsync();
                var auctionData = new
                {
                    EffectId = EffectId,
                    StartDate = DateTime.Parse(form["StartDate"]),
                    EndDate = DateTime.Parse(form["EndDate"]),
                    AuctionStatus = AuctionStatus.OnGoing
                };

                var json = JsonSerializer.Serialize(auctionData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var auctionResponse = await client.PostAsync("auction", content);
                if (!auctionResponse.IsSuccessStatusCode)
                {
                    var errorContent = await auctionResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Fejl ved oprettelse af auktion: {Error}", errorContent);
                    ErrorMessage = "Kunne ikke oprette auktionen.";
                    return Page();
                }

                // Overfør effekt til auktionstilstand
                var transferResponse = await client.PostAsync($"effect/{EffectId}/transfer-to-auction", null);
                if (!transferResponse.IsSuccessStatusCode)
                {
                    var errorContent = await transferResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Fejl ved opdatering af effektstatus: {Error}", errorContent);
                    ErrorMessage = "Auktionen blev oprettet, men status kunne ikke opdateres.";
                    return Page();
                }

                return RedirectToPage("/auction/all");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved oprettelse af auktion.");
                ErrorMessage = "Der opstod en fejl. Prøv igen senere.";
                return Page();
            }
        }


        private async Task ReloadEffectAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("gateway");
                var response = await client.GetAsync($"effect/{EffectId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var content = await response.Content.ReadAsStringAsync();
                    Effect = JsonSerializer.Deserialize<Effect>(content, options);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved genindlæsning af effekt med ID: {EffectId}", EffectId);
            }
        }
    }
}