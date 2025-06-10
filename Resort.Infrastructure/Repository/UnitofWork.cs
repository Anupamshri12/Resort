using Resort.Application.Common.Interfaces;
using Resort.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resort.Infrastructure.Repository
{
    public class UnitofWork:IUnitofWork
    {
        private readonly ApplicationDbContext _dbContext;
        public IVillaInterface Villa { get; private set; }
        public IVillaNumberInterface VillaNumberInterface { get; private set; }
        public IAmenityInterface Amenity { get; private set; }
        public IBookingInterface BookingInterface { get; set; }
        public IApplicationUserInterface User { get; set; }
        public UnitofWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            Villa = new VillaRepository(_dbContext);
            BookingInterface = new BookingRepository(_dbContext);
            VillaNumberInterface = new VillaNumberRepository(_dbContext);
            Amenity = new AmenityRepository(_dbContext);
            User = new ApplicationUserRepository(_dbContext);

        }
        public void Save()
        {
            _dbContext.SaveChanges();
        }
    }
}
