using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace BanCoreBot.Infrastructure.QnAMakerAI
{
    public class QnAMakerAIService : ActivityHandler , IQnAMakerAIService
    {
        private readonly IBotTelemetryClient _telemetryClient;

        public QnAMaker _qnAMakerResult { get; set; }

        public QnAMakerAIService(IConfiguration configuration, ILogger<QnAMakerAIService> logger, IHttpClientFactory httpClientFactory, IBotTelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
            _qnAMakerResult = new QnAMaker(new QnAMakerEndpoint
            {
                KnowledgeBaseId = configuration["QnAMakerBaseId"],
                EndpointKey = configuration["QnAMakerKey"],
                Host = configuration["QnAMakerHostName"]

            },
            null,
            httpClientFactory.CreateClient(),
            _telemetryClient);
        }
    }


}
