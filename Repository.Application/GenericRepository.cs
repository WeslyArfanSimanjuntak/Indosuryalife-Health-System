using Interface.Application;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic;

namespace Repository.Application
{
    public class GenericRepository<TEntity> where TEntity : class
    {
        internal DbContext context;
        internal DbSet<TEntity> dbSet;

        public GenericRepository(DbContext context)
        {
            this.context = context;
            this.dbSet = context.Set<TEntity>();
        }

        public virtual IEnumerable<TEntity> Get(
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            foreach (var includeProperty in includeProperties.Split
            (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
            if (orderBy != null)
            {
                var temp = orderBy(query).ToList();
                return temp;
            }
            else
            {
                return query.ToList();
            }
        }

        public virtual IEnumerable<TEntity> Get2(
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter).Skip(2);
            }
            foreach (var includeProperty in includeProperties.Split
            (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
            if (orderBy != null)
            {
                var temp = orderBy(query);
                return temp;
            }
            else
            {
                return query;
            }
        }

        public virtual TEntity GetByID(object id)
        {
            return dbSet.Find(id);
        }
        public virtual void Insert(TEntity entity)
        {
            dbSet.Add(entity);
        }
        public virtual void Delete(object id)
        {
            TEntity entityToDelete = dbSet.Find(id);
            Delete(entityToDelete);
        }
        public virtual void Delete(TEntity entityToDelete)
        {
            if (context.Entry(entityToDelete).State == EntityState.Detached)
            {
                dbSet.Attach(entityToDelete);
            }
            dbSet.Remove(entityToDelete);
        }
        public virtual void Update(TEntity entityToUpdate)
        {
            dbSet.Attach(entityToUpdate);
            context.Entry(entityToUpdate).State = EntityState.Modified;
        }
        public virtual IEnumerable<TEntity> GetWithRawSql(string query, params object[] parameters)
        {
            return dbSet.SqlQuery(query, parameters).ToList();
        }
        public DbRawSqlQuery<TElement> SqlQuery<TElement>(string sql, params object[] parameters)
        {
            return this.context.Database.SqlQuery<TElement>(sql, parameters);

        }

        public virtual IEnumerable<TEntity> GetForDropDown(
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        string includeProperties = "", bool giveEmptyList = true)
        {



            IQueryable<TEntity> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            foreach (var includeProperty in includeProperties.Split
            (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
            if (orderBy != null)
            {
                var temp = orderBy(query).ToList();
                if (giveEmptyList)
                {
                    TEntity emptyEntity = null;
                    temp.Reverse();
                    temp.Add(emptyEntity);
                    temp.Reverse();
                }
                return temp;
            }
            else
            {
                var temp = query.ToList();
                if (giveEmptyList)
                {
                    TEntity emptyEntity = null;
                    temp.Reverse();
                    temp.Add(emptyEntity);
                    temp.Reverse();
                }
                return temp;
            }
        }
        public virtual void SetAuditLog(object id, TEntity entityToSet)
        {



            TEntity entityAuditLogToSet = dbSet.Find(id);
            if ((entityAuditLogToSet as IAuditableObject != null) && (entityToSet as IAuditableObject != null))
            {
                (entityToSet as IAuditableObject).CreatedBy = (entityAuditLogToSet as IAuditableObject).CreatedBy;
                (entityToSet as IAuditableObject).CreatedDate = (entityAuditLogToSet as IAuditableObject).CreatedDate;
                (entityToSet as IAuditableObject).UpdatedBy = (entityAuditLogToSet as IAuditableObject).UpdatedBy;
                (entityToSet as IAuditableObject).UpdatedDate = (entityAuditLogToSet as IAuditableObject).UpdatedDate;
            };
            context.Entry(entityAuditLogToSet).State = EntityState.Detached;
        }
        public virtual IEnumerable<TEntity> GetForDataTable(out int totalRecords, int rowSkip = 0, int rowToTake = 10, string orderByField = "1", string orderByType = "ASC",
      Expression<Func<TEntity, bool>> filter = null,

      string includeProperties = "")
        {


            totalRecords = 0;
            IQueryable<TEntity> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
                totalRecords = query.Count();
            }
            foreach (var includeProperty in includeProperties.Split
            (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
            totalRecords = query.Count();
            return query.OrderBy(orderByField + " " + orderByType).Skip(rowSkip).Take(rowToTake);

        }
    }
}
