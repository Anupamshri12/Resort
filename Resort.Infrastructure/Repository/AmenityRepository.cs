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
    public class AmenityRepository:Repository<Amenity> ,IAmenityInterface
    {
        private readonly ApplicationDbContext _dbContext;
        public AmenityRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }


        public void Update(Amenity amenity)
        {
            _dbContext.Update(amenity);
        }

       
       
    }
}
