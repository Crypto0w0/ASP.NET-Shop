namespace ASP.Models
{
    public class FeedbackFormModel
    {
        public string Comment { get; set; }
        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; } 
    }
}
