namespace IMDBApi.Data.Models
{
    public class MovieComment : BaseModel
    {
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public string Comment { get; set; }
        public double Point { get; set; }
    }
}
