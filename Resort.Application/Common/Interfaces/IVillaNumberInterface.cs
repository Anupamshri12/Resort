using Resort.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resort.Application.Common.Interfaces
{
    public interface IVillaNumberInterface:IRepository<VillaNumber>
    {
      
      
        public void Update(VillaNumber villanumber);
       
    }
}
