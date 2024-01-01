using MediatR.Application.CQRS.Products.Dto;

namespace MediatR.Application.CQRS.Products.Query
{
    public class GetProductsQuery : IRequest<ProductDto>
    {
        public string? SearchQuery { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
