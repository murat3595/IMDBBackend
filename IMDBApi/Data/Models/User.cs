using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IMDBApi.Data.Models
{
    public class User : BaseModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        [JsonIgnore]
        public string Password { get; set; }


        [NotMapped]
        public UserWatchList UserWatchList { get; set; }
    }
}
