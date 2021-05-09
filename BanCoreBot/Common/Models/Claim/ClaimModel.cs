using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BanCoreBot.Common.Models.Claim
{
    public class ClaimModel
    {
        public string id { get; set; }
        public string type { get; set; }
        public string fullName { get; set; }
        public string typeDoc { get; set; }
        public string ci { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string Description { get; set; }
    }
}
