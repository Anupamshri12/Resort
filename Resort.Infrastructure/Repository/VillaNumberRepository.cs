using Microsoft.EntityFrameworkCore;
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
    public class VillaNumberRepository:Repository<VillaNumber> ,IVillaNumberInterface
    {
        private readonly ApplicationDbContext _dbContext;
        public VillaNumberRepository(ApplicationDbContext dbContext):base(dbContext)
        {
            _dbContext = dbContext;
        }
       
       
        public void Update(VillaNumber villaNumber)
        {
            _dbContext.Update(villaNumber); 
        }


        public void Save()
        {
            _dbContext.SaveChanges();
        }
    }
}
