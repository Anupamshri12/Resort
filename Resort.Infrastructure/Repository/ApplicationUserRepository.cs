using Resort.Application.Common.Interfaces;
using Resort.Domain.Entities;
using Resort.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resort.Infrastructure.Repository
{
    public class ApplicationUserRepository:Repository<ApplicationUser> ,IApplicationUserInterface
    {
        private readonly ApplicationDbContext _dbContext;
        public ApplicationUserRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }


       

       
       
    }
}
