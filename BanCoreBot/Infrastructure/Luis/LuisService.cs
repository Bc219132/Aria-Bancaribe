using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;

namespace BanCoreBot.Infrastructure.Luis
{
    public class LuisService : ILuisService
    {
        public LuisRecognizer _luisRecognizer { get; set; }

        public LuisService(IConfiguration configuration, IBotTelemetryClient telemetryClient)
        {
            var luisApplication = new LuisApplication(
                configuration["LuisAppId"],
                configuration["LuisApiKey"],
                configuration["LuisHostName"]
            );

            var recognizerOptions = new LuisRecognizerOptionsV3(luisApplication) 
            {
                TelemetryClient = telemetryClient,
                PredictionOptions = new Microsoft.Bot.Builder.AI.LuisV3.LuisPredictionOptions() 
                {
                    IncludeInstanceData = true,
                    Log = true
                }
            };
            _luisRecognizer = new LuisRecognizer(recognizerOptions); 
        }
    }
}
