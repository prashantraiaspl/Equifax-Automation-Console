using Equifax.Api.Domain.Models;
using System.Data.Entity;

namespace Equifax.Api.AppDbContext
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("name=EquifaxContext") { }

        DbSet<RequestMaster> RequestMasters { get; set; }
    }
}
