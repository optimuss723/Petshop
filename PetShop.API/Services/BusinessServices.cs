using System.Data;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PetShop.API.Hubs;
using PetShop.Repository.Data;
using PetShop.Repository.Entities;
using PetShop.Repository.UnitOfWork;
using PetShop.Shared.Constants;
using PetShop.Shared.DTOs.Request;
using PetShop.Shared.DTOs.Response;
using PetShop.Shared.Enums;

namespace PetShop.API.Services;

// ── PRODUCT SERVICE ───────────────────────────────────────────────
public interface IProductService
{
    Task<ApiResponse<List<ProductResponse>>> GetAllAsync();
    Task<ApiResponse<PagedResponse<ProductResponse>>> SearchAsync(string? search = null, string? category = null, decimal? minPrice = null, decimal? maxPrice = null, int page = 1, int pageSize = 8);
    Task<ApiResponse<ProductResponse>> GetByIdAsync(int id);
    Task<ApiResponse<ProductResponse>> CreateAsync(ProductRequest request);
    Task<ApiResponse<ProductResponse>> UpdateAsync(int id, ProductRequest request);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ProductService(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

    public async Task<ApiResponse<List<ProductResponse>>> GetAllAsync()
    {
        var items = await _uow.Products.FindAsync(p => p.IsActive);
        return new ApiResponse<List<ProductResponse>>(true, null, _mapper.Map<List<ProductResponse>>(items));
    }

    public async Task<ApiResponse<PagedResponse<ProductResponse>>> SearchAsync(
        string? search = null,
        string? category = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int page = 1,
        int pageSize = 8)
    {
        var query = _uow.Products.Query().Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(p => p.Name.Contains(s) || p.Description.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<ProductCategory>(category, true, out var catEnum))
        {
            query = query.Where(p => p.Category == catEnum);
        }

        if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);

        var total = await query.CountAsync();
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<ProductResponse>>(items);
        var paged = new PagedResponse<ProductResponse>(dtos, total, safePage, safePageSize);
        return new ApiResponse<PagedResponse<ProductResponse>>(true, null, paged);
    }

    public async Task<ApiResponse<ProductResponse>> GetByIdAsync(int id)
    {
        var item = await _uow.Products.GetByIdAsync(id);
        if (item == null || !item.IsActive)
            return new ApiResponse<ProductResponse>(false, "Sản phẩm không tồn tại.", null);
        return new ApiResponse<ProductResponse>(true, null, _mapper.Map<ProductResponse>(item));
    }

    public async Task<ApiResponse<ProductResponse>> CreateAsync(ProductRequest request)
    {
        var product = _mapper.Map<Product>(request);
        await _uow.Products.AddAsync(product);
        await _uow.SaveChangesAsync();
        return new ApiResponse<ProductResponse>(true, "Tạo sản phẩm thành công.", _mapper.Map<ProductResponse>(product));
    }

    public async Task<ApiResponse<ProductResponse>> UpdateAsync(int id, ProductRequest request)
    {
        var product = await _uow.Products.GetByIdAsync(id);
        if (product == null) return new ApiResponse<ProductResponse>(false, "Sản phẩm không tồn tại.", null);
        _mapper.Map(request, product);
        _uow.Products.Update(product);
        await _uow.SaveChangesAsync();
        return new ApiResponse<ProductResponse>(true, "Cập nhật thành công.", _mapper.Map<ProductResponse>(product));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var product = await _uow.Products.GetByIdAsync(id);
        if (product == null) return new ApiResponse<bool>(false, "Sản phẩm không tồn tại.", false);
        product.IsActive = false;
        _uow.Products.Update(product);
        await _uow.SaveChangesAsync();
        return new ApiResponse<bool>(true, "Xóa sản phẩm thành công.", true);
    }
}

// ── PET SERVICE ───────────────────────────────────────────────────
public interface IPetService
{
    Task<ApiResponse<List<PetResponse>>> GetAllAsync();
    Task<ApiResponse<PagedResponse<PetResponse>>> SearchAsync(string? search = null, string? category = null, decimal? minPrice = null, decimal? maxPrice = null, string? status = null, int page = 1, int pageSize = 8);
    Task<ApiResponse<PetResponse>> GetByIdAsync(int id);
    Task<ApiResponse<PetResponse>> CreateAsync(PetRequest request);
    Task<ApiResponse<PetResponse>> UpdateAsync(int id, PetRequest request);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}

