using System;

namespace BanCoreBot.Common.Models.API
{
    public class RequestBodyQueryClaimByClaimID
    {
        public Request request { get; set; }
    }

    public class Request
    {
        public int canal { get; set; }
        public string identificadorExterno { get; set; }
        public string oficina { get; set; }
        public string operacion { get; set; }
        public int secuencialReclamo { get; set; }
        public string terminal { get; set; }

    }

}


