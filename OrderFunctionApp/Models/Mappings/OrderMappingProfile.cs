using AutoMapper;
using OrderFunctionApp.Models.DTOs;

namespace OrderFunctionApp.Models.Mappings
{
    /// <summary>
    /// AutoMapper profile for mapping between domain entities and DTOs.
    /// This profile handles conversions between Order, Customer, OrderLine entities
    /// and their corresponding DTOs as well as ERP DTOs.
    /// </summary>
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            // Map Order entity to OrderDTO
            CreateMap<Order, OrderDTO>()
                .ReverseMap();

            // Map OrderLine entity to OrderLineDTO
            CreateMap<OrderLine, OrderLineDTO>()
                .ReverseMap();

            // Map Customer entity to CustomerDTO
            CreateMap<Customer, CustomerDTO>()
                .ReverseMap();

            // Map ERP Order DTO to Order entity
            CreateMap<ERPOrderDTO, Order>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.ExternalOrderId))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.ExternalCustomerId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => OrderStatus.Pending))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerDatabaseId, opt => opt.Ignore())
                .ForMember(dest => dest.OrderLines, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore());

            // Map ERP Order Line DTO to OrderLine entity
            CreateMap<ERPOrderLineDTO, OrderLine>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit ?? "pcs"))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.LineTotal))
                .ForMember(dest => dest.DiscountPercentage, opt => opt.MapFrom(src => src.DiscountPercentage))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrderId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Order, opt => opt.Ignore());

            // Map ERP Order DTO to Customer entity (for customer creation/update)
            CreateMap<ERPOrderDTO, Customer>()
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.ExternalCustomerId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.CustomerName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.CustomerEmail))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Phone, opt => opt.Ignore())
                .ForMember(dest => dest.RegistrationNumber, opt => opt.Ignore())
                .ForMember(dest => dest.BillingAddress, opt => opt.Ignore())
                .ForMember(dest => dest.ShippingAddress, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore());
        }
    }
}
