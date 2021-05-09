

namespace BanCoreBot.Common.Models.API
{
    public class RequestBodyQueryClaimByDocumentID
    {
        public RequestByDocumentID request { get; set; }
    }

    public class RequestByDocumentID
    {
        public int canal { get; set; }
        public string cedula { get; set; }
        public string identificadorExterno { get; set; }
        public string oficina { get; set; }
        public string operacion { get; set; }
        public int secuencialReclamo { get; set; }
        public string terminal { get; set; }
    }
}
