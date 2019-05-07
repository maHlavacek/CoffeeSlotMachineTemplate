using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSlotMachine.Core.Contracts
{
    public interface IUnitOfWork : IDisposable
    {
        ICoinRepository CoinRepository { get; }
        IOrderRepository OrderRepository { get; }
        IProductRepository ProductRepository { get; }
        void Save();
        void DeleteDatabase();
        void MigrateDatabase();
    }
}
