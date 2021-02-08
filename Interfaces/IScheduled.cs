using System.Threading.Tasks;

namespace WebRTCServer.Interfaces
{
    public interface IScheduled
    {
        Task onTimeExecute(int counter);
    }
}