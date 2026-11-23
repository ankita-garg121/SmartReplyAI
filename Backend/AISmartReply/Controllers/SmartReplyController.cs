using AISmartReply.Models;
using AISmartReply.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AISmartReply.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("SmartReplyPolicy")]
    public class SmartReplyController : ControllerBase
    {
        private readonly AiService _aiService;
        public SmartReplyController(AiService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateReply(
            SmartReplyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CustomerMessage))
            {
                return BadRequest("Customer message is required.");
            }
            if (request.CustomerMessage.Trim().Length < 3)
            {
                return BadRequest(new
                {
                    message = "Customer message must contain at least 3 characters."
                });
            }
            if (request.CustomerMessage.Length > 1000)
            {
                return BadRequest(new
                {
                    message = "Customer message cannot exceed 1000 characters."
                });
            }
            var allowedTones = new[] { "Professional", "Friendly", "Polite" };

            if (!allowedTones.Contains(request.Tone, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Invalid tone selected." });
            }
            var allowedLengths = new[] { "Short", "Medium", "Detailed" };

            if (!allowedLengths.Contains(request.ReplyLength, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Invalid reply length selected." });
            }
            try
            {
                var reply = await _aiService.GenerateReplyAsync(
                    request.CustomerMessage,
                    request.Tone,
                    request.ReplyLength);

                return Ok(new { reply });
            }
            catch (TaskCanceledException)
            {
                return StatusCode(504, new
                {
                    message = "Ollama request timed out. Please try again."
                });
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, new
                {
                    message = "AI service is currently unavailable. Please try again later."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "Something went wrong. Please try again."
                });
            }
        }
    }
}
