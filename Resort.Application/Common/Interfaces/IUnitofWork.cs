using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resort.Application.Common.Interfaces
{
    public  interface IUnitofWork
    {
        IVillaInterface Villa { get;}
        IVillaNumberInterface VillaNumberInterface { get;}
        IAmenityInterface Amenity { get;}

        IBookingInterface BookingInterface { get; }
        IApplicationUserInterface User { get; set; }
        void Save();

    }
}
