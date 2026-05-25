using AutoMapper;
using Category.Api.Domain;
using Category.Api.Infrastructure;
using Category.Api.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<MiniApiDbContext>(option =>
option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddValidation();
builder.Services.AddProblemDetails();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.CreateMap<Product, ProductResponseDto>().ReverseMap();

    cfg.CreateMap<CreateProductDto, Product>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.AddedDate, opt => opt.Ignore());

    cfg.CreateMap<UpdateProductDto, Product>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.AddedDate, opt => opt.Ignore());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();
app.UseHttpsRedirection();

app.MapGet("/api/products", async (MiniApiDbContext db, ILogger<Program> logger) =>
{
    logger.LogInformation("Fetching all products");
    var products = await db.Products.ToListAsync();
    logger.LogInformation("Fetched {Count} products", products.Count);
    return Results.Ok(products);
}).WithName("GetAllProducts");

app.MapGet("/api/products/{id:int}", async (int id, MiniApiDbContext db, ILogger<Program> logger) =>
{
    logger.LogInformation("Fetching product with Id: {ProductId}", id);
    var product = await db.Products.FirstOrDefaultAsync(u => u.Id == id);
    if (product == null)
    {
        logger.LogWarning("Product with Id {ProductId} not found", id);
        return Results.NotFound();
    }
    else
    {
        logger.LogInformation("Product with Id {ProductId} retrieved successfully", id);
        return Results.Ok(product);
    }
}).WithName("GetProductById");

app.MapPost("/api/products", async (CreateProductDto createProductDto, MiniApiDbContext db, ILogger<Program> logger, IMapper mapper) =>
{
    logger.LogInformation("Creating a new product with Name: {Name}", createProductDto.Name);
    var product = mapper.Map<Product>(createProductDto);
    product.AddedDate = DateTime.Now;

    db.Products.Add(product);
    await db.SaveChangesAsync();
    logger.LogInformation("Product created successfully with Id: {ProductId}", product.Id);

    var productDto = mapper.Map<ProductResponseDto>(product);
    return Results.CreatedAtRoute("GetProductById", new { id = product.Id }, productDto);
}).WithName("CreateProduct")
.Accepts<CreateProductDto>("application/json")
.Produces<ProductResponseDto>(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status400BadRequest);

app.MapPut("/api/products/{id:int}", async (int id, UpdateProductDto updateProductDto, MiniApiDbContext db, ILogger<Program> logger, IMapper mapper) =>
{
    logger.LogInformation("Updating product with Id: {ProductId}", id);
    var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id);

    if (product == null)
    {
        logger.LogError("Update failed! Product with Id {ProductId} not found", id);
        return Results.NotFound();
    }
    else
    {
        mapper.Map(updateProductDto, product);
        await db.SaveChangesAsync();
        logger.LogInformation("Product with Id {ProductId} was updated successfully", id);
        return Results.Ok(product);
    }
}).WithName("UpdateProduct");

app.MapDelete("/api/products/{id:int}", async (int id, MiniApiDbContext db, ILogger<Program> logger) =>
{
    logger.LogInformation("Attempting to delete product with Id {ProductId}", id);
    var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id);

    if (product == null)
    {
        logger.LogError("Delete failed! Product with Id {ProductId} was not found", id);
        return Results.NotFound();
    }
    else
    {
        db.Products.Remove(product);
        await db.SaveChangesAsync();
        logger.LogInformation("Product with Id {ProductId} deleted successfully", id);
        return Results.NoContent();
    }
}).WithName("DeleteProduct");

app.Run();