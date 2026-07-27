using System.ComponentModel.DataAnnotations;
using PetShop.Shared.Enums;

namespace PetShop.Shared.DTOs.Request;

public record LoginRequest(
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    string Email,
    
    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    string Password);

public record RegisterRequest(
    [Required(ErrorMessage = "Tên đầy đủ không được để trống")]
    [MinLength(2, ErrorMessage = "Tên phải có ít nhất 2 ký tự")]
    string FullName,
    
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    string Email,
    
    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    string Password,
    
    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    string Phone,
    
    string? Address);

/// <summary>Đổi mật khẩu (#6)</summary>
public record ChangePasswordRequest(
    [Required(ErrorMessage = "Mật khẩu hiện tại không được để trống")]
    string CurrentPassword,
    [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
    string NewPassword);

public record ProductRequest(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    ProductCategory Category,
    string? ImageUrl);

public record PetRequest(
    string Name,
    string Breed,
    PetCategory Category,
    int AgeMonths,
    decimal Price,
    string Description,
    string? ImageUrl,
    PetStatus Status);

public record OrderRequest(
    List<OrderItemRequest> Items,
    string ShippingAddress,
    string? Note,
    bool IsCod = true);

public record OrderItemRequest(
    int? ProductId,
    int? PetId,
    int Quantity,
    decimal UnitPrice);

public record UpdateOrderStatusRequest(string Status);

public record UserUpdateRequest(
    string FullName,
    string Phone,
    string ? Address);

/// <summary>Gửi đánh giá sản phẩm / thú cưng (#14)</summary>
public record ReviewCreateRequest(
    int?   ProductId,
    int?   PetId,
    [Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5")]
    int    Rating,
    [Required(ErrorMessage = "Nội dung đánh giá không được để trống")]
    string Comment);

/// <summary>Tạo / cập nhật danh mục (#10)</summary>
public record CategoryCreateRequest(
    [Required] string  Name,
    [Required] string  Type,
    string?            Description,
    string?            Icon);
