using AutoMapper;
using MediatR.Application.CQRS.Products.Dto;
using MediatR.Application.CQRS.Products.Query;

namespace MediatR.Application.CQRS.Products.QueryHandlers
{
    public class Product
    {
        public Product(int id, string name, string author)
        {
            Id = id;
            Name = name;
            Author = author;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
    }

    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, ProductDto>
    {
        private readonly IMapper _mapper;

        public GetProductsQueryHandler(IMapper mapper)
        {
            _mapper = mapper;
        }

        public Task<ProductDto> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var data = new List<Product>()
            {
                new(1, "A", "B"),
                new(2, "B", "C"),
            };

            var results = data.Where(x => x.Name.Contains(request.SearchQuery!) ||
                                          x.Author.Contains(request.SearchQuery!)).First();

            return Task.FromResult(_mapper.Map<ProductDto>(results));
        }
    }
}
