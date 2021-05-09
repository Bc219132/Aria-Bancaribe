using BanCoreBot.Common.Models.Claim;
using BanCoreBot.Common.Models.User;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BanCoreBot.Data
{
    public class DataBaseService :  DbContext, IDataBaseService
    {
        public DataBaseService(DbContextOptions options ): base (options)
        {
            Database.EnsureCreatedAsync();
        }

        public DataBaseService()
        {
            Database.EnsureCreatedAsync();
        }
        public DbSet<UserModel> User { get; set; }
        public DbSet<ClaimModel> Claim { get; set; }
        public DbSet<UserPersonalData> UserPersonal { get; set; }
        
        public async Task<bool> SaveAsync() 
        {
            return (await SaveChangesAsync() >0 );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>().ToContainer("User").HasPartitionKey("channel").HasNoDiscriminator().HasKey("id");
            modelBuilder.Entity<ClaimModel>().ToContainer("Claim").HasPartitionKey("type").HasNoDiscriminator().HasKey("ci");
            modelBuilder.Entity<UserPersonalData>().ToContainer("UserPersonal").HasPartitionKey("ci").HasNoDiscriminator().HasKey("id");
        }


    }
}
 