using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Jellyfin.Extensions.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JellyfinNotify.Plugin
{
    [ApiController]
    [Route("[controller]")]
    [Produces(
    MediaTypeNames.Application.Json,
    JsonDefaults.CamelCaseMediaType,
    JsonDefaults.PascalCaseMediaType)]
    public class PluginApi : ControllerBase
    {
        private readonly ILogger<PluginApi> _logger;

        public PluginApi(ILogger<PluginApi> logger)
        {
            _logger = logger;
        }

        [HttpGet("Unsubscribe")]
        public ActionResult Unsubscribe(string userGuidString, string seriesGuidString)
        {
            if (Guid.TryParse(userGuidString, out var userGuid) && Guid.TryParse(seriesGuidString, out var seriesGuid))
            {
                if (Plugin.Instance == null)
                {
                    _logger.LogCritical("Instance of Jellyfin-Notify Plugin was not found. Inspect debug logs to identify source.");
                    return NotFound();
                }
                else
                {
                    if (Data.UserConfiguration.UnsubscribeUserFromSeries(userGuid, seriesGuid))
                    {
                        return Ok("Successfully Processed Unsubscribe Request.");
                    }
                    else
                    {
                        return Ok("Encountered an Error when Processing Subscribe Request. Please contact the administrator.");
                    }
                }
            }

            _logger.LogCritical("Invalid request for Jellyfin-Notify Plugin was recieved. Inspect network traffic, this should not happen.");
            return NotFound();
        }

        [HttpGet("Subscribe")]
        public ActionResult Subscribe(string userGuidString, string seriesGuidString)
        {
            if (Guid.TryParse(userGuidString, out var userGuid) && Guid.TryParse(seriesGuidString, out var seriesGuid))
            {
                if (Plugin.Instance == null)
                {
                    _logger.LogCritical("Instance of Jellyfin-Notify Plugin was not found. Inspect debug logs to identify source.");
                    return NotFound();
                }
                else
                {
                    if (Data.UserConfiguration.SubscribeUserToSeries(userGuid, seriesGuid))
                    {
                        return Ok("Successfully Processed Subscribe Request.");
                    }
                    else
                    {
                        return Ok("Encountered an Error when Processing Subscribe Request. Please contact the administrator.");
                    }
                }
            }

            _logger.LogCritical("Invalid request for Jellyfin-Notify Plugin was recieved. Inspect network traffic, this should not happen.");
            return NotFound();
        }
    }
}
