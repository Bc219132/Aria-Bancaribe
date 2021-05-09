using BanCoreBot.Dialogs.Cancel;
using BanCoreBot.Infrastructure.Luis;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;
using BanCoreBot.Common.Models.User;
using BanCoreBot.Common.Utils;
using System.Collections.Generic;
using BanCoreBot.Infrastructure.APIs;
using System.Net.Http;
using BanCoreBot.Common.Models.API;
using BanCoreBot.Dialogs.Tarjetas;
using BanCoreBot.Infrastructure.SendGrid;

namespace BanCoreBot.Dialogs.Reclamos
{
    public class ConsultarReclamoDialog : CancelDialog
    {
        private PrivateConversationState _userState;
        private readonly ISendGridEmailService _sendGridEmailService;
        private ILuisService _luisService;
        private const string _SetCIPass = "SetCIPass";
        private const string _GetCI = "GetCI";
        private const string _GetNroReclamo = "GetNroReclamo";
        private const string _Review = "Review";
        private const string _NroReclamo = "NroReclamo";
        private const string _CallAPIs = "CallAPIs";
        private const string _StartOverAgain = "StartOverAgain";
        private Utils utilitario = new Utils();
        private readonly IHttpClientFactory _httpClientFactory;
        private UserPersonalData ClientData;

        public ConsultarReclamoDialog(ISendGridEmailService sendGridEmailService, ILuisService luisService, PrivateConversationState userState, IHttpClientFactory httpClientFactory) : base(nameof(ConsultarReclamoDialog))
        {
            _userState = userState;
            _luisService = luisService;
            _httpClientFactory = httpClientFactory;
            _sendGridEmailService = sendGridEmailService;
            var waterfallSteps = new WaterfallStep[]
            {
                SetCIPass,
                GetCI,
                NroReclamo,
                GetNroReclamo,
                Review,
                CallAPIs,
                StartOverAgain
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(_SetCIPass, CIPassValidator));
            AddDialog(new TextPrompt(_GetCI, CIValidator));
            AddDialog(new TextPrompt(_NroReclamo, ValidateInputValidator));
            AddDialog(new TextPrompt(_GetNroReclamo));
            AddDialog(new TextPrompt(_Review, ValidateInputValidator));
            AddDialog(new TextPrompt(_CallAPIs, ValidateInputValidator));
            AddDialog(new TextPrompt(_StartOverAgain, ValidateInputValidator));
        }

        #region DialogConsultaReclamos

        



