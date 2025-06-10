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
    public class VillaRepository:Repository<Villa>,IVillaInterface
    {
        private readonly ApplicationDbContext _dbContext;
        public VillaRepository(ApplicationDbContext dbContext):base(dbContext)
        {
            _dbContext = dbContext;
        }
        
       
        public void Update(Villa villa)
        {
            _dbContext.Update(villa);
        }
        
        public DateTime GetCreatedTime(Villa villa)
        {
            DateTime datetime = (DateTime)_dbContext.Villas.Where(v => v.Id == villa.Id).Select(v => v.Created_Time).FirstOrDefault();
            return datetime;
        }
        public void Save()
        {
            _dbContext.SaveChanges();
        }
    }
}
