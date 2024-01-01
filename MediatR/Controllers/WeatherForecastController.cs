using Mediatr.Api.Context;
using Mediatr.Api.Filters.ActionFilters;
using Mediatr.Api.MongoModels;
using MediatR.Application.CQRS.Products.Query;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace MediatR.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly IGenericMongoRepository<People> _dbContext;

	public ProductsController(IMediator mediator, IGenericMongoRepository<People> dbContext) 
		=> (_dbContext, _mediator) = (dbContext, mediator);

	[HttpGet]
	[CachedFilter]
	public async Task<ActionResult> GetProducts([FromQuery]GetProductsQuery query)
	{
		var result = await _mediator.Send(query);
		return Ok(result);
	}

	[HttpGet("p")]
	public async Task<ActionResult<IReadOnlyList<People>>> GetPeopleFromMongo()
	{

		return Ok(await _dbContext.GetAll());
	}

	[HttpPost]
	public async Task<ActionResult> UpdatePeople()
	{
        var client = new MongoClient("mongodb://localhost:27017");
        var db = client.GetDatabase("test_database");

        var collectionReference = db.GetCollection<People>("persons");
		var firstPeople = await collectionReference.Find(Builders<People>.Filter.Empty).FirstAsync();

		firstPeople.Address = new Address()
		{
			City = "Wroclaw",
			Street = "Prusa",
			Building = 15
		};

		await collectionReference.FindOneAndUpdateAsync(Builders<People>.Filter.Eq(p => p.Id, "64c57f58644ab63f9ab8d51e"),
													    Builders<People>.Update.Set(x => x.Age, 23));


		var pipeline =
		await collectionReference.Aggregate().Lookup<Invoice, People>(
												Invoice.MongoCollectionName,	
												new ExpressionFieldDefinition<People, string>(x => x.Id),
												new ExpressionFieldDefinition<Invoice, string>(x => x.PeopleId),
												new ExpressionFieldDefinition<People, Invoice[]>(x => x.Invoices)
											 ).ToListAsync();

		return NoContent();
    }
}
