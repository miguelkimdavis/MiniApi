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

app.MapGet("/api/products", async (MiniApiDbContext db) =>
{
    var products = await db.Products.ToListAsync();
    return Results.Ok(products);
}).WithName("GetAllProducts");

app.MapGet("/api/products/{id:int}", async (int id, MiniApiDbContext db) =>
{
    var product = await db.Products.FirstOrDefaultAsync(u => u.Id == id);
    if(product == null)
    {
        return Results.NotFound();
    } 
    else
    {
        return Results.Ok(product);
    }
}).WithName("GetProductById");

app.MapPost("/api/products", async (CreateProductDto createProductDto, MiniApiDbContext db, IMapper mapper) =>
{
    var product = mapper.Map<Product>(createProductDto);
    product.AddedDate = DateTime.Now;

    db.Products.Add(product);
    await db.SaveChangesAsync();

    var productDto = mapper.Map<ProductResponseDto>(product);
    return Results.CreatedAtRoute("GetProductById", new {id = product.Id}, productDto);
}).WithName("CreateProduct").Accepts<CreateProductDto>("application/json")
.Produces<ProductResponseDto>(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status400BadRequest);

app.MapPut("/api/products/{id:int}", async (int id, UpdateProductDto updateProductDto, MiniApiDbContext db, IMapper mapper) =>
{
    var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id);

    if (product == null)
    {
        return Results.NotFound();
    }
    else
    {
        mapper.Map(updateProductDto, product);
        await db.SaveChangesAsync();
        return Results.Ok(product);
    }
}).WithName("UpdateProduct");

app.MapDelete("/api/products/{id:int}", async (int id, MiniApiDbContext db) =>
{
    var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id);

    if (product == null)
    {
        return Results.NotFound();
    }
    else
    {
        db.Products.Remove(product);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}).WithName("DeleteProduct");

app.Run();