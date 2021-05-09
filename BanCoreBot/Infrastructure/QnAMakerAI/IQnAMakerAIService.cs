using Microsoft.Bot.Builder.AI.QnA;

namespace BanCoreBot.Infrastructure.QnAMakerAI
{
    public interface IQnAMakerAIService
    {
        QnAMaker _qnAMakerResult { get; set; }
    }
}
