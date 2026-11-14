using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PetShop.Shared.Enums;

namespace PetShop.Repository.Entities;

/// <summary>
/// Thực thể Người dùng (Tài khoản) - Sử dụng Data Annotations
/// </summary>
[Table("Users")]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Họ tên không được để trống")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 100 ký tự")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [StringLength(150, ErrorMessage = "Email tối đa 150 ký tự")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu băm là bắt buộc")]
    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự")]
    public string Phone { get; set; } = null!;

    [StringLength(300, ErrorMessage = "Địa chỉ tối đa 300 ký tự")]
    public string? Address { get; set; }

    [Required]
    public UserRole Role { get; set; } = UserRole.Customer;

    [StringLength(255)]
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiry { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public bool IsActive { get; set; } = true;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

/// <summary>
/// Thực thể Sản phẩm (Thành viên 1 phụ trách) - Sử dụng Data Annotations
/// </summary>
[Table("Products")]
public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên sản phẩm phải từ 2 đến 200 ký tự")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Mô tả sản phẩm không được để trống")]
    [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
    [Range(0.01, 1000000000, ErrorMessage = "Giá sản phẩm phải lớn hơn 0")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc")]
    [Range(0, 100000, ErrorMessage = "Số lượng tồn kho phải từ 0 trở lên")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "Danh mục sản phẩm là bắt buộc")]
    public ProductCategory Category { get; set; }

    [StringLength(500, ErrorMessage = "URL hình ảnh tối đa 500 ký tự")]
    public string? ImageUrl { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public bool IsActive { get; set; } = true;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

/// <summary>
/// Thực thể Thú cưng (Thành viên 2 phụ trách) - Sử dụng Data Annotations
/// </summary>
[Table("Pets")]
public class Pet
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên thú cưng không được để trống")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên thú cưng phải từ 2 đến 100 ký tự")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Giống thú cưng không được để trống")]
    [StringLength(100, ErrorMessage = "Giống thú cưng tối đa 100 ký tự")]
    public string Breed { get; set; } = null!;

    [Required(ErrorMessage = "Loài thú cưng là bắt buộc")]
    public PetCategory Category { get; set; }

    [Required(ErrorMessage = "Tuổi thú cưng là bắt buộc")]
    [Range(0, 300, ErrorMessage = "Tuổi (tháng) phải từ 0 đến 300")]
    public int AgeMonths { get; set; }

    [Required(ErrorMessage = "Giá bán là bắt buộc")]
    [Range(0.01, 1000000000, ErrorMessage = "Giá bán phải lớn hơn 0")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Mô tả thú cưng không được để trống")]
    [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự")]
    public string Description { get; set; } = null!;

    [StringLength(500, ErrorMessage = "URL hình ảnh tối đa 500 ký tự")]
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Trạng thái thú cưng là bắt buộc")]
    public PetStatus Status { get; set; } = PetStatus.Available;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

/// <summary>
/// Thực thể Đơn hàng - Sử dụng Data Annotations
/// </summary>
[Table("Orders")]
public class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Địa chỉ nhận hàng không được để trống")]
    [StringLength(500, ErrorMessage = "Địa chỉ nhận hàng tối đa 500 ký tự")]
    public string ShippingAddress { get; set; } = null!;

    [StringLength(1000, ErrorMessage = "Ghi chú tối đa 1000 ký tự")]
    public string? Note { get; set; }

    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Required]
    [Range(0, 1000000000, ErrorMessage = "Tổng tiền phải >= 0")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

/// <summary>
/// Thực thể Chi tiết đơn hàng - Sử dụng Data Annotations
/// </summary>
[Table("OrderItems")]
public class OrderItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Order))]
    public int OrderId { get; set; }

    [ForeignKey(nameof(Product))]
    public int? ProductId { get; set; }

    [ForeignKey(nameof(Pet))]
    public int? PetId { get; set; }

    [Required]
    [Range(1, 1000, ErrorMessage = "Số lượng phải từ 1 đến 1000")]
    public int Quantity { get; set; }

    [Required]
    [Range(0.01, 1000000000, ErrorMessage = "Đơn giá phải lớn hơn 0")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    public Order Order { get; set; } = null!;
    public Product? Product { get; set; }
    public Pet? Pet { get; set; }
}
