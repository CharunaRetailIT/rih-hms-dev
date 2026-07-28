using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.HMSOrderTaker.Data
{
    public  interface IGenericRepository<TEntity, TContext>
    where TEntity : class
    where TContext : DbContext
    {

        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> Get(
           Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
           string includeProperties = "");
        TEntity GetByID(object id);
        void Insert(TEntity entity);


        void Delete(object id);
        void Delete(TEntity entityToDelete);

        void Update(TEntity entityToUpdate);
    }

}
