using FluentValidation;
using MediatR.Application.CQRS.Products.Query;

namespace MediatR.Application.CQRS.Products.Validators
{
    public class GetProductQueryValidator : AbstractValidator<GetProductsQuery>
    {
        public GetProductQueryValidator()
        {
            RuleFor(x => x.SearchQuery).NotNull()
                                       .NotEmpty()
                                       .WithMessage("SearchQuery cannot be null");

            RuleFor(x => x.PageIndex).GreaterThan(0).WithMessage("Page index must be greater than 0");
            RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("Page size must be greater than 0");
        }
    }
}
