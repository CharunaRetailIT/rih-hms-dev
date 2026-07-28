using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using RIT.HMS.Domain.Transactions;

namespace RIT.HMS.Data
{
    public class GenericRepository<TEntity> where TEntity : class
    {
        internal ApplicationDbContext Context;
        internal DbSet<TEntity> DbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            this.Context = context;
            this.DbSet = context.Set<TEntity>();
        }

        //public virtual IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null,
        //   Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,string includeProperties = "")
        public virtual IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null,Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null)
        {
            IQueryable<TEntity> query = DbSet;//

            if (filter != null)
            {
                query = query.Where(filter);
            }

            //foreach (var includeProperty in includeProperties.Split
            //    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            //{
            //    query = query.Include(includeProperty);

            //}

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

      

        public virtual IEnumerable<TEntity> GetAsNoTracking(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = DbSet;

            if (filter != null)
            {
                query = query.AsNoTracking().Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

        public virtual TEntity GetById(object id)
        {
            return DbSet.Find(id);
        }

        public virtual void Insert(TEntity entity)
        {
            DbSet.Add(entity);
        }



        public virtual void BulkInsert(ICollection<TEntity> entity)
        {
            DbSet.AddRange(entity);
        }

        public virtual void Delete(object id)
        {
            TEntity entityToDelete = DbSet.Find(id);
            Delete(entityToDelete);
        }

        public virtual void Delete(TEntity entityToDelete)
        {
            if (Context.Entry(entityToDelete).State == EntityState.Modified)
            {
                DbSet.Attach(entityToDelete);
            }
            DbSet.Remove(entityToDelete);
        }

        public virtual void DeleteRange(IEnumerable<TEntity> entitiesToDelete)
        {
            //if (Context.Entry(entityToDelete).State == EntityState.Modified)
            //{
            //    DbSet.Attach(entityToDelete);
            //}
            DbSet.RemoveRange(entitiesToDelete);
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            DbSet.Attach(entityToUpdate);
            Context.Entry(entityToUpdate).State = EntityState.Modified;
        }
        public virtual void UpdateIsTransferTOG(TEntity entityToUpdate)
        {
            DbSet.Attach(entityToUpdate);
            Context.Entry(entityToUpdate).State = EntityState.Modified;
        }

        public virtual void UpdateById(TEntity entityToUpdate, object id, object AccessId)
        {
            TEntity entityToDelete = DbSet.Find(id, AccessId);
            Context.Entry(entityToUpdate).State = EntityState.Modified;
        }

        public virtual void UpdateBySet(TEntity entityToUpdate, TEntity entityFromUpdate)
        {

            Context.Entry(entityToUpdate).CurrentValues.SetValues(entityFromUpdate);
           
        }

        public virtual void UpdateByBulk(ICollection<TEntity> entityToUpdate, ICollection<TEntity> entityFromUpdate)
        {

            Context.Entry(entityToUpdate).CurrentValues.SetValues(entityFromUpdate);

        }


        // only for temporary :) because of hurry to handover   -- BY hasanka 
        public virtual IEnumerable<TEntity> ExecuteSPDailySales(string spname, object datetime,object location)
        {
            return Context.Database.SqlQuery<TEntity>(spname, datetime,location).ToList();
        }
        public virtual IEnumerable<TEntity> ExecuteSPImportJurnalDet(string spname, object Fromdatetime, object Todatetime)
        {
            return Context.Database.SqlQuery<TEntity>(spname, Fromdatetime, Todatetime).ToList();
        }
        public virtual IEnumerable<TEntity> ExecuteSPTranferToGL(string spname, object Fromdatetime, object Todatetime, object GroupCompany)
        {
            return Context.Database.SqlQuery<TEntity>(spname, Fromdatetime, Todatetime, GroupCompany).ToList();
        }

        public virtual IEnumerable<TEntity> ExecuteSPSalesRegister(string name,object object1, object object2, object object3, object object4, object object5,
            object object6, object object7, object object8, object object9, object object10, object object11,object object12,object object13)

        {
            try
            {
                return Context.Database.SqlQuery<TEntity>(name, object1, object2, object3, object4, object5,
                 object6, object7, object8, object9, object10, object11, object12, object13).ToList();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        //  -------------------------------------------------------------------------------------
        public virtual IEnumerable<TEntity> ExecuteSPIsValidMonthEnd(string spname, 
            object location, object year, object month)
        {
            return Context.Database.SqlQuery<TEntity>(spname, location, year, month).ToList();
        }

        // dashboard version 2.0.0
        public virtual DbRawSqlQuery<T> SQLQuery<T>(string sql, params object[] parameters)
        {
            return Context.Database.SqlQuery<T>(sql, parameters);
        }

        public async Task<List<T>> SQLQueryAsync<T>(string sql, params object[] parameters)
        {
            return await Context.Database.SqlQuery<T>(sql, parameters).ToListAsync();
        }
        //public object Get(Func<PurchaseHeader, bool> p)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
