using System.Threading.Tasks;
using WebRTCServer.Models;

namespace WebRTCServer.Interfaces
{
    public interface IUserRepository
    {
        public Task<User> getUserById(string userid);

        public Task<User> getUserByName(string name);

    }
}