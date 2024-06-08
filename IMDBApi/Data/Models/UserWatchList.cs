using System.ComponentModel.DataAnnotations.Schema;

namespace IMDBApi.Data.Models
{
    public class UserWatchList :BaseModel
    {
        public int UserId { get; set; }
        public int MovieId { get; set; }

    }
}
