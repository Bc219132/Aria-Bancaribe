using BanCoreBot.Data;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace BanCoreBot.Dialogs.Cancel
{
    public class CancelDialog : ComponentDialog
    {
        private const string CancelMsgText = "Has decidido salir de la operación actual, ¿Existe algo más que pueda hacer por ti?";

        public CancelDialog(string id)
            : base(id)
        {
        }

        
        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }
        
        private async Task<DialogTurnResult> InterruptAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message)
            {
                var text = innerDc.Context.Activity.Text.ToLowerInvariant();

                switch (text)
                {
                    case "cancelar":
                    case "ir atras":
                    case "atras":
                    case "retroceder":
                    case "volver":
                    case "adios":
                    case "iniciar otra vez":
                    case "salir":
                        var cancelMessage = MessageFactory.Text(CancelMsgText, CancelMsgText, InputHints.IgnoringInput);
                        await innerDc.Context.SendActivityAsync(cancelMessage, cancellationToken);
                        ClientData.request = "";
                        return await innerDc.CancelAllDialogsAsync(cancellationToken);
                }
            }

            return null;
        }
    }
}

