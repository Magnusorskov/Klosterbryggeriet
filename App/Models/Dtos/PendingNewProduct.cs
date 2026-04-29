namespace BlazorApp.Models.Dtos;

public class PendingNewProduct
{
    public required Product Product { get; set; }
    public List<string> Errors { get; set; } = [];
    public HashSet<string> FieldErrors { get; set; } = [];
    public bool CustomCategory { get; set; }

    public int KegCollarInput
    {
        get => Product.KegCollar ?? 0;
        set => Product.KegCollar = value == 0 ? null : value;
    }
}
