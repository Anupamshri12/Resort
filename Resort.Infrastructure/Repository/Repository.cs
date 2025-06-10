using Microsoft.EntityFrameworkCore;
using Resort.Application.Common.Interfaces;
using Resort.Domain.Entities;
using Resort.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Resort.Infrastructure.Repository
{
    public class Repository<T>:IRepository<T> where T:class
    {
        private readonly ApplicationDbContext _dbContext;
        internal DbSet<T> dbset;
        public Repository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            dbset = _dbContext.Set<T>();
        }
        public T Get(Expression<Func<T, bool>> filter, string? includeproperties = null)
        {
            IQueryable<T> query = dbset;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (!string.IsNullOrEmpty(includeproperties))
            {
                foreach (var inclprop in includeproperties.Split(","))
                {
                    query = query.Include(inclprop);
                }
            }
            
            return query.FirstOrDefault();
        }
        public IEnumerable<T>GetAll(Expression<Func<T ,bool>>?filter = null ,string? includeproperties = null)
        {
            IQueryable<T> query = dbset;
            if(filter != null)
            {
                query = query.Where(filter);
            }
            if (!string.IsNullOrEmpty(includeproperties))
            {
                foreach(var inclprop in includeproperties.Split(new char[] {','} ,StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(inclprop);
                }
            }
            query.ToList();
            return query;
        }
       
        public void Remove(T villa)
        {
            dbset.Remove(villa);
        }

        public void Add(T villa)
        {
            dbset.Add(villa);
        }
        
    }
}
