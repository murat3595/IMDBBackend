using System.ComponentModel.DataAnnotations.Schema;

namespace IMDBApi.Data.Models
{
    public class MovieActor : BaseModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double Rating { get; set; }
        public string VideoLink { get; set; }
        public int MType { get; set; } // 0 movie 1 actor


        [NotMapped]
        public MovieComment? Rate { get; set; }
        [NotMapped]
        public bool AddedToWatchlist { get; set; }
    }
}
