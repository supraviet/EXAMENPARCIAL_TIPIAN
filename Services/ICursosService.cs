using EXAPARCIALALVARO.Models;

namespace EXAPARCIALALVARO.Services
{
    public interface ICursosService
    {
        Task<List<Curso>> GetCursosActivosAsync();
        Task InvalidarCacheCursosAsync();
    }
}