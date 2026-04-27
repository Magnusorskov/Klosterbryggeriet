namespace BlazorApp.Models.Dtos;

public class ProductCreated
{
    public required int OctopusId { get; set; }
    public required string OctopusTitle { get; set; }
    public required int Available { get; set; }
    public ProductStatus Status => Product.StatusFor(Available);
}
