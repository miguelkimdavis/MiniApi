using System.ComponentModel.DataAnnotations;

namespace Category.Api.Shared.Dtos
{
    public class UpdateProductDto
    {
        [Required(ErrorMessage = "Product Name Is Required!")]
        [StringLength(30, ErrorMessage = "Product Name Should Not Exceed 30 Characters!")]
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
