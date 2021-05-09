using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BanCoreBot.Common
{
    public class MainCard
    {
        public static async Task ToShow(DialogContext stepContext, CancellationToken cancellationToken) 
        {
            await stepContext.Context.SendActivityAsync(activity: CreateCarousel() , cancellationToken);
        }
        private static Activity CreateCarousel() 
        {
            var cardPreguntas = new HeroCard
            {
                Title = "Preguntas Frecuentes",
                Text = $"Puedo ayudarte con dudas generales relacionadas con la preapertura de cuentas, activación de TDC y mucho más.",
                Images = new List<CardImage> { new CardImage("https://bancabotstorage.blob.core.windows.net/images/preguntas.png") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title = "Más información", Value = "Más información" , Type = ActionTypes.ImBack  }
                }

            };

            var cardReclamos = new HeroCard
            {
                Title = "Reclamos",
                Text = $"Puedo apoyarte en la generación de 15 distintos tipos de reclamos relacionados con nuestros productos y servicios.",
                Images = new List<CardImage> { new CardImage("https://bancabotstorage.blob.core.windows.net/images/reclamos.png") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title = "Crear reclamo", Value = "Crear reclamo" , Type = ActionTypes.ImBack  }
                }

            };

            var cardContacto = new HeroCard
            {
                Title = "Contacto",
                Text = $"Existen canales alternativos a través de los cuales puedes contactarnos, conocelos con el botón \"Opciones de contacto.\" ",
                Images = new List<CardImage> { new CardImage("https://bancabotstorage.blob.core.windows.net/images/contacto.png") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title = "Opciones de contacto", Value = "Opciones de contacto" , Type = ActionTypes.ImBack  }
                }

            };

            var cardRedes = new HeroCard
            {
                Title = "Síguenos en nuestras redes",
                Text = $"Estamos presentes en las redes de mayor uso, síguenos y mantente informado con nuestro contenido así como otro punto de contacto.",
                Images = new List<CardImage> { new CardImage("https://bancabotstorage.blob.core.windows.net/images/redes.png") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title = "Detalle", Value = "Redes Sociales" , Type = ActionTypes.ImBack  }
                }
            };

            var optionsAttachments = new List<Attachment>()
            {
                cardPreguntas.ToAttachment(),
                cardReclamos.ToAttachment(),
                cardContacto.ToAttachment(),
                cardRedes.ToAttachment()
            };

            var reply = MessageFactory.Attachment(optionsAttachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            return reply as Activity;
        }
    }
}
