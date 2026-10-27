using AutoMapper;
using Catalog.Service.DTOs;
using Catalog.Service.Entities;

namespace Catalog.Service.Profiles;

/// <summary>
/// AutoMapper mapping profile cho Catalog.Service.
/// Map giữa Entity ↔ DTO (Request/Response).
/// </summary>
public class CatalogMappingProfile : Profile
{
    public CatalogMappingProfile()
    {
        // ── Product mappings ────────────────────────────────────────
        CreateMap<Product, ProductResponse>();
        CreateMap<ProductCreateRequest, Product>()
            .ForMember(d => d.Id,        o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.IsActive,  o => o.Ignore());
        CreateMap<ProductUpdateRequest, Product>()
            .ForMember(d => d.Id,        o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.IsActive,  o => o.Ignore());

        // ── Pet mappings ────────────────────────────────────────────
        CreateMap<Pet, PetResponse>();
        CreateMap<PetCreateRequest, Pet>()
            .ForMember(d => d.Id,        o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore());
        CreateMap<PetUpdateRequest, Pet>()
            .ForMember(d => d.Id,        o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore());

        // ── Review mappings ─────────────────────────────────────────
        CreateMap<Review, ReviewResponse>();

        // ── Category mappings ───────────────────────────────────────
        CreateMap<Category, CategoryResponse>();
        CreateMap<CategoryCreateRequest, Category>()
            .ForMember(d => d.Id, o => o.Ignore());
    }
}
