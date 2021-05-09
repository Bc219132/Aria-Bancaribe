using BanCoreBot.Common.Models.API;
using BanCoreBot.Common.Models.User;
using BanCoreBot.Infrastructure.SendGrid;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BanCoreBot.Infrastructure.APIs
{
    public class QueryClaimAPI
    {
        private ISendGridEmailService _sendGridEmailService;
        private PrivateConversationState _userState;
        private IHttpClientFactory _httpClientFactory;
        private UserPersonalData ClientData;
        private GeneralResponseAPIs generalResponseAPIs = new GeneralResponseAPIs();

        public QueryClaimAPI(IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
        }

        public async Task<GeneralResponseAPIs> getClaimByClaimID(ISendGridEmailService sendGridEmailService, IHttpClientFactory httpClientFactory, int secuencialReclamo, PrivateConversationState userState, ITurnContext Context)
        {
            _httpClientFactory = httpClientFactory;
            _userState = userState;
            _sendGridEmailService = sendGridEmailService;

            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(Context, () => new UserPersonalData());

            var client = _httpClientFactory.CreateClient("QueryClaim");
            double TimeOut = Convert.ToDouble(EasyAccessToConfig.Configuration.GetSection("ApiTimeOut").Value);
            client.Timeout = TimeSpan.FromSeconds(TimeOut);
            
            var requestClaim = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(client.BaseAddress.ToString())
            };
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + ClientData.responseAPIAuthentication.access_token);
            var requestformat = new RequestBodyQueryClaimByClaimID();
            requestformat.request = new Request();
            requestformat.request.canal = 1;
            requestformat.request.identificadorExterno = ClientData.responseAPIAuthentication.access_token;
            requestformat.request.operacion = "C";
            requestformat.request.secuencialReclamo = secuencialReclamo;
            requestformat.request.terminal = "J00254590-9";


            requestClaim.Content = new StringContent(JsonConvert.SerializeObject(requestformat), Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.SendAsync(requestClaim);
                if (response.IsSuccessStatusCode)
                {
                    var jsonPuro = await response.Content.ReadAsStringAsync();
                    var json3 = JValue.Parse(jsonPuro);
                    Out Salida = JsonConvert.DeserializeObject<Out>(json3["Envelope"]["Body"]["consultarReclamosAriaResponse"]["out"].ToString());

                    if (Salida.codigoError == 0)// Transacción Exitosa
                    {
                        ReclamoariadtoDocumentID ResponseAPIClaimByIDClaim = JsonConvert.DeserializeObject<ReclamoariadtoDocumentID>(json3["Envelope"]["Body"]["consultarReclamosAriaResponse"]["out"]["listaReclamos"]["ReclamoAriaDTO"].ToString());
                        ClientData.objectReclamo = ResponseAPIClaimByIDClaim;
                        generalResponseAPIs.error = 0;
                        generalResponseAPIs.description = response.ReasonPhrase;
                        return await Task.FromResult(generalResponseAPIs);
                    }
                    else if (Salida.codigoError == 5000) //Consulta sin resultados
                    {
                        generalResponseAPIs.error = Salida.codigoError;
                        generalResponseAPIs.description = Salida.descripcionError;
                        return await Task.FromResult(generalResponseAPIs);
                    }
                    else if (Salida.codigoError == 401)
                    {   //Solicitar un nuevo Token y reintentar
                        var authApi = new AuthenticationAPI(_httpClientFactory);
                        await authApi.getToken(_sendGridEmailService, _httpClientFactory, _userState, Context);
                        return await getClaimByClaimID(_sendGridEmailService, httpClientFactory, secuencialReclamo, userState, Context);
                    }
                    else
                    {   //Error general
                        generalResponseAPIs.error = Salida.codigoError;
                        generalResponseAPIs.description = Salida.descripcionError;

                        //Armar correo para TI 
                        string contentEmail = $"Ha ocurrido un error al Aria intentar consumir la API de Consulta de Reclamos {Environment.NewLine}" +
                        $"{Environment.NewLine} Error: " + response.StatusCode +
                        $"{Environment.NewLine} Razón: " + response.ReasonPhrase;
                        ClientData.APIError = true;
                        sendEmail(contentEmail, "Error");
                        return await Task.FromResult(generalResponseAPIs);
                    }

                }
                else
                {
                    //Error general
                    if (response.StatusCode.Equals("InternalServerError"))
                    {
                        generalResponseAPIs.error = 500;
                        generalResponseAPIs.description = response.ReasonPhrase;
                    }
                    else
                    {
                        generalResponseAPIs.error = 1;
                        generalResponseAPIs.description = response.ReasonPhrase;
                    }

                    //Armar correo para TI 
                    string contentEmail = $"Ha ocurrido un error al Aria intentar consumir la API de Consulta de Reclamos {Environment.NewLine}" +
                    $"{Environment.NewLine} Error: " + response.StatusCode +
                    $"{Environment.NewLine} Razón: " + response.ReasonPhrase;
                    ClientData.APIError = true;
                    sendEmail(contentEmail, "Error");
                    return await Task.FromResult(generalResponseAPIs);
                }
            }
            catch (HttpRequestException exception)
            {
                generalResponseAPIs.error = 1;
                generalResponseAPIs.description = "HttpRequestException when calling the API. " + exception.Message;

                //Armar correo para TI 
                string contentEmail = $"Ha ocurrido una excepción al Aria intentar consumir la API {Environment.NewLine}" +
                $"{Environment.NewLine} API: Consulta de Reclamos" +
                $"{Environment.NewLine} Excepción Capturada: HttpRequestException" +
                $"{Environment.NewLine} Tipo: " + exception.GetType().ToString() +
                $"{Environment.NewLine} Mensaje: " + exception.Message +
                $"{Environment.NewLine} Información restante disponible {Environment.NewLine}" +
                $"{Environment.NewLine} HelpLink: " + exception.HelpLink +
                $"{Environment.NewLine} HResult: " + exception.HResult +
                $"{Environment.NewLine} InnerException: " + exception.InnerException +
                $"{Environment.NewLine} Source: " + exception.Source +
                $"{Environment.NewLine} StackTrace: " + exception.StackTrace +
                $"{Environment.NewLine} TargetSite: " + exception.TargetSite;
                ClientData.APIError = true;
                sendEmail(contentEmail, "Excepción");
                return generalResponseAPIs;
            }
            catch (TimeoutException exception)
            {
                generalResponseAPIs.error = 1;
                generalResponseAPIs.description = "TimeoutException during call to API. " + exception.Message;

                //Armar correo para TI 
                string contentEmail = $"Ha ocurrido una excepción al Aria intentar consumir la API {Environment.NewLine}" +
                $"{Environment.NewLine} API: Consulta de Reclamos" +
                $"{Environment.NewLine} Excepción Capturada: TimeoutException" +
                $"{Environment.NewLine} Tipo: " + exception.GetType().ToString() +
                $"{Environment.NewLine} Mensaje: " + exception.Message +
                $"{Environment.NewLine} Información restante disponible {Environment.NewLine}" +
                $"{Environment.NewLine} HelpLink: " + exception.HelpLink +
                $"{Environment.NewLine} HResult: " + exception.HResult +
                $"{Environment.NewLine} InnerException: " + exception.InnerException +
                $"{Environment.NewLine} Source: " + exception.Source +
                $"{Environment.NewLine} StackTrace: " + exception.StackTrace +
                $"{Environment.NewLine} TargetSite: " + exception.TargetSite;
                ClientData.APIError = true;
                sendEmail(contentEmail, "Excepción");
                return generalResponseAPIs;
            }
            catch (OperationCanceledException exception)
            {
                generalResponseAPIs.error = 1;
                generalResponseAPIs.description = "OperationCanceledException during call to API. " + exception.Message;

                //Armar correo para TI 
                string contentEmail = $"Ha ocurrido una excepción al Aria intentar consumir la API {Environment.NewLine}" +
                $"{Environment.NewLine} API: Consulta de Reclamos" +
                $"{Environment.NewLine} Excepción Capturada: OperationCanceledException" +
                $"{Environment.NewLine} Tipo: " + exception.GetType().ToString() +
                $"{Environment.NewLine} Mensaje: " + exception.Message +
                $"{Environment.NewLine} Información restante disponible {Environment.NewLine}" +
                $"{Environment.NewLine} HelpLink: " + exception.HelpLink +
                $"{Environment.NewLine} HResult: " + exception.HResult +
                $"{Environment.NewLine} InnerException: " + exception.InnerException +
                $"{Environment.NewLine} Source: " + exception.Source +
                $"{Environment.NewLine} StackTrace: " + exception.StackTrace +
                $"{Environment.NewLine} TargetSite: " + exception.TargetSite;
                ClientData.APIError = true;
                sendEmail2Consein(contentEmail, "Excepción");
                return generalResponseAPIs;
            }
            catch (Exception exception)
            {
                generalResponseAPIs.error = 1;
                generalResponseAPIs.description = "Unhandled exception when calling the API. " + exception.Message;

                //Armar correo para TI 
                string contentEmail = $"Ha ocurrido una excepción al Aria intentar consumir la API {Environment.NewLine}" +
                $"{Environment.NewLine} API: Consulta de Reclamos" +
                $"{Environment.NewLine} Excepción Capturada: Exception" +
                $"{Environment.NewLine} Tipo: " + exception.GetType().ToString() +
                $"{Environment.NewLine} Mensaje: " + exception.Message +
                $"{Environment.NewLine} Información restante disponible {Environment.NewLine}" +
                $"{Environment.NewLine} HelpLink: " + exception.HelpLink +
                $"{Environment.NewLine} HResult: " + exception.HResult +
                $"{Environment.NewLine} InnerException: " + exception.InnerException +
                $"{Environment.NewLine} Source: " + exception.Source +
                $"{Environment.NewLine} StackTrace: " + exception.StackTrace +
                $"{Environment.NewLine} TargetSite: " + exception.TargetSite;
                ClientData.APIError = true;
                sendEmail(contentEmail, "Excepción");
                return generalResponseAPIs;
            }
        }

        public async Task<GeneralResponseAPIs> getClaimByDocumentID(ISendGridEmailService sendGridEmailService, IHttpClientFactory httpClientFactory, string cedula, PrivateConversationState userState, ITurnContext Context)
        {
            _httpClientFactory = httpClientFactory;
            _userState = userState;
            _sendGridEmailService = sendGridEmailService;

            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(Context, () => new UserPersonalData());

            var client = _httpClientFactory.CreateClient("QueryClaim");
            double TimeOut = Convert.ToDouble(EasyAccessToConfig.Configuration.GetSection("ApiTimeOut").Value);
            client.Timeout = TimeSpan.FromSeconds(TimeOut);

            var requestClaim = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(client.BaseAddress.ToString())
            };
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + ClientData.responseAPIAuthentication.access_token);
            
            var requestformat = new RequestBodyQueryClaimByDocumentID();
            requestformat.request = new RequestByDocumentID();
            requestformat.request.canal = 1;
            requestformat.request.cedula = cedula;
            requestformat.request.identificadorExterno = ClientData.responseAPIAuthentication.access_token;
            requestformat.request.operacion = "A";
            requestformat.request.secuencialReclamo = 0;
            requestformat.request.terminal = "J00254590-9";
            

            requestClaim.Content = new StringContent(JsonConvert.SerializeObject(requestformat), Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.SendAsync(requestClaim);
                if (response.IsSuccessStatusCode)
                {

                    var jsonPuro = await response.Content.ReadAsStringAsync();
                    var json3 = JValue.Parse(jsonPuro);
                    if(jsonPuro.IndexOf("[")==-1)
                    {
                        Out Salida = JsonConvert.DeserializeObject<Out>(json3["Envelope"]["Body"]["consultarReclamosAriaResponse"]["out"].ToString());
                        generalResponseAPIs.error = Salida.codigoError;
                        generalResponseAPIs.description = Salida.descripcionError;
                        generalResponseAPIs.control = "ok1";
                    }
                    else
                    {
                        OutDocumentID Salida = JsonConvert.DeserializeObject<OutDocumentID>(json3["Envelope"]["Body"]["consultarReclamosAriaResponse"]["out"].ToString());
                        generalResponseAPIs.error = Salida.codigoError;
                        generalResponseAPIs.description = Salida.descripcionError;
                        generalResponseAPIs.control = "ok2";
                    }
                    
                    if (generalResponseAPIs.error==0)// Transacción Exitosa
                    {
                        if (generalResponseAPIs.control.Equals("ok2"))
                        {
                            ReclamoariadtoDocumentID[] ResponseAPIClaimByDocumentID = JsonConvert.DeserializeObject<ReclamoariadtoDocumentID[]>(json3["Envelope"]["Body"]["consultarReclamosAriaResponse"]["out"]["listaReclamos"]["ReclamoAriaDTO"].ToString());
                            ClientData.listaReclamos = ResponseAPIClaimByDocumentID;
                        }
                        else
                        {
                            ReclamoariadtoDocumentID ResponseAPIClaimByIDClaim = JsonConvert.DeserializeObject<ReclamoariadtoDocumentID>(json3["Envelope"]["Body"]["consultarReclamosAriaResponse"]["out"]["listaReclamos"]["ReclamoAriaDTO"].ToString());
                            ClientData.objectReclamo = ResponseAPIClaimByIDClaim;
                        }
                        generalResponseAPIs.error = 0;
                        generalResponseAPIs.description = response.ReasonPhrase;
                        return await Task.FromResult(generalResponseAPIs);
                    }
                    else if (generalResponseAPIs.error== 5000) //Consulta sin resultados
                    {
                        generalResponseAPIs.description = response.ReasonPhrase;
                        return await Task.FromResult(generalResponseAPIs);
                    }
                    else
                    {   //Error general

                        //Armar correo para TI 
                        string contentEmail = $"Ha ocurrido un error al Aria intentar consumir la API de Consulta de Reclamos {Environment.NewLine}" +
                        $"{Environment.NewLine} Error: " + response.StatusCode +
                        $"{Environment.NewLine} Razón: " + response.ReasonPhrase + " " + response.Content.ToString();
                        ClientData.APIError = true;
                        sendEmail(contentEmail, "Error");
                        return await Task.FromResult(generalResponseAPIs);
                    }
                }

                else if ((int)response.StatusCode== 401)
                {   //Solicitar un nuevo Token y reintentar
                    var authApi = new AuthenticationAPI(_httpClientFactory);
                    await authApi.getToken(_sendGridEmailService, _httpClientFactory, _userState, Context);
                    return await getClaimByDocumentID(_sendGridEmailService, httpClientFactory, cedula, userState, Context);
                }
                else
                {
                    if (response.StatusCode.Equals("InternalServerError"))
                    {
                        generalResponseAPIs.error = 500;
                        generalResponseAPIs.description = response.ReasonPhrase;
                    }
                    else
                    {
                        generalResponseAPIs.error = 1;
                        generalResponseAPIs.description = response.ReasonPhrase;
                    }
                    //Armar correo para TI 
                    string contentEmail = $"Ha ocurrido un error al Aria intentar consumir la API de Consulta de Reclamos {Environment.NewLine}" +
                    $"{Environment.NewLine} Error: " + response.StatusCode +
                    $"{Environment.NewLine} Razón: " + response.ReasonPhrase;
                    ClientData.APIError = true;
                    sendEmail(contentEmail, "Error");
                    return await Task.FromResult(generalResponseAPIs);
                }
            }

            catch (HttpRequestException exception)
            {
                generalResponseAPIs.error = 1;
                generalResponseAPIs.description = "HttpRequestException when calling the API. " + exception.Message;

                //Armar correo para TI 
                string contentEmail = $"Ha ocurrido una excepción al Aria intentar consumir la API {Environment.NewLine}" +
                $"{Environment.NewLine} API: Consulta de Reclamos" +
                $"{Environment.NewLine} Excepción Capturada: HttpRequestException" +
                $"{Environment.NewLine} Tipo: " + exception.GetType().ToString() +
                $"{Environment.NewLine} Mensaje: " + exception.Message +
                $"{Environment.NewLine} Información restante disponible {Environment.NewLine}" +
                $"{Environment.NewLine} HelpLink: " + exception.HelpLink +
                $"{Environment.NewLine} HResult: " + exception.HResult +
                $"{Environment.NewLine} InnerException: " + exception.InnerException +
                $"{Environment.NewLine} Source: " + exception.Source +
                $"{Environment.NewLine} StackTrace: " + exception.StackTrace +
                $"{Environment.NewLine} TargetSite: " + exception.TargetSite;
                ClientData.APIError = true;
                sendEmail(contentEmail, "Excepción");
                return generalResponseAPIs;
            }
            catch (TimeoutException exception)
            {
                generalResponseAPIs.error = 1;
                generalResponseAPIs.description = "TimeoutException during call to API. " + exception.Message;

                //Armar correo para TI 
                string contentEmail = $"Ha ocurrido una excepción al Aria intentar consumir la API {Environment.NewLine}" +
                $"{Environment.NewLine} API: Consulta de Reclamos" +
                $"{Environment.NewLine} Excepción Capturada: TimeoutException" +
                $"{Environment.NewLine} Tipo: " + exception.GetType().ToString() +
                $"{Environment.NewLine} Mensaje: " + exception.Message +
                $"{Environment.NewLine} Información restante disponible {Environment.NewLine}" +
                $"{Environment.NewLine} HelpLink: " + exception.HelpLink +
                $"{Environment.NewLine} HResult: " + exception.HResult +
                $"{Environment.NewLine} InnerException: " + exception.InnerException +
                $"{Environment.NewLine} Source: " + exception.Source +
                $"{Environment.NewLine} StackTrace: " + exception.StackTrace +
                $"{Environment.NewLine} TargetSite: " + exception.TargetSite;
                ClientData.APIError = true;
                sendEmail(contentEmail, "Excepción");
                return generalResponseAPIs;
            }
            catch (OperationCanceledException exception)
            {
                generalResponseAPIs.error = 1;
                generalResponseAPIs.description = "OperationCanceledException during call to API. " + exception.Message;

                //Armar correo para TI 
                string contentEmail = $"Ha ocurrido una excepción al Aria intentar consumir la API {Environment.NewLine}" +
                $"{Environment.NewLine} API: Consulta de Reclamos" +
                $"{Environment.NewLine} Excepción Capturada: OperationCanceledException" +
                $"{Environment.NewLine} Tipo: " + exception.GetType().ToString() +
                $"{Environment.NewLine} Mensaje: " + exception.Message +
                $"{Environment.NewLine} Información restante disponible {Environment.NewLine}" +
                $"{Environment.NewLine} HelpLink: " + exception.HelpLink +
                $"{Environment.NewLine} HResult: " + exception.HResult +
                $"{Environment.NewLine} InnerException: " + exception.InnerException +
                $"{Environment.NewLine} Source: " + exception.Source +
                $"{Environment.NewLine} StackTrace: " + exception.StackTrace +
                $"{Environment.NewLine} TargetSite: " + exception.TargetSite;
                ClientData.APIError = true;
                sendEmail2Consein(contentEmail, "Excepción");
                return generalResponseAPIs;
            }
            catch (Exception exception)
            {
                generalResponseAPIs.error = 1;
                generalResponseAPIs.description = "Unhandled exception when calling the API. " + exception.Message;

                //Armar correo para TI 
                string contentEmail = $"Ha ocurrido una excepción al Aria intentar consumir la API {Environment.NewLine}" +
                $"{Environment.NewLine} API: Consulta de Reclamos" +
                $"{Environment.NewLine} Excepción Capturada: Exception" +
                $"{Environment.NewLine} Tipo: " + exception.GetType().ToString() +
                $"{Environment.NewLine} Mensaje: " + exception.Message +
                $"{Environment.NewLine} Información restante disponible {Environment.NewLine}" +
                $"{Environment.NewLine} HelpLink: " + exception.HelpLink +
                $"{Environment.NewLine} HResult: " + exception.HResult +
                $"{Environment.NewLine} InnerException: " + exception.InnerException +
                $"{Environment.NewLine} Source: " + exception.Source +
                $"{Environment.NewLine} StackTrace: " + exception.StackTrace +
                $"{Environment.NewLine} TargetSite: " + exception.TargetSite;
                ClientData.APIError = true;
                sendEmail(contentEmail, "Excepción");
                return generalResponseAPIs;
            }
        }

        private async void sendEmail(string contentEmail, string Error) // Enviar correo al personal de TI
        {
            string from = "Aria@consein.com";
            string fromName = "Aria";
            string to = "GerenciadeOperacionesIT@bancaribe.com.ve";
            string toName = "GerenciadeOperacionesIT";
            string tittle = Error + $" al intentar consumir la API de Consulta de Reclamos";
            await _sendGridEmailService.Execute(from, fromName, to, toName, tittle, contentEmail, "");


            to = "jzerpa@consein.com";
            toName = "José Zerpa";
            tittle = Error + $" al intentar consumir la API de Consulta de Reclamos";
            await _sendGridEmailService.Execute(from, fromName, to, toName, tittle, contentEmail, "");
        }

        private async void sendEmail2Consein(string contentEmail, string Error) // Enviar correo al personal de TI
        {
            string from = "Aria@consein.com";
            string fromName = "Aria";
            string to = "jzerpa@consein.com";
            string toName = "José Zerpa";
            string tittle = Error + $" al intentar consumir la API de Consulta de Reclamos";
            await _sendGridEmailService.Execute(from, fromName, to, toName, tittle, contentEmail, "");
        }
    }
}
