using System.ComponentModel.DataAnnotations;
using PetShop.Shared.Enums;

namespace PetShop.Shared.DTOs.Request;

/// <summary>Request Đăng nhập</summary>
public record LoginRequest(
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    string Email = "",
    
    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    string Password = "")
{
    public LoginRequest() : this(string.Empty, string.Empty) { }
}

/// <summary>Request Đăng ký tài khoản</summary>
public record RegisterRequest(
    [Required(ErrorMessage = "Tên đầy đủ không được để trống")]
    [MinLength(2, ErrorMessage = "Tên phải có ít nhất 2 ký tự")]
    [StringLength(100, ErrorMessage = "Tên tối đa 100 ký tự")]
    string FullName = "",
    
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(150, ErrorMessage = "Email tối đa 150 ký tự")]
    string Email = "",
    
    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    string Password = "",
    
    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự")]
    string Phone = "",
    
    [StringLength(300, ErrorMessage = "Địa chỉ tối đa 300 ký tự")]
    string? Address = null)
{
    public RegisterRequest() : this(string.Empty, string.Empty, string.Empty, string.Empty, null) { }
}

/// <summary>Đổi mật khẩu</summary>
public record ChangePasswordRequest(
    [Required(ErrorMessage = "Mật khẩu hiện tại không được để trống")]
    string CurrentPassword,
    [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
    string NewPassword);

/// <summary>Request Tạo / Cập nhật Sản phẩm (Thành viên 1 phụ trách) - Data Annotations</summary>
public record ProductRequest(
    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên sản phẩm phải từ 2 đến 200 ký tự")]
    string Name,

    [Required(ErrorMessage = "Mô tả sản phẩm không được để trống")]
    [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự")]
    string Description,

    [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
    [Range(0.01, 1000000000, ErrorMessage = "Giá sản phẩm phải lớn hơn 0")]
    decimal Price,

    [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc")]
    [Range(0, 100000, ErrorMessage = "Số lượng tồn kho phải từ 0 trở lên")]
    int Stock,

    [Required(ErrorMessage = "Danh mục sản phẩm là bắt buộc")]
    ProductCategory Category,

    [StringLength(500, ErrorMessage = "URL hình ảnh tối đa 500 ký tự")]
    string? ImageUrl)
{
    public ProductRequest() : this(string.Empty, string.Empty, 0, 0, ProductCategory.Food, null) { }
}

/// <summary>Request Tạo / Cập nhật Thú cưng (Thành viên 2 phụ trách) - Data Annotations</summary>
public record PetRequest(
    [Required(ErrorMessage = "Tên thú cưng không được để trống")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên thú cưng phải từ 2 đến 100 ký tự")]
    string Name,

    [Required(ErrorMessage = "Giống thú cưng không được để trống")]
    [StringLength(100, ErrorMessage = "Giống thú cưng tối đa 100 ký tự")]
    string Breed,

    [Required(ErrorMessage = "Loài thú cưng là bắt buộc")]
    PetCategory Category,

    [Required(ErrorMessage = "Tuổi (tháng) là bắt buộc")]
    [Range(0, 300, ErrorMessage = "Tuổi thú cưng phải từ 0 đến 300 tháng")]
    int AgeMonths,

    [Required(ErrorMessage = "Giá bán là bắt buộc")]
    [Range(0.01, 1000000000, ErrorMessage = "Giá bán phải lớn hơn 0")]
    decimal Price,

    [Required(ErrorMessage = "Mô tả thú cưng không được để trống")]
    [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự")]
    string Description,

    [StringLength(500, ErrorMessage = "URL hình ảnh tối đa 500 ký tự")]
    string? ImageUrl,

    [Required(ErrorMessage = "Trạng thái thú cưng là bắt buộc")]
    PetStatus Status)
{
    public PetRequest() : this(string.Empty, string.Empty, PetCategory.Dog, 0, 0, string.Empty, null, PetStatus.Available) { }
}

/// <summary>Request Đặt hàng - Data Annotations</summary>
public class OrderRequest
{
    [Required(ErrorMessage = "Danh sách sản phẩm trong đơn là bắt buộc")]
    [MinLength(1, ErrorMessage = "Đơn hàng phải có ít nhất 1 sản phẩm")]
    public List<OrderItemRequest> Items { get; set; } = [];

    [Required(ErrorMessage = "Địa chỉ nhận hàng không được để trống")]
    [StringLength(500, ErrorMessage = "Địa chỉ nhận hàng tối đa 500 ký tự")]
    public string ShippingAddress { get; set; } = "";

    [StringLength(1000, ErrorMessage = "Ghi chú tối đa 1000 ký tự")]
    public string? Note { get; set; }

    public bool IsCod { get; set; } = true;

    public OrderRequest() { }

    public OrderRequest(List<OrderItemRequest> Items, string ShippingAddress, string? Note = null, bool IsCod = true)
    {
        this.Items = Items;
        this.ShippingAddress = ShippingAddress;
        this.Note = Note;
        this.IsCod = IsCod;
    }
}

/// <summary>Request Chi tiết món hàng - Data Annotations</summary>
public class OrderItemRequest
{
    public int? ProductId { get; set; }
    public int? PetId { get; set; }

    [Required(ErrorMessage = "Số lượng là bắt buộc")]
    [Range(1, 1000, ErrorMessage = "Số lượng phải từ 1 đến 1000")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Đơn giá là bắt buộc")]
    [Range(0.01, 1000000000, ErrorMessage = "Đơn giá phải lớn hơn 0")]
    public decimal UnitPrice { get; set; }

    public OrderItemRequest() { }

    public OrderItemRequest(int? ProductId, int? PetId, int Quantity, decimal UnitPrice)
    {
        this.ProductId = ProductId;
        this.PetId = PetId;
        this.Quantity = Quantity;
        this.UnitPrice = UnitPrice;
    }
}

/// <summary>Request Cập nhật trạng thái đơn hàng</summary>
public record UpdateOrderStatusRequest(
    [Required(ErrorMessage = "Trạng thái không được để trống")]
    string Status)
{
    public UpdateOrderStatusRequest() : this(string.Empty) { }
}

/// <summary>Request tạo vận đơn giao hàng</summary>
public class ShipmentRequest
{
    public string? Note { get; set; }
}

/// <summary>Request Cập nhật thông tin người dùng - Data Annotations</summary>
public record UserUpdateRequest(
    [Required(ErrorMessage = "Họ tên không được để trống")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 100 ký tự")]
    string FullName,

    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự")]
    string Phone,

    [StringLength(300, ErrorMessage = "Địa chỉ tối đa 300 ký tự")]
    string? Address)
{
    public UserUpdateRequest() : this(string.Empty, string.Empty, null) { }
}

/// <summary>Gửi đánh giá sản phẩm / thú cưng</summary>
public record ReviewCreateRequest(
    int?   ProductId,
    int?   PetId,
    [Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5")]
    int    Rating,
    [Required(ErrorMessage = "Nội dung đánh giá không được để trống")]
    [StringLength(1000, ErrorMessage = "Nội dung tối đa 1000 ký tự")]
    string Comment)
{
    public ReviewCreateRequest() : this(null, null, 5, string.Empty) { }
}

/// <summary>Tạo / cập nhật danh mục</summary>
public record CategoryCreateRequest(
    [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên danh mục tối đa 100 ký tự")]
    string  Name,

    [Required(ErrorMessage = "Loại danh mục là bắt buộc")]
    [StringLength(50)]
    string  Type,

    [StringLength(500)]
    string? Description,

    [StringLength(100)]
    string? Icon)
{
    public CategoryCreateRequest() : this(string.Empty, string.Empty, null, null) { }
}