public class PetService : IPetService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public PetService(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

    public async Task<ApiResponse<List<PetResponse>>> GetAllAsync()
    {
        var items = await _uow.Pets.GetAllAsync();
        return new ApiResponse<List<PetResponse>>(true, null, _mapper.Map<List<PetResponse>>(items));
    }

    public async Task<ApiResponse<PagedResponse<PetResponse>>> SearchAsync(
        string? search = null,
        string? category = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? status = null,
        int page = 1,
        int pageSize = 8)
    {
        var query = _uow.Pets.Query();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(p => p.Name.Contains(s) || p.Breed.Contains(s) || p.Description.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<PetCategory>(category, true, out var catEnum))
        {
            query = query.Where(p => p.Category == catEnum);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PetStatus>(status, true, out var statusEnum))
        {
            query = query.Where(p => p.Status == statusEnum);
        }

        if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);

        var total = await query.CountAsync();
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<PetResponse>>(items);
        var paged = new PagedResponse<PetResponse>(dtos, total, safePage, safePageSize);
        return new ApiResponse<PagedResponse<PetResponse>>(true, null, paged);
    }

    public async Task<ApiResponse<PetResponse>> GetByIdAsync(int id)
    {
        var item = await _uow.Pets.GetByIdAsync(id);
        if (item == null) return new ApiResponse<PetResponse>(false, "Thú cưng không tồn tại.", null);
        return new ApiResponse<PetResponse>(true, null, _mapper.Map<PetResponse>(item));
    }

    public async Task<ApiResponse<PetResponse>> CreateAsync(PetRequest request)
    {
        var pet = _mapper.Map<Pet>(request);
        await _uow.Pets.AddAsync(pet);
        await _uow.SaveChangesAsync();
        return new ApiResponse<PetResponse>(true, "Thêm thú cưng thành công.", _mapper.Map<PetResponse>(pet));
    }

    public async Task<ApiResponse<PetResponse>> UpdateAsync(int id, PetRequest request)
    {
        var pet = await _uow.Pets.GetByIdAsync(id);
        if (pet == null) return new ApiResponse<PetResponse>(false, "Thú cưng không tồn tại.", null);
        _mapper.Map(request, pet);
        _uow.Pets.Update(pet);
        await _uow.SaveChangesAsync();
        return new ApiResponse<PetResponse>(true, "Cập nhật thành công.", _mapper.Map<PetResponse>(pet));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var pet = await _uow.Pets.GetByIdAsync(id);
        if (pet == null) return new ApiResponse<bool>(false, "Thú cưng không tồn tại.", false);
        _uow.Pets.Remove(pet);
        await _uow.SaveChangesAsync();
        return new ApiResponse<bool>(true, "Xóa thành công.", true);
    }
}

