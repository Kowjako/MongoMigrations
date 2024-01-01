namespace MediatR.Application.CQRS.Products.Dto;

public record ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string Author { get; init; } = null!;

    public ProductDto(int id, string name, string author)
    {
        Id = id;
        Name = name;
        Author = author;
    }
}
