using GoDeliver.Application.Exceptions;
using GoDeliver.Core.Interfaces;
using GoDeliver.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Infrastructure.Repositories
{
    public class GenericRepository <T> : IGenericRepository<T> where T : class
    {
        protected readonly GoDeliverDbContext _Context;
        protected readonly DbSet<T> _dbSet;
        public GenericRepository(GoDeliverDbContext dbContext)
        {
            _Context = dbContext;
            _dbSet = _Context.Set<T>();
        }

        public async Task<T?> GetByIdAsync ( Guid id )
        {
            var obj =  await _dbSet.FindAsync( id );
            if (obj == null) throw new NotFoundException(typeof(T).Name, id);
            return obj;
        }

        public async Task<IEnumerable<T>> GetAllAsync () => await _dbSet.ToListAsync();

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
          var resultlist = await _dbSet.Where(predicate).ToListAsync();
            if (resultlist.Count == 0) throw new NotFoundException(typeof(T).Name, "matching criteria");
            return resultlist;
        }

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);


        public void Update(T entity) => _dbSet.Update(entity);

        public void Delete(T entity) => _dbSet.Remove(entity);


    }
}