// ── ORDER SERVICE ─────────────────────────────────────────────────
public interface IOrderService
{
    Task<ApiResponse<List<OrderResponse>>> GetAllAsync(string? status = null);
    Task<ApiResponse<List<OrderResponse>>> GetByUserIdAsync(int userId, string? status = null);
    Task<ApiResponse<OrderResponse>> GetByIdAsync(int id);
    Task<ApiResponse<OrderResponse>> CreateAsync(int userId, OrderRequest request);
    Task<ApiResponse<OrderResponse>> UpdateStatusAsync(int id, OrderStatus status, int requestingUserId, string requestingRole);
    Task<ApiResponse<bool>> CancelAsync(int id, int userId, string? role = null);
    Task<ApiResponse<ShipmentCreatedResponse>> CreateShipmentAsync(int id, string? note);
}

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IHubContext<OrderHub> _hub;
    private readonly PetShopDbContext _db;

    public OrderService(IUnitOfWork uow, IMapper mapper, IHubContext<OrderHub> hub, PetShopDbContext db)
    {
        _uow = uow; _mapper = mapper; _hub = hub; _db = db;
    }

    private static OrderStatus? ParseOrderStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return null;
        var s = status.Trim().ToLowerInvariant();
        if (s == "pending" || s.Contains("chờ") || s.Contains("chua") || s.Contains("chưa"))
            return OrderStatus.Pending;
        if (s == "confirmed" || s.Contains("xác nhận") || s.Contains("xac nhan"))
            return OrderStatus.Confirmed;
        if (s == "shipping" || (s.Contains("giao") && !s.Contains("đã")))
            return OrderStatus.Shipping;
        if (s == "awaitingreceipt")
            return OrderStatus.AwaitingReceipt;
        if (s == "delivered" || s.Contains("đã giao") || s.Contains("hoàn thành"))
            return OrderStatus.Delivered;
        if (s == "cancelled" || s.Contains("huỷ") || s.Contains("huy"))
            return OrderStatus.Cancelled;
        if (Enum.TryParse<OrderStatus>(status, true, out var parsed))
            return parsed;
        return null;
    }

    private async Task RestoreOrderStockAsync(int orderId)
    {
        var items = await _db.OrderItems
            .Where(i => i.OrderId == orderId)
            .ToListAsync();

        foreach (var item in items)
        {
            if (item.ProductId.HasValue && item.Quantity > 0)
            {
                await _db.Products
                    .Where(p => p.Id == item.ProductId.Value)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock + item.Quantity));
            }

            if (item.PetId.HasValue)
            {
                await _db.Pets
                    .Where(p => p.Id == item.PetId.Value)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, PetStatus.Available));
            }
        }
    }

    public async Task<ApiResponse<List<OrderResponse>>> GetAllAsync(string? status = null)
    {
        var query = _uow.Orders.Query()
            .Include(o => o.User)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Items).ThenInclude(i => i.Pet)
            .AsQueryable();

        var parsedStatus = ParseOrderStatus(status);
        if (parsedStatus.HasValue)
        {
            query = query.Where(o => o.Status == parsedStatus.Value);
        }

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return new ApiResponse<List<OrderResponse>>(true, null, _mapper.Map<List<OrderResponse>>(orders));
    }

    public async Task<ApiResponse<List<OrderResponse>>> GetByUserIdAsync(int userId, string? status = null)
    {
        var query = _uow.Orders.Query()
            .Where(o => o.UserId == userId)
            .Include(o => o.User)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Items).ThenInclude(i => i.Pet)
            .AsQueryable();

        var parsedStatus = ParseOrderStatus(status);
        if (parsedStatus.HasValue)
        {
            query = query.Where(o => o.Status == parsedStatus.Value);
        }

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return new ApiResponse<List<OrderResponse>>(true, null, _mapper.Map<List<OrderResponse>>(orders));
    }

    public async Task<ApiResponse<OrderResponse>> GetByIdAsync(int id)
    {
        var order = await _uow.Orders.Query()
            .Include(o => o.User)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Items).ThenInclude(i => i.Pet)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return new ApiResponse<OrderResponse>(false, "Đơn hàng không tồn tại.", null);
        return new ApiResponse<OrderResponse>(true, null, _mapper.Map<OrderResponse>(order));
    }

    public async Task<ApiResponse<OrderResponse>> CreateAsync(int userId, OrderRequest request)
    {
        // ── BƯỚC 1: Kiểm tra sơ bộ (nhanh, không lock) ──────────────
        // Mục đích: trả lỗi sớm với message rõ ràng trước khi vào transaction
        var preCheckItems = new List<(int productId, string name, int qty)>();
        var preCheckPets  = new List<(int petId, string name)>();

        foreach (var item in request.Items)
        {
            if (item.ProductId.HasValue)
            {
                var product = await _uow.Products.GetByIdAsync(item.ProductId.Value);

                if (product == null || !product.IsActive)
                    return new ApiResponse<OrderResponse>(false,
                        $"Sản phẩm ID {item.ProductId} không tồn tại hoặc đã ngừng bán.", null);

                if (product.Stock < item.Quantity)
                    return new ApiResponse<OrderResponse>(false,
                        $"Sản phẩm '{product.Name}' không đủ hàng. " +
                        $"Tồn kho: {product.Stock}, yêu cầu: {item.Quantity}.", null);

                preCheckItems.Add((product.Id, product.Name, item.Quantity));
            }

            if (item.PetId.HasValue)
            {
                var pet = await _uow.Pets.GetByIdAsync(item.PetId.Value);

                if (pet == null)
                    return new ApiResponse<OrderResponse>(false,
                        $"Thú cưng ID {item.PetId} không tồn tại.", null);

                if (pet.Status != PetStatus.Available)
                    return new ApiResponse<OrderResponse>(false,
                        $"Thú cưng '{pet.Name}' hiện không còn để bán (trạng thái: {pet.Status}).", null);

                preCheckPets.Add((pet.Id, pet.Name));
            }
        }

        // ── BƯỚC 2: Atomic transaction — chống race condition ────────
        // Dùng CreateExecutionStrategy() để tương thích với SqlServerRetryingExecutionStrategy
        var strategy = _db.Database.CreateExecutionStrategy();
        ApiResponse<OrderResponse>? txResult = null;

        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                // 2a. Trừ stock Product (atomic)
                foreach (var (productId, name, qty) in preCheckItems)
                {
                    var rowsAffected = await _db.Products
                        .Where(p => p.Id == productId && p.Stock >= qty)
                        .ExecuteUpdateAsync(s =>
                            s.SetProperty(p => p.Stock, p => p.Stock - qty));

                    if (rowsAffected == 0)
                    {
                        await tx.RollbackAsync();
                        txResult = new ApiResponse<OrderResponse>(false,
                            $"Sản phẩm '{name}' vừa hết hàng do người khác đặt trước. " +
                            $"Vui lòng kiểm tra lại giỏ hàng.", null);
                        return;
                    }
                }

                // 2b. Đổi Pet → Sold (atomic, chỉ khi vẫn Available)
                foreach (var (petId, name) in preCheckPets)
                {
                    var rowsAffected = await _db.Pets
                        .Where(p => p.Id == petId && p.Status == PetStatus.Available)
                        .ExecuteUpdateAsync(s =>
                            s.SetProperty(p => p.Status, PetStatus.Sold));

                    if (rowsAffected == 0)
                    {
                        await tx.RollbackAsync();
                        txResult = new ApiResponse<OrderResponse>(false,
                            $"Thú cưng '{name}' vừa được người khác đặt mua. " +
                            $"Vui lòng chọn thú cưng khác.", null);
                        return;
                    }
                }

                // 2c. Tạo đơn hàng
                var order = new Order
                {
                    UserId          = userId,
                    ShippingAddress = request.ShippingAddress,
                    Note            = request.Note,
                    Status          = OrderStatus.Pending,
                    Items           = request.Items.Select(i => new OrderItem
                    {
                        ProductId = i.ProductId,
                        PetId     = i.PetId,
                        Quantity  = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                };
                order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                await tx.CommitAsync();

                await _hub.Clients.Group("Admins")
                    .SendAsync("NewOrder", new { order.Id, order.TotalAmount, CreatedAt = DateTime.UtcNow });

                var created = await GetByIdAsync(order.Id);
                txResult = new ApiResponse<OrderResponse>(true, "Đặt hàng thành công.", created.Data);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        });

        return txResult!;
    }

    public async Task<ApiResponse<OrderResponse>> UpdateStatusAsync(int id, OrderStatus status, int requestingUserId, string requestingRole)
    {
        var order = await _uow.Orders.GetByIdAsync(id);
        if (order == null) return new ApiResponse<OrderResponse>(false, "Đơn hàng không tồn tại.", null);

        if (requestingRole == "Customer" && requestingUserId != order.UserId)
            return new ApiResponse<OrderResponse>(false, "Không có quyền.", null);

        var previousStatus = order.Status;
        if (previousStatus != OrderStatus.Cancelled && status == OrderStatus.Cancelled)
        {
            await RestoreOrderStockAsync(order.Id);
        }

        order.Status    = status;
        order.UpdatedAt = DateTime.UtcNow;
        _uow.Orders.Update(order);
        await _uow.SaveChangesAsync();

        await _hub.Clients.User(order.UserId.ToString())
            .SendAsync("OrderStatusChanged", new { order.Id, Status = status.ToString() });
        await _hub.Clients.Group($"User_{order.UserId}")
            .SendAsync("OrderStatusChanged", new { order.Id, Status = status.ToString() });
        await _hub.Clients.Group("Admins")
            .SendAsync("OrderStatusChanged", new { order.Id, Status = status.ToString() });

        return await GetByIdAsync(id);
    }

    public async Task<ApiResponse<bool>> CancelAsync(int id, int userId, string? role = null)
    {
        var order = await _uow.Orders.GetByIdAsync(id);
        if (order == null) return new ApiResponse<bool>(false, "Đơn hàng không tồn tại.", false);

        bool isStaffOrAdmin = role == Roles.Admin || role == Roles.Staff;
        if (!isStaffOrAdmin && order.UserId != userId)
            return new ApiResponse<bool>(false, "Không có quyền.", false);

        if (order.Status == OrderStatus.Cancelled)
            return new ApiResponse<bool>(false, "Đơn hàng đã được huỷ trước đó.", false);

        if (order.Status == OrderStatus.Delivered)
            return new ApiResponse<bool>(false, "Không thể huỷ đơn hàng đã giao thành công.", false);

        if (!isStaffOrAdmin && order.Status != OrderStatus.Pending)
            return new ApiResponse<bool>(false, "Chỉ có thể huỷ đơn hàng đang chờ xử lý.", false);

        await RestoreOrderStockAsync(order.Id);

        order.Status    = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        _uow.Orders.Update(order);
        await _uow.SaveChangesAsync();

        await _hub.Clients.User(order.UserId.ToString())
            .SendAsync("OrderStatusChanged", new { order.Id, Status = "Cancelled" });
        await _hub.Clients.Group($"User_{order.UserId}")
            .SendAsync("OrderStatusChanged", new { order.Id, Status = "Cancelled" });
        await _hub.Clients.Group("Admins")
            .SendAsync("OrderStatusChanged", new { order.Id, Status = "Cancelled" });

        return new ApiResponse<bool>(true, "Huỷ đơn hàng thành công, số lượng đã được hoàn lại kho hàng.", true);
    }

    public async Task<ApiResponse<ShipmentCreatedResponse>> CreateShipmentAsync(int id, string? note)
    {
        var order = await _uow.Orders.GetByIdAsync(id);
        if (order == null) return new ApiResponse<ShipmentCreatedResponse>(false, "Đơn hàng không tồn tại.", null);

        order.Status = OrderStatus.Shipping;
        order.UpdatedAt = DateTime.UtcNow;

        var trackingCode = $"GHN{DateTime.UtcNow:yyMMdd}{order.Id:D4}";
        var provider = "Giao Hàng Nhanh (GHN)";
        var trackingUrl = $"https://donhang.ghn.vn/?code={trackingCode}";

        var trackingNote = $"[Mã vận đơn: {trackingCode} | {provider}]";
        if (string.IsNullOrWhiteSpace(order.Note))
            order.Note = trackingNote;
        else if (!order.Note.Contains(trackingCode))
            order.Note = $"{order.Note} - {trackingNote}";

        _uow.Orders.Update(order);
        await _uow.SaveChangesAsync();

        await _hub.Clients.Group("Admins")
            .SendAsync("ShipmentCreated", new { orderId = order.Id, trackingCode });
        await _hub.Clients.Group("Admins")
            .SendAsync("OrderStatusChanged", new { order.Id, Status = "Shipping" });
        await _hub.Clients.User(order.UserId.ToString())
            .SendAsync("OrderStatusChanged", new { order.Id, Status = "Shipping" });
        await _hub.Clients.Group($"User_{order.UserId}")
            .SendAsync("OrderStatusChanged", new { order.Id, Status = "Shipping" });

        var response = new ShipmentCreatedResponse(
            trackingCode,
            provider,
            trackingUrl,
            $"Đã tạo vận đơn {trackingCode} thành công. Đơn hàng #{order.Id} đã chuyển sang Đang giao."
        );

        return new ApiResponse<ShipmentCreatedResponse>(true, "Tạo vận đơn thành công.", response);
    }
}

