using auctionServiceAPI.Services;
using effectServiceAPI.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace auctionServiceAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class EffectController : ControllerBase
    {
        private readonly ILogger<EffectController> _logger;
        private readonly IEffectService _effectService;
        private readonly string _serviceIp;
        private readonly string _imagePath;
        private readonly string _imageUrlPath;

        public EffectController(
            ILogger<EffectController> logger, 
            IEffectService effectService,
            IConfiguration config)
        {
            _logger = logger;
            _effectService = effectService;
            _imagePath = config["EffectImagePath"] ?? "/srv/resources/effect-images";
            
            _imageUrlPath = "/images/effect/";
            // Get and log the service IP address
            var hostName = System.Net.Dns.GetHostName();
            var ips = System.Net.Dns.GetHostAddresses(hostName);
            _serviceIp = ips.First().MapToIPv4().ToString();
            _logger.LogInformation($"Effect Service responding from {_serviceIp}");
        }

        [Authorize(Roles = "admin")]
        [HttpGet("GetAllEffect")]
        public async Task<ActionResult<IEnumerable<Effect>>> GetAllEffects()
        {
            _logger.LogInformation($"Getting all effects from {_serviceIp}");
            var effects = await _effectService.GetAllEffectsAsync();
            return Ok(effects);
        }
        
        [Authorize(Roles = "admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Effect>> GetEffect(Guid id)
        {
            _logger.LogInformation($"Getting effect {id} from {_serviceIp}");
            var effect = await _effectService.GetEffectAsync(id);
            
            if (effect == null)
            {
                return NotFound();
            }
            
            return Ok(effect);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<Effect>> CreateEffect([FromForm] Effect effect, IFormFile? image)
        {
            _logger.LogInformation($"Creating new effect from {_serviceIp}");
            
            try
            {
                if (effect.EffectId == Guid.Empty)
                {
                    effect.EffectId = Guid.NewGuid();
                }
                
                effect.EffectStatus = EffectStatus.InStock;
                
                // Handle image upload if provided
                if (image != null && image.Length > 0)
                {
                    try
                    {
                        string fileName = $"{effect.EffectId}{Path.GetExtension(image.FileName)}";
                        string filePath = Path.Combine(_imagePath, fileName);
                        
                        Directory.CreateDirectory(_imagePath);
                        
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }
                        
                        effect.Image = $"{_imageUrlPath}{fileName}";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading image.");
                        return StatusCode(500, "An error occurred while uploading the image.");
                    }
                }
                
                await _effectService.CreateEffectAsync(effect);
                return CreatedAtAction(nameof(GetEffect), new { id = effect.EffectId }, effect);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating effect.");
                return StatusCode(500, "An error occurred while creating the effect.");
            }
        }
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEffect(Guid id, [FromForm] Effect effect, IFormFile? image)
        {
            _logger.LogInformation($"Updating effect {id} from {_serviceIp}");
            
            if (id != effect.EffectId)
            {
                return BadRequest();
            }
            
            var existingEffect = await _effectService.GetEffectAsync(id);
            if (existingEffect == null)
            {
                return NotFound();
            }
            
            // Handle image upload if provided
            if (image != null && image.Length > 0)
            {
                try
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(existingEffect.Image))
                    {
                        string oldFilePath = Path.Combine(_imagePath, Path.GetFileName(existingEffect.Image));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    
                    string fileName = $"{effect.EffectId}{Path.GetExtension(image.FileName)}";
                    string filePath = Path.Combine(_imagePath, fileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    
                    effect.Image = $"{_imageUrlPath}{fileName}";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating image.");
                    return StatusCode(500, "An error occurred while updating the image.");
                }
            }
            else
            {
                // Keep existing image if no new one provided
                effect.Image = existingEffect.Image;
            }
            
            await _effectService.UpdateEffectAsync(effect);
            return NoContent();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEffect(Guid id)
        {
            _logger.LogInformation($"Deleting effect {id} from {_serviceIp}");
            
            var effect = await _effectService.GetEffectAsync(id);
            if (effect == null)
            {
                return NotFound();
            }
            
            // Delete image if exists
            if (!string.IsNullOrEmpty(effect.Image))
            {
                string filePath = Path.Combine(_imagePath, Path.GetFileName(effect.Image));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            
            await _effectService.DeleteEffectAsync(id);
            return NoContent();
        }
        
        [Authorize(Roles = "admin")]
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<Effect>>> GetEffectsByStatus(EffectStatus status)
        {
            _logger.LogInformation($"Getting effects with status {status} from {_serviceIp}");
            var effects = await _effectService.GetEffectsByStatusAsync(status);
            return Ok(effects);
        }

        [Authorize(Roles = "admin")]
        [HttpGet("seller/{sellerId}")]
        public async Task<ActionResult<IEnumerable<Effect>>> GetEffectsBySeller(Guid sellerId)
        {
            _logger.LogInformation($"Getting effects for seller {sellerId} from {_serviceIp}");
            var effects = await _effectService.GetEffectsBySellerAsync(sellerId);
            return Ok(effects);
        }

        [Authorize(Roles = "admin")]
        [HttpPost("{id}/transfer-to-auction")]
        public async Task<IActionResult> TransferToAuction(Guid id)
        {
            _logger.LogInformation($"Transferring effect {id} to auction from {_serviceIp}");
            
            var effect = await _effectService.GetEffectAsync(id);
            if (effect == null)
            {
                return NotFound();
            }
            
            if (effect.EffectStatus != EffectStatus.InStock)
            {
                return BadRequest("Effect must be in stock to transfer to auction");
            }
            
            var success = await _effectService.TransferToAuctionAsync(id);
            if (!success)
            {
                return StatusCode(500, "Failed to transfer effect to auction");
            }
            
            return Ok("Effect successfully transferred to auction");
        }

        [Authorize(Roles = "admin")]
        [HttpPost("{id}/mark-as-sold")]
        public async Task<IActionResult> MarkAsSold(Guid id, [FromBody] SoldEffectDto soldDto)
        {
            _logger.LogInformation($"Marking effect {id} as sold from {_serviceIp}");
            
            var effect = await _effectService.GetEffectAsync(id);
            if (effect == null)
            {
                return NotFound();
            }
            
            if (effect.EffectStatus != EffectStatus.OnAuction)
            {
                return BadRequest("Effect must be on auction to be marked as sold");
            }
            
            var success = await _effectService.MarkAsSoldAsync(id, soldDto.BuyerId, soldDto.SoldFor);
            if (!success)
            {
                return StatusCode(500, "Failed to mark effect as sold");
            }
            
            return Ok("Effect successfully marked as sold");
        }
    }

    public class SoldEffectDto
    {
        public Guid BuyerId { get; set; }
        public decimal SoldFor { get; set; }
    }
}