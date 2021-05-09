using Microsoft.Bot.Builder.AI.Luis;

namespace BanCoreBot.Infrastructure.Luis
{
    public interface ILuisService
    {
        LuisRecognizer _luisRecognizer { get; set; }
    }
}
