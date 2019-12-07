using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace app.DAL.Managers
{
    public interface IDBManager<T> : IDisposable
    {
        List<T> Scan();
        Task<T> Get(string id);
        Task<T> Get(T entity);
        Task<bool> Create(T entity);
        Task<bool> Update(T entity);
        Task<bool> Delete(string id);
        Task<bool> Delete(T entity);
    }
}
