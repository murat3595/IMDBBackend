using IMDBApi.Data;
using IMDBApi.Data.Models;

namespace IMDBApi
{
    public class CurrentUserService
    {
        public User? User { get; set; }
        
        private ImdbDbContext dbContext;

        public CurrentUserService(ImdbDbContext context)
        {
            dbContext = context;
            User = null;
        }

        public void SetCurrentUser(int id)
        {
            this.User = dbContext.Users.First(s => s.Id == id);
        }

    }
}
