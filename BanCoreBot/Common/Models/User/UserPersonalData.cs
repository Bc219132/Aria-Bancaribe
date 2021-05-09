using BanCoreBot.Common.Models.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BanCoreBot.Common.Models.User
{
    public class UserPersonalData

    {
        public string id { get; set; }
        public string userNameChannel { get; set; }
        public string channel { get; set; }
        public string name { get; set; }
        public string typeDoc { get; set; }
        public string ci { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public bool hiprompt { get; set; }
        public string code { get; set; }
        public string claimName { get; set; }
        public string typeClaim { get; set; }
        public string SubTypeClaim { get; set; }
        public bool hasOtherPhone { get; set; }
        public string phoneAlt { get; set; }
        public string date { get; set; }
        public string amount { get; set; }
        public string ult4DigCta { get; set; }
        public string ult4DigTjta { get; set; }
        public string fullNameBenef { get; set; }
        public string nameOtherBank { get; set; }
        public string numCtaBenef { get; set; }
        public string numCelEnvia { get; set; }
        public string numCelRecibe { get; set; }
        public string phoneRefill { get; set; }
        public string Description { get; set; }
        public bool AllData { get; set; }
        public bool Review { get; set; }
        public bool ReviewSecondPhone { get; set; }
        public bool ReviewEmail { get; set; }
        public bool ReviewFecha { get; set; }
        public bool ReviewMonto { get; set; }
        public bool Review4DigCta { get; set; }
        public bool Review4DigTjta { get; set; }
        public bool ReviewDescription { get; set; }
        public string rif { get; set; }
        public string companyname { get; set; }
        public string request { get; set; }
        public bool Saludo1 { get; set; }
        public bool Saludo2 { get; set; }
        public bool ClienteAtendido { get; set; }
        public bool ClienteAtendidoAux { get; set; }
        public string TextoDesborde { get; set; }
        public bool controlDialogvar { get; set; }
        public int controlFecha72h { get; set; }
        public string texto500 { get; set; }
        public string nroReclamo { get; set; }
        public ReclamoariadtoDocumentID[] listaReclamos { get; set; }
        public ReclamoariadtoDocumentID objectReclamo { get; set; }
        public ResponseAPIAuthentication responseAPIAuthentication { get; set; }
        public int retryQueryClaim { get; set; }
        public bool desbordeQueryClaim { get; set; } = false;
        public bool ClaimFound { get; set; } = false;
        public bool APIError { get; set; } = false;
    }
}
