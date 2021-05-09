using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BanCoreBot.Common.Models.Claim
{
    public class ClaimAllModels
    {
        public string id { get; set; }
        public string code { get; set; }
        public string claimName { get; set; }
        public string fullName { get; set; }
        public string typeDoc { get; set; }
        public string ci { get; set; }
        public string phone { get; set; }
        public string typeClaim { get; set; }
        public string SubTypeClaim { get; set; }
        public bool hasOtherPhone{ get; set; }
        public string phoneAlt { get; set; }
        public string email { get; set; }
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

    }
}
