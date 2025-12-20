using FitHub.Models;

namespace FitHub.Services
{
    public interface IGroqService
    {
        Task<string> GenerateCoachPlanAsync(AiCoachViewModel vm);
    }
}
