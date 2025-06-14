using System.ComponentModel.DataAnnotations;

namespace ASP.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва товару є обов'язковою")]
        [StringLength(100, ErrorMessage = "Назва не може перевищувати 100 символів")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Опис є обов'язковим")]
        [StringLength(500, ErrorMessage = "Опис не може перевищувати 500 символів")]
        public string Description { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "Ціна повинна бути більшою за 0")]
        public decimal Price { get; set; }

        [Range(0, 100000, ErrorMessage = "Кількість повинна бути 0 або більше")]
        public int Stock { get; set; }
        public string? ImagePath { get; set; }
        public List<Feedback> Feedbacks { get; set; }

    }
}