// ── USER SERVICE ──────────────────────────────────────────────────
public interface IUserService
{
    Task<ApiResponse<List<UserResponse>>> GetAllAsync();
    Task<ApiResponse<UserResponse>> GetByIdAsync(int id);
    Task<ApiResponse<UserResponse>> UpdateAsync(int id, UserUpdateRequest request);
    Task<ApiResponse<bool>> DeactivateAsync(int id);
}

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

    public async Task<ApiResponse<List<UserResponse>>> GetAllAsync()
    {
        var users = await _uow.Users.GetAllAsync();
        return new ApiResponse<List<UserResponse>>(true, null, _mapper.Map<List<UserResponse>>(users));
    }

    public async Task<ApiResponse<UserResponse>> GetByIdAsync(int id)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        if (user == null) return new ApiResponse<UserResponse>(false, "Người dùng không tồn tại.", null);
        return new ApiResponse<UserResponse>(true, null, _mapper.Map<UserResponse>(user));
    }

    public async Task<ApiResponse<UserResponse>> UpdateAsync(int id, UserUpdateRequest request)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        if (user == null) return new ApiResponse<UserResponse>(false, "Người dùng không tồn tại.", null);
        user.FullName = request.FullName;
        user.Phone    = request.Phone;
        user.Address  = request.Address;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();
        return new ApiResponse<UserResponse>(true, "Cập nhật thành công.", _mapper.Map<UserResponse>(user));
    }

    public async Task<ApiResponse<bool>> DeactivateAsync(int id)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        if (user == null) return new ApiResponse<bool>(false, "Người dùng không tồn tại.", false);
        user.IsActive = false;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();
        return new ApiResponse<bool>(true, "Vô hiệu hoá tài khoản thành công.", true);
    }
}
