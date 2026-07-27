using PetShop.Repository.Data;
using PetShop.Repository.Entities;
using PetShop.Repository.Repositories;

namespace PetShop.Repository.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IRepository<User>      Users      { get; }
    IRepository<Product>   Products   { get; }
    IRepository<Pet>       Pets       { get; }
    IRepository<Order>     Orders     { get; }
    IRepository<OrderItem> OrderItems { get; }
    Task<int> SaveChangesAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly PetShopDbContext _context;

    public IRepository<User>      Users      { get; }
    public IRepository<Product>   Products   { get; }
    public IRepository<Pet>       Pets       { get; }
    public IRepository<Order>     Orders     { get; }
    public IRepository<OrderItem> OrderItems { get; }

    public UnitOfWork(PetShopDbContext context,
        IRepository<User>      users,
        IRepository<Product>   products,
        IRepository<Pet>       pets,
        IRepository<Order>     orders,
        IRepository<OrderItem> orderItems)
    {
        _context   = context;
        Users      = users;
        Products   = products;
        Pets       = pets;
        Orders     = orders;
        OrderItems = orderItems;
    }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}
