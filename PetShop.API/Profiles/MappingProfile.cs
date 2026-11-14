using AutoMapper;
using PetShop.Repository.Entities;
using PetShop.Shared.DTOs.Request;
using PetShop.Shared.DTOs.Response;

namespace PetShop.API.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User,      UserResponse>().ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));
        CreateMap<Product,   ProductResponse>().ForMember(d => d.Category, o => o.MapFrom(s => s.Category.ToString()));
        CreateMap<Pet,       PetResponse>()
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Category.ToString()))
            .ForMember(d => d.Status,   o => o.MapFrom(s => s.Status.ToString()));
        CreateMap<Order,     OrderResponse>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.CustomerPhone, o => o.MapFrom(s => s.User.Phone))
            .ForMember(d => d.Status,   o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.TrackingCode, o => o.MapFrom(s =>
                s.Status == PetShop.Shared.Enums.OrderStatus.Shipping || s.Status == PetShop.Shared.Enums.OrderStatus.Delivered || s.Status == PetShop.Shared.Enums.OrderStatus.AwaitingReceipt
                    ? (s.Note != null && System.Text.RegularExpressions.Regex.IsMatch(s.Note, @"GHN\d+")
                        ? System.Text.RegularExpressions.Regex.Match(s.Note, @"GHN\d+").Value
                        : $"GHN{s.CreatedAt:yyMMdd}{s.Id:D4}")
                    : null))
            .ForMember(d => d.ShippingProvider, o => o.MapFrom(s =>
                s.Status == PetShop.Shared.Enums.OrderStatus.Shipping || s.Status == PetShop.Shared.Enums.OrderStatus.Delivered || s.Status == PetShop.Shared.Enums.OrderStatus.AwaitingReceipt
                    ? "Giao Hàng Nhanh (GHN)"
                    : null))
            .ForMember(d => d.TrackingUrl, o => o.MapFrom(s =>
                s.Status == PetShop.Shared.Enums.OrderStatus.Shipping || s.Status == PetShop.Shared.Enums.OrderStatus.Delivered || s.Status == PetShop.Shared.Enums.OrderStatus.AwaitingReceipt
                    ? $"https://donhang.ghn.vn/?code=GHN{s.CreatedAt:yyMMdd}{s.Id:D4}"
                    : null));
        CreateMap<OrderItem, OrderItemResponse>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : null))
            .ForMember(d => d.PetName,     o => o.MapFrom(s => s.Pet     != null ? s.Pet.Name     : null));

        CreateMap<ProductRequest, Product>();
        CreateMap<ProductRequest, Product>().ReverseMap();
        CreateMap<PetRequest,     Pet>();
        CreateMap<PetRequest,     Pet>().ReverseMap();
    }
}
