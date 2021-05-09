using BanCoreBot.Common.Models.Claim;
using BanCoreBot.Common.Models.User;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BanCoreBot.Data
{
    public interface IDataBaseService
    {
        DbSet<UserModel> User { get; set; }
        DbSet<ClaimModel> Claim { get; set; }
        public DbSet<UserPersonalData> UserPersonal { get; set; }
        Task<bool> SaveAsync();
    }
}
