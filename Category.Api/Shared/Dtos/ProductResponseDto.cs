using System.ComponentModel.DataAnnotations;

namespace Category.Api.Shared.Dtos
{
    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime AddedDate { get; set; }
    }
}