        private async Task<DialogTurnResult> SetCIPass(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
            if (luisResult.TopIntent().intent.ToString().Equals("afirmacion"))
            {
                var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
                ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

                /*if(!String.IsNullOrEmpty(ClientData.request))
                {
                    if(ClientData.request.Equals("QueryClaimFromDocumentID"))
                    {
                        return await stepContext.NextAsync(cancellationToken: cancellationToken);
                    }
                }

                if (string.IsNullOrEmpty(ClientData.typeDoc))
                {*/
                var option = await stepContext.PromptAsync(
                    _SetCIPass,
                    new PromptOptions
                    {
                        Prompt = CreateButtonsCIPass()
                    }, cancellationToken);
                    return option;
                /*}
                else
                {
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                }*/
            }
            else
            {
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> GetCI(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            /*if (!String.IsNullOrEmpty(ClientData.request))
            {
                if (ClientData.request.Equals("QueryClaimFromDocumentID"))
                {
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                }
            }*/
            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;

            //if (string.IsNullOrEmpty(ClientData.typeDoc))
            //{
                if (luisResult.Entities.venezolano != null || stepContext.Context.Activity.Text.ToLower().Equals("v"))
                {
                    ClientData.typeDoc = "V";
                }
                else if (luisResult.Entities.Extranjero != null || stepContext.Context.Activity.Text.ToLower().Equals("e"))
                {
                    ClientData.typeDoc = "E";
                }
                else if (luisResult.Entities.juridico!= null || stepContext.Context.Activity.Text.ToLower().Equals("j"))
                {
                    ClientData.typeDoc = "J";
                }
                else if (luisResult.Entities.rifgobierno != null || stepContext.Context.Activity.Text.ToLower().Equals("g"))
                {
                    ClientData.typeDoc = "G";
                }
                else
                {
                    ClientData.typeDoc = "P";
                }
            //}

            //if (string.IsNullOrEmpty(ClientData.ci))
            //{
                var auxText = "";
                if (luisResult.Entities.venezolano != null || luisResult.Entities.Extranjero != null || stepContext.Context.Activity.Text.ToLower().Equals("e") || stepContext.Context.Activity.Text.ToLower().Equals("v"))
                {
                    auxText = "¿Cuál es tu número de cédula?";
                }
                else if (luisResult.Entities.juridico != null || luisResult.Entities.rifgobierno != null || stepContext.Context.Activity.Text.ToLower().Equals("j") || stepContext.Context.Activity.Text.ToLower().Equals("g"))
                {
                    auxText = "¿Cuál es el número del rif?";
                }
                else
                {
                    auxText = "¿Cuál es tu número de pasaporte?";
                }

                return await stepContext.PromptAsync(
                    _GetCI,
                    new PromptOptions { Prompt = MessageFactory.Text(auxText) },
                    cancellationToken
                    );
            /* 
            }
            else
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            } */
        }

        private async Task<DialogTurnResult> NroReclamo(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

        /*    if (string.IsNullOrEmpty(ClientData.ci))
            { */
                ClientData.ci = stepContext.Context.Activity.Text.Replace(".", "").Replace("-", "").Replace("/", "").Replace(",", "").Replace("_", "").Replace(" ", "").Trim();
         //   }

            if (string.IsNullOrEmpty(ClientData.nroReclamo))
            {
                var option = await stepContext.PromptAsync(
                  _NroReclamo,
                  new PromptOptions
                  {
                      Prompt = CreateButtonsHasNumberClaim(""),
                      RetryPrompt = CreateButtonsHasNumberClaim("Reintento")
                  }, cancellationToken
              );
                return option;
                
            }
            else
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            
        }

        private async Task<DialogTurnResult> GetNroReclamo(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
            if (luisResult.TopIntent().intent.ToString().Equals("afirmacion"))
            {
                return await stepContext.PromptAsync(
                    _GetNroReclamo,
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Indícame el número de Reporte"),
                       },
                    cancellationToken
                    );
            }
            else
            {
                ClientData.nroReclamo = "";
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }
        private async Task<DialogTurnResult> Review(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());
            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;

            if (string.IsNullOrEmpty(ClientData.nroReclamo))
            {
                if (luisResult.Entities.NroReclamo != null)
                {
                    ClientData.nroReclamo = stepContext.Context.Activity.Text;
                }
            }
            return await stepContext.BeginDialogAsync(nameof(ReviewConsultaReclamoDialog), cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> CallAPIs(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            var authApi = new AuthenticationAPI(_httpClientFactory);

            var response = await authApi.getToken(_sendGridEmailService,_httpClientFactory, _userState, stepContext.Context);
            if (response.error ==0)
            {

                var queryClaimApi = new QueryClaimAPI(_httpClientFactory);
                if (!String.IsNullOrEmpty(ClientData.nroReclamo))
                {
                    var responseClaimbyID = await queryClaimApi.getClaimByClaimID(_sendGridEmailService, _httpClientFactory, int.Parse(ClientData.nroReclamo.ToString()), _userState, stepContext.Context);
                    if (responseClaimbyID.error == 0)
                    {
                        ReclamoariadtoDocumentID responseAPIClaimByClaimID = ClientData.objectReclamo;
                        if ((ClientData.typeDoc + ClientData.ci).Equals(responseAPIClaimByClaimID.cedula))
                        {
                            if (!String.IsNullOrEmpty(getStatusFromEnum(responseAPIClaimByClaimID.codEstado.ToString() + responseAPIClaimByClaimID.codEstatus.ToString())))
                            {
                                await stepContext.Context.SendActivityAsync("A continuación el resultado de tu consulta", cancellationToken: cancellationToken);
                                await stepContext.Context.SendActivityAsync(
                                "Reporte: " + responseAPIClaimByClaimID.nroReclamo.ToString() + $"{ Environment.NewLine}" +
                                "Monto: " + responseAPIClaimByClaimID.montoReclamo.ToString() + $"{ Environment.NewLine}" +
                                "Estatus: " + getStatusFromEnum(responseAPIClaimByClaimID.codEstado.ToString() + responseAPIClaimByClaimID.codEstatus.ToString()), cancellationToken: cancellationToken);
                                ClientData.nroReclamo = "";
                                ClientData.request = "QueryClaimFromNumberClaim";
                                ClientData.ClaimFound = true;
                                ClientData.objectReclamo = null;
                                await stepContext.Context.SendActivityAsync($"Un reclamo ya generado, tiene hasta un máximo de 20 días continuos para su resolución, tiempo que empieza a transcurrir una vez se te notifique vía correo electrónico o a través de un mensaje de texto, según lo establece el ente regulador SUDEBAN", cancellationToken: cancellationToken);
                            }
                            else
                            {// Capturar datos enviar al call
                                ClientData.request = "consultareclamo";
                                await stepContext.Context.SendActivityAsync($"No logro visualizar en este momento ningún reporte con tu cédula, pero puedo tomar tu solicitud y enviarla al área encargada", cancellationToken: cancellationToken);
                                return await stepContext.ReplaceDialogAsync(nameof(NaturalJuridicoDialog), stepContext.Values, cancellationToken);
                            }
                        }
                    }
                }

                ClientData.objectReclamo = null;
                
                if (ClientData.ClaimFound)
                {
                    ClientData.ClaimFound = false;
                    var option = await stepContext.PromptAsync(
                            _CallAPIs,
                            new PromptOptions
                            {
                                Prompt = CreateButtonsStartAgain(""),
                                RetryPrompt = CreateButtonsStartAgain("Reintento")
                            }, cancellationToken
                        );
                    return option;
                }

                var responseClaim = await queryClaimApi.getClaimByDocumentID(_sendGridEmailService, _httpClientFactory, ClientData.typeDoc+ClientData.ci,_userState, stepContext.Context);
                if (responseClaim.error==0)
                {
                    if (responseClaim.control.Equals("ok2"))
                    {
                        ReclamoariadtoDocumentID[] responseAPIClaimByClaimID = ClientData.listaReclamos;
                        if (!String.IsNullOrEmpty(ClientData.nroReclamo))
                        {
                            var existClaim = false;

                            foreach (var elem in responseAPIClaimByClaimID)
                            {
                                if (String.Compare(elem.nroReclamo.ToString(), ClientData.nroReclamo, false) == 0)
                                {
                                    existClaim = true;
                                    if (!String.IsNullOrEmpty(getStatusFromEnum(elem.codEstado.ToString() + elem.codEstatus.ToString())))
                                    {
                                        await stepContext.Context.SendActivityAsync("A continuación el resultado de tu consulta", cancellationToken: cancellationToken);
                                        await stepContext.Context.SendActivityAsync(
                                            "Reporte: " + elem.nroReclamo.ToString() + $"{ Environment.NewLine}" +
                                            "Monto: " + elem.montoReclamo.ToString() + $"{ Environment.NewLine}" +
                                            "Estatus: " + getStatusFromEnum(elem.codEstado.ToString() + elem.codEstatus.ToString()), cancellationToken: cancellationToken);
                                        ClientData.nroReclamo = "";
                                        ClientData.listaReclamos = null;
                                        ClientData.request = "QueryClaimFromNumberClaim";
                                        await stepContext.Context.SendActivityAsync($"Un reclamo ya generado, tiene hasta un máximo de 20 días continuos para su resolución, tiempo que empieza a transcurrir una vez se te notifique vía correo electrónico o a través de un mensaje de texto, según lo establece el ente regulador SUDEBAN", cancellationToken: cancellationToken);

                                        var option = await stepContext.PromptAsync(
                                            _CallAPIs,
                                            new PromptOptions
                                            {
                                                Prompt = CreateButtonsStartAgain(""),
                                                RetryPrompt = CreateButtonsStartAgain("Reintento")
                                            }, cancellationToken
                                        );
                                        return option;
                                    }
                                    else
                                    {
                                        existClaim = true;
                                        ClientData.request = "consultareclamo";
                                        await stepContext.Context.SendActivityAsync($"No logro visualizar en este momento ningún reporte con tu cédula, pero puedo tomar tu solicitud y enviarla al área encargada", cancellationToken: cancellationToken);
                                        return await stepContext.ReplaceDialogAsync(nameof(NaturalJuridicoDialog), stepContext.Values, cancellationToken);
                                    }
                                }
                            }
                            if (!existClaim)
                            {
                                int QtyStatusUnknown = 0;

                                foreach (var elem in responseAPIClaimByClaimID)
                                {
                                    if (String.IsNullOrEmpty(getStatusFromEnum(elem.codEstado.ToString() + elem.codEstatus.ToString())))
                                    {
                                        QtyStatusUnknown = QtyStatusUnknown + 1;
                                    }
                                }
                                if (QtyStatusUnknown == responseAPIClaimByClaimID.Length)
                                {
                                    ClientData.request = "consultareclamo";
                                    //await stepContext.Context.SendActivityAsync($"A través del sistema no logré validar ningún reporte asociado a la información que me indicaste. ", cancellationToken: cancellationToken);
                                    await stepContext.Context.SendActivityAsync($"No logro visualizar en este momento ningún reporte con tu cédula, pero puedo tomar tu solicitud y enviarla al área encargada", cancellationToken: cancellationToken);
                                    return await stepContext.ReplaceDialogAsync(nameof(NaturalJuridicoDialog), stepContext.Values, cancellationToken);
                                }

                                if (responseAPIClaimByClaimID.Length < 2 || responseAPIClaimByClaimID.Length - QtyStatusUnknown == 1)
                                {
                                    await stepContext.Context.SendActivityAsync("A través del sistema no logré validar ningún reporte asociado con el número que me indicaste, sin embargo, visualicé este reporte asociado a tus datos ( " + ClientData.typeDoc + ClientData.ci + " )", cancellationToken: cancellationToken);
                                }
                                else
                                {
                                    await stepContext.Context.SendActivityAsync("A través del sistema no logré validar ningún reporte asociado con el número que me indicaste, sin embargo, visualicé estos reportes asociados a tus datos ( " + ClientData.typeDoc + ClientData.ci + " )", cancellationToken: cancellationToken);
                                }
                                foreach (var elem in responseAPIClaimByClaimID)
                                {
                                    if (!String.IsNullOrEmpty(getStatusFromEnum(elem.codEstado.ToString() + elem.codEstatus.ToString())))
                                    {
                                        await stepContext.Context.SendActivityAsync(
                                        "Reporte: " + elem.nroReclamo.ToString() + $"{ Environment.NewLine}" +
                                        "Monto: " + elem.montoReclamo.ToString() + $"{ Environment.NewLine}" +
                                        "Estatus: " + getStatusFromEnum(elem.codEstado.ToString() + elem.codEstatus.ToString()), cancellationToken: cancellationToken);
                                        ClientData.nroReclamo = null;
                                    }
                                }
                                await stepContext.Context.SendActivityAsync($"Un reclamo ya generado, tiene hasta un máximo de 20 días continuos para su resolución, tiempo que empieza a transcurrir una vez se te notifique vía correo electrónico o a través de un mensaje de texto, según lo establece el ente regulador SUDEBAN", cancellationToken: cancellationToken);

                                if (QtyStatusUnknown > 0)
                                {
                                    ClientData.request = "consultareclamo";
                                    await stepContext.Context.SendActivityAsync($"Si no logras visualizar tu reporte en los que te acabo de indicar, puedo tomar tu solicitud y enviarla al área encargada.", cancellationToken: cancellationToken);
                                    return await stepContext.ReplaceDialogAsync(nameof(NaturalJuridicoDialog), stepContext.Values, cancellationToken);
                                }
                                else
                                {
                                    ClientData.listaReclamos = null;
                                    await stepContext.Context.SendActivityAsync("¿Hay algo más en lo que te pueda ayudar?", cancellationToken: cancellationToken);
                                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                                }
                            }
                        }
                        else
                        {
                            int QtyStatusUnknown = 0;

                            foreach (var elem in responseAPIClaimByClaimID)
                            {
                                if (String.IsNullOrEmpty(getStatusFromEnum(elem.codEstado.ToString() + elem.codEstatus.ToString())))
                                {
                                    QtyStatusUnknown = QtyStatusUnknown + 1;
                                }
                            }
                            if (QtyStatusUnknown == responseAPIClaimByClaimID.Length)
                            {
                                ClientData.request = "consultareclamo";
                                //await stepContext.Context.SendActivityAsync($"A través del sistema no logré validar ningún reporte asociado a la información que me indicaste. ", cancellationToken: cancellationToken);
                                await stepContext.Context.SendActivityAsync($"No logro visualizar en este momento ningún reporte con tu cédula, pero puedo tomar tu solicitud y enviarla al área encargada", cancellationToken: cancellationToken);
                                return await stepContext.ReplaceDialogAsync(nameof(NaturalJuridicoDialog), stepContext.Values, cancellationToken);
                            }

                            await stepContext.Context.SendActivityAsync("A continuación el resultado de tu consulta", cancellationToken: cancellationToken);
                            //Mostrar todos los reclamos
                            foreach (var elem in responseAPIClaimByClaimID)
                            {
                                if (!String.IsNullOrEmpty(getStatusFromEnum(elem.codEstado.ToString() + elem.codEstatus.ToString())))
                                {
                                    await stepContext.Context.SendActivityAsync(
                                    "Reporte: " + elem.nroReclamo.ToString() + $"{ Environment.NewLine}" +
                                    "Monto: " + elem.montoReclamo.ToString() + $"{ Environment.NewLine}" +
                                    "Estatus: " + getStatusFromEnum(elem.codEstado.ToString() + elem.codEstatus.ToString()), cancellationToken: cancellationToken);
                                    ClientData.nroReclamo = null;
                                }
                            }
                            await stepContext.Context.SendActivityAsync($"Un reclamo ya generado, tiene hasta un máximo de 20 días continuos para su resolución, tiempo que empieza a transcurrir una vez se te notifique vía correo electrónico o a través de un mensaje de texto, según lo establece el ente regulador SUDEBAN", cancellationToken: cancellationToken);

                            if (QtyStatusUnknown > 0)
                            {
                                ClientData.request = "consultareclamo";
                                await stepContext.Context.SendActivityAsync($"Si no logras visualizar tu reporte en los que te acabo de indicar, puedo tomar tu solicitud y enviarla al área encargada.", cancellationToken: cancellationToken);
                                return await stepContext.ReplaceDialogAsync(nameof(NaturalJuridicoDialog), stepContext.Values, cancellationToken);
                            }
                            else
                            {
                                ClientData.listaReclamos = null;
                                await stepContext.Context.SendActivityAsync("¿Hay algo más en lo que te pueda ayudar?", cancellationToken: cancellationToken);
                                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                            }
                        }
                    }
                    else
                    {
                        ReclamoariadtoDocumentID responseAPIClaimByClaimID = ClientData.objectReclamo;
                        if (!String.IsNullOrEmpty(ClientData.nroReclamo))
                        {
                            var existClaim = false;
                            if (String.Compare(responseAPIClaimByClaimID.nroReclamo.ToString(), ClientData.nroReclamo, false) == 0)
                            {
                                existClaim = true;
                                if (!String.IsNullOrEmpty(getStatusFromEnum(responseAPIClaimByClaimID.codEstado.ToString() + responseAPIClaimByClaimID.codEstatus.ToString())))
                                {
                                    await stepContext.Context.SendActivityAsync("A continuación el resultado de tu consulta", cancellationToken: cancellationToken);
                                    await stepContext.Context.SendActivityAsync(
                                        "Reporte: " + responseAPIClaimByClaimID.nroReclamo.ToString() + $"{ Environment.NewLine}" +
                                        "Monto: " + responseAPIClaimByClaimID.montoReclamo.ToString() + $"{ Environment.NewLine}" +
                                        "Estatus: " + getStatusFromEnum(responseAPIClaimByClaimID.codEstado.ToString() + responseAPIClaimByClaimID.codEstatus.ToString()), cancellationToken: cancellationToken);
                                    ClientData.nroReclamo = "";
                                    ClientData.objectReclamo = null;
                                    ClientData.request = "QueryClaimFromNumberClaim";
                                    await stepContext.Context.SendActivityAsync($"Un reclamo ya generado, tiene hasta un máximo de 20 días continuos para su resolución, tiempo que empieza a transcurrir una vez se te notifique vía correo electrónico o a través de un mensaje de texto, según lo establece el ente regulador SUDEBAN", cancellationToken: cancellationToken);

                                    var option = await stepContext.PromptAsync(
                                        _CallAPIs,
                                        new PromptOptions
                                        {
                                            Prompt = CreateButtonsStartAgain(""),
                                            RetryPrompt = CreateButtonsStartAgain("Reintento")
                                        }, cancellationToken
                                    );
                                    return option;
                                }

                                else
                                {// Capturar datos enviar al call
                                    existClaim = true;
                                    ClientData.request = "consultareclamo";
                                    await stepContext.Context.SendActivityAsync($"No logro visualizar en este momento ningún reporte con tu cédula, pero puedo tomar tu solicitud y enviarla al área encargada", cancellationToken: cancellationToken);
                                    return await stepContext.ReplaceDialogAsync(nameof(NaturalJuridicoDialog), stepContext.Values, cancellationToken);
                                }
                            }

                            if (!existClaim)
                            {
                                if (!String.IsNullOrEmpty(getStatusFromEnum(responseAPIClaimByClaimID.codEstado.ToString() + responseAPIClaimByClaimID.codEstatus.ToString())))
                                {
                                    await stepContext.Context.SendActivityAsync("A través del sistema no logré validar ningún reporte asociado con el número que me indicaste, sin embargo, visualicé estos reportes asociados a tus datos ( " + ClientData.typeDoc + ClientData.ci + " )", cancellationToken: cancellationToken);

                                    await stepContext.Context.SendActivityAsync(
                                            "Reporte: " + responseAPIClaimByClaimID.nroReclamo.ToString() + $"{ Environment.NewLine}" +
                                            "Monto: " + responseAPIClaimByClaimID.montoReclamo.ToString() + $"{ Environment.NewLine}" +
                                            "Estatus: " + getStatusFromEnum(responseAPIClaimByClaimID.codEstado.ToString() + responseAPIClaimByClaimID.codEstatus.ToString()), cancellationToken: cancellationToken);
                                    ClientData.objectReclamo = null;
                                    ClientData.nroReclamo = null;
                                    await stepContext.Context.SendActivityAsync($"Un reclamo ya generado, tiene hasta un máximo de 20 días continuos para su resolución, tiempo que empieza a transcurrir una vez se te notifique vía correo electrónico o a través de un mensaje de texto, según lo establece el ente regulador SUDEBAN", cancellationToken: cancellationToken);

                                    await stepContext.Context.SendActivityAsync("¿Hay algo más en lo que te pueda ayudar?", cancellationToken: cancellationToken);
                                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                                }
                                else
                                {// Capturar datos enviar al call
                                    ClientData.request = "consultareclamo";
                                    await stepContext.Context.SendActivityAsync($"No logro visualizar en este momento ningún reporte con tu cédula, pero puedo tomar tu solicitud y enviarla al área encargada", cancellationToken: cancellationToken);
                                    return await stepContext.ReplaceDialogAsync(nameof(NaturalJuridicoDialog), stepContext.Values, cancellationToken);
                                }
                            }
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(getStatusFromEnum(responseAPIClaimByClaimID.codEstado.ToString() + responseAPIClaimByClaimID.codEstatus.ToString())))
                            {
                                await stepContext.Context.SendActivityAsync("A continuación el resultado de tu consulta", cancellationToken: cancellationToken);
                                await stepContext.Context.SendActivityAsync(
                                       "Reporte: " + responseAPIClaimByClaimID.nroReclamo.ToString() + $"{ Environment.NewLine}" +
                                       "Monto: " + responseAPIClaimByClaimID.montoReclamo.ToString() + $"{ Environment.NewLine}" +
                                       "Estatus: " + getStatusFromEnum(responseAPIClaimByClaimID.codEstado.ToString() + responseAPIClaimByClaimID.codEstatus.ToString()), cancellationToken: cancellationToken);
                                ClientData.objectReclamo = null;
                                ClientData.nroReclamo = null;
                                await stepContext.Context.SendActivityAsync($"Un reclamo ya generado, tiene hasta un máximo de 20 días continuos para su resolución, tiempo que empieza a transcurrir una vez se te notifique vía correo electrónico o a través de un mensaje de texto, según lo establece el ente regulador SUDEBAN", cancellationToken: cancellationToken);

                                await stepContext.Context.SendActivityAsync("¿Hay algo más en lo que te pueda ayudar?", cancellationToken: cancellationToken);
                                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                            }
                            else
                            {// Capturar datos enviar al call
                                ClientData.request = "consultareclamo";
                                await stepContext.Context.SendActivityAsync($"No logro visualizar en este momento ningún reporte con tu cédula, pero puedo tomar tu solicitud y enviarla al área encargada", cancellationToken: cancellationToken);
                                return await stepContext.ReplaceDialogAsync(nameof(NaturalJuridicoDialog), stepContext.Values, cancellationToken);
                            }
                        }
                    }
                }
                else if (responseClaim.error== 5000)  //La consulta no trajo resultados
                {

                    ClientData.listaReclamos = null;
                    ClientData.objectReclamo = null;
                    ClientData.nroReclamo = null;
                    ClientData.typeDoc = null;
                    ClientData.ci = null;

                    if (ClientData.retryQueryClaim < 1)
                    {
                        await stepContext.Context.SendActivityAsync($"A través del sistema no logré validar ningún reporte asociado a la información que me indicaste. " +
                       $"Te recomiendo realizar tu reporte por el [Formulario de Atención](https://www.bancaribe.com.ve/informacion-vital/atencion-al-cliente/formulario-unico-de-reclamos-de-atencion-al-cliente)", cancellationToken: cancellationToken);

                        var option = await stepContext.PromptAsync(
                            _CallAPIs,
                            new PromptOptions
                            {
                                Prompt = CreateButtonsStartAgainFromDocumentID(""),
                                RetryPrompt = CreateButtonsStartAgainFromDocumentID("Reintento")
                            }, cancellationToken
                        );
                        return option;
                    }
                    else
                    { 
                        if (ClientData.desbordeQueryClaim)
                        {
                            await stepContext.Context.SendActivityAsync($"A través del sistema no logré validar ningún reporte asociado a la información que me indicaste. " +
                                 $"Te recomiendo realizar tu reporte por el [Formulario de Atención](https://www.bancaribe.com.ve/informacion-vital/atencion-al-cliente/formulario-unico-de-reclamos-de-atencion-al-cliente)", cancellationToken: cancellationToken);
                            await stepContext.Context.SendActivityAsync($"De igual manera te recuerdo que envíe la información que me indicaste al departamento encargado");
                            var option = await stepContext.PromptAsync(
                                _CallAPIs,
                                new PromptOptions
                                {
                                    Prompt = CreateButtonsStartAgainFromDocumentID(""),
                                    RetryPrompt = CreateButtonsStartAgainFromDocumentID("Reintento")
                                }, cancellationToken
                            );
                            return option;
                        }
                        else
                        {// Capturar datos enviar al call
                            ClientData.request = "consultareclamo";
                            await stepContext.Context.SendActivityAsync($"A través del sistema no logré validar ningún reporte asociado a la información que me indicaste. ", cancellationToken: cancellationToken);
                            return await stepContext.ReplaceDialogAsync(nameof(NaturalJuridicoDialog), stepContext.Values, cancellationToken);
                        }
                    }
                }
                else if (responseClaim.error.Equals("401"))
                { // Luego de reintentar en QueryClaimAPI no se obtuvo resultados satisfactorios en el llamado
                  // Ahora se debe informar al cliente que no se pudo procesar su solicitud.

                    ClientData.listaReclamos = null;
                    ClientData.objectReclamo = null; 
                    ClientData.request = "consultareclamo";
                    await stepContext.Context.SendActivityAsync($"A través del sistema no logré validar ningún reporte asociado a la información que me indicaste. ", cancellationToken: cancellationToken);
                    return await stepContext.ReplaceDialogAsync(nameof(NaturalJuridicoDialog), stepContext.Values, cancellationToken);
                }
                else
                {

                    ClientData.listaReclamos = null;
                    ClientData.objectReclamo = null; 
                    ClientData.request = "consultareclamo";
                    await stepContext.Context.SendActivityAsync($"A través del sistema no logré validar ningún reporte asociado a la información que me indicaste. ", cancellationToken: cancellationToken);
                    return await stepContext.ReplaceDialogAsync(nameof(NaturalJuridicoDialog), stepContext.Values, cancellationToken);
                }
            }
            else
            {
                ClientData.listaReclamos = null;
                ClientData.objectReclamo = null;
                ClientData.request = "consultareclamo";
                await stepContext.Context.SendActivityAsync($"A través del sistema no logré validar ningún reporte asociado a la información que me indicaste. ", cancellationToken: cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(NaturalJuridicoDialog), stepContext.Values, cancellationToken);
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> StartOverAgain(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserPersonalData());

            var luisResult = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(stepContext.Context, cancellationToken).Result;
            if (luisResult.TopIntent().intent.ToString().Equals("afirmacion"))
            {
                ClientData.retryQueryClaim = 1;
                return await stepContext.ReplaceDialogAsync(nameof(ConsultarReclamoDialog), stepContext.Values, cancellationToken);
            }
            else
            {
                ClientData.nroReclamo = "";
                ClientData.request = "";
                await stepContext.Context.SendActivityAsync("¿Hay algo más en lo que te pueda ayudar?", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }

        #endregion DialogConsultaReclamos


        public string getStatusFromEnum(string codString)
        {
            if(codString.StartsWith("A"))
            {
                return EnumStatusClaim.A;
            }
            else if (codString.StartsWith("D"))
            {
                return EnumStatusClaim.D;
            }
            else if (codString.StartsWith("V"))
            {
                return EnumStatusClaim.V;
            }
            else if (codString.StartsWith("PD"))
            {
                return EnumStatusClaim.PD;
            }
            else if (codString.StartsWith("PV"))
            {
                return EnumStatusClaim.PV;
            }

            switch (codString)
            {
                case "C20":
                    return EnumStatusClaim.C20;
                case "C21":
                    return EnumStatusClaim.C21;
                case "C50":
                    return EnumStatusClaim.C50;
                case "CA20":
                    return EnumStatusClaim.CA20;
                case "CA22":
                    return EnumStatusClaim.CA22;
                case "CN21":
                    return EnumStatusClaim.CN21;
                case "CNA20":
                    return EnumStatusClaim.CNA20;
                case "CPN21":
                    return EnumStatusClaim.CPN21;
                default:
                    return "";
            }
        }


        #region Validators

        private Task<bool> CIPassValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var result = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;
            if (
                result.Entities.venezolano != null
                || result.Entities.Extranjero != null
                || result.Entities.pasaporte != null
                || result.Entities.juridico != null
                || result.Entities.rifgobierno != null
                || promptContext.Context.Activity.Text.ToLower().Equals("j")
                || promptContext.Context.Activity.Text.ToLower().Equals("v")
                || promptContext.Context.Activity.Text.ToLower().Equals("g")
                || promptContext.Context.Activity.Text.ToLower().Equals("e")
                || promptContext.Context.Activity.Text.ToLower().Equals("p")
                )
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        private async Task<bool> CIValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(promptContext.Context, () => new UserPersonalData());
            var Validation = utilitario.ValidateNumberCI(promptContext.Context.Activity.Text, ClientData.typeDoc);
            if (!Validation.Equals("OK"))
            {
                await promptContext.Context.SendActivityAsync(Validation, cancellationToken: cancellationToken);
                return false;
            }

            return true;
        }

        private Task<bool> ValidateInputValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var result = _luisService._luisRecognizer.RecognizeAsync<ClassToGenerate>(promptContext.Context, cancellationToken).Result;
            if (
                result.TopIntent().intent.ToString().Equals("afirmacion")
               || result.TopIntent().intent.ToString().Equals("negación")
               || promptContext.Context.Activity.Text.ToLower().Equals("si")
               || promptContext.Context.Activity.Text.ToLower().Equals("no")
                )
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        #endregion Validators

        #region Buttons
        private Activity CreateButtonsCIPass()
        {
            var reply = MessageFactory.Text("Por favor dime si tu tipo de documento de identidad corresponde a Venezolano (V), Extranjero (E), Pasaporte( P), Jurídico (J) o Gubernamental (G)");
            return reply as Activity;
        }

        private Activity CreateButtonsHasNumberClaim(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿Presentas el número de reporte?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor,¿me confirmas si posees el número de reporte que deseas consultar?");
            }
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() {Title = "Si", Value = "Si" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "No", Value = "No" , Type = ActionTypes.ImBack}

                }
            };
            return reply as Activity;
        }

        private Activity CreateButtonsStartAgain(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿Deseas consultar otro reporte?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, indicame si deseas consultar otro reporte");
            }

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() {Title = "Si", Value = "Si" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "No", Value = "No" , Type = ActionTypes.ImBack}
                }
            };
            return reply as Activity;
        }

        private Activity CreateButtonsStartAgainFromDocumentID(string retry)
        {
            var reply = MessageFactory.Text("");
            if (String.IsNullOrEmpty(retry))
            {
                reply = MessageFactory.Text("¿Deseas corregir los datos ingresados?");
            }
            else
            {
                reply = MessageFactory.Text("Por favor, indicame si deseas consultar otro reporte");
            }

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() {Title = "Si", Value = "Si" , Type = ActionTypes.ImBack},
                    new CardAction() {Title = "No", Value = "No" , Type = ActionTypes.ImBack}
                }
            };
            return reply as Activity;
        }

        #endregion Buttons

    }
}
