using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Initializer
{
    public interface IDbInitializer
    {
        Task<bool> Initialize();
    }
}
