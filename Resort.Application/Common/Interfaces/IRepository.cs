using Resort.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Resort.Application.Common.Interfaces
{
    public interface IRepository<T> where T:class
    {
        public IEnumerable<T> GetAll(Expression<Func<T ,bool>>?filter = null ,string ?includeproperties = null);
        public T Get(Expression<Func<T ,bool>>filter ,string? includeproperties = null);
        public void Add(T villa);
        public void Remove(T villa);
       
    }
}
