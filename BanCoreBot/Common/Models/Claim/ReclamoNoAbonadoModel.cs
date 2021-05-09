using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BanCoreBot.Common.Models.Claim
{
    public class ReclamoNoAbonadoModel
    {
        public string typeDoc { get; set; }
        public string ci { get; set; }
        public string numberClaim { get; set; }
        public string amount { get; set; }
        public string date { get; set; }
        public string type { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string fullname { get; set; }
    }
}
