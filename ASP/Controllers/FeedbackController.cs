using ASP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASP.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly AppDbContext _context;
        public FeedbackController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create([FromBody] FeedbackFormModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var feedback = new Feedback
            {
                Comment = model.Comment,
                // інші поля
                CreatedAt = model.CreatedAt == default ? DateTime.UtcNow : model.CreatedAt
            };

            _context.Feedbacks.Add(feedback);
            _context.SaveChanges();

            return Ok(new { success = true });
        }

    }
}
