using BanCoreBot.Common.Models.API;
using BanCoreBot.Common.Models.User;
using BanCoreBot.Infrastructure.SendGrid;
using Microsoft.Bot.Builder;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BanCoreBot.Infrastructure.APIs
{
    public class AuthenticationAPI
    {
        private ISendGridEmailService _sendGridEmailService;
        private PrivateConversationState _userState;
        private IHttpClientFactory _httpClientFactory;
        private UserPersonalData ClientData;
        private GeneralResponseAPIs generalResponseAPIs = new GeneralResponseAPIs();

        public AuthenticationAPI(IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
        }

        public async Task<GeneralResponseAPIs> getToken(ISendGridEmailService sendGridEmailService, IHttpClientFactory httpClientFactory, PrivateConversationState userState, ITurnContext Context)
        {
            _httpClientFactory = httpClientFactory;
            _userState = userState;
            _sendGridEmailService = sendGridEmailService;


            var userStateAccessors = _userState.CreateProperty<UserPersonalData>(nameof(UserPersonalData));
            ClientData = await userStateAccessors.GetAsync(Context, () => new UserPersonalData());

            
            var client = _httpClientFactory.CreateClient("AuthenticationAPI");
            double TimeOut = Convert.ToDouble(EasyAccessToConfig.Configuration.GetSection("ApiTimeOut").Value);
            client.Timeout= TimeSpan.FromSeconds(TimeOut);

            var requestToken = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(client.BaseAddress.ToString()),
                Content = new StringContent("grant_type=client_credentials")
            };
            requestToken.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };
            try
            {
                HttpResponseMessage response = await client.SendAsync(requestToken);

                if (response.Content.Headers.ContentType.MediaType.Equals("text/html")) // La API no está expuesta a la calle
                {
                    generalResponseAPIs.error = 1;
                    generalResponseAPIs.description = "ERROR API EXPUESTA SIN ACCESO";

                    //Armar correo para TI 
                    string contentEmail = $"Ha ocurrido un error al Aria intentar consumir la API {Environment.NewLine}" +
                    $"{Environment.NewLine} La API está respondiendo en formato HTML"+
                    $"{Environment.NewLine} Respuesta obtenida al consumir la API"+
                    $"{Environment.NewLine} "+response.Content.ReadAsStringAsync().Result.ToString();
                    ClientData.APIError = true;
                    sendEmail(contentEmail, "Error");

                    return generalResponseAPIs;
                }

                if (response.IsSuccessStatusCode)
                {
                    ResponseAPIAuthentication responseAPI = await response.Content.ReadFromJsonAsync<ResponseAPIAuthentication>();
                    ClientData.responseAPIAuthentication = responseAPI;
                    generalResponseAPIs.error = 0;
                    generalResponseAPIs.description = response.ReasonPhrase;

                    return generalResponseAPIs;
                }
                else
                {
                    generalResponseAPIs.error = 1;
                    generalResponseAPIs.description = response.ReasonPhrase;

                    //Armar correo para TI 
                    string contentEmail = $"Ha ocurrido un error al Aria intentar consumir la API de Autenticación {Environment.NewLine}" +
                    $"{Environment.NewLine} Error: " + response.StatusCode +
                    $"{Environment.NewLine} Razón: " + response.ReasonPhrase;
                    ClientData.APIError = true;
                    sendEmail(contentEmail, "Error");
                    return generalResponseAPIs;
                }
            }
            catch (HttpRequestException exception)
            {
                generalResponseAPIs.error = 1;
                generalResponseAPIs.description = "HttpRequestException when calling the API. " + exception.Message;

                //Armar correo para TI 
                string contentEmail = $"Ha ocurrido una excepción al Aria intentar consumir la API {Environment.NewLine}" +
                $"{Environment.NewLine} API: AuthenticationAPI - API Manager" +
                $"{Environment.NewLine} Excepción Capturada: HttpRequestException" +
                $"{Environment.NewLine} Tipo: "+ exception.GetType().ToString() +
                $"{Environment.NewLine} Mensaje: "+ exception.Message +
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
                $"{Environment.NewLine} API: AuthenticationAPI - API Manager" +
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
                $"{Environment.NewLine} API: AuthenticationAPI - API Manager" +
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
                $"{Environment.NewLine} API: AuthenticationAPI - API Manager" +
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
            string tittle = Error + $" al intentar consumir la API de Autenticación";
            await _sendGridEmailService.Execute(from, fromName, to, toName, tittle, contentEmail, "");


            to = "jzerpa@consein.com";
            toName = "José Zerpa";
            tittle = Error + $" al intentar consumir la API de Autenticación";
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
