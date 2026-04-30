namespace BlazorApp.Models.Dtos;

public class ProductCreated
{
    public required int OctopusId { get; set; }
    public required string OctopusTitle { get; set; }
    public required int Available { get; set; }
    public PendingProductKind Kind { get; set; } = PendingProductKind.RegularProduct;
    public ProductStatus Status => Kind == PendingProductKind.DraftBeer
        ? DraftBeer.StatusFor(Available)
        : Product.StatusFor(Available);
}
