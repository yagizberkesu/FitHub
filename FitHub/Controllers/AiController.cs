using FitHub.Models;
using FitHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitHub.Controllers
{
    public class AiController : Controller
    {
        private readonly IGroqService _groq;

        public AiController(IGroqService groq)
        {
            _groq = groq;
        }

        [HttpGet]
        public IActionResult Koc()
        {
            return View(new AiCoachViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Koc(AiCoachViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            ViewBag.Result = await _groq.GenerateCoachPlanAsync(vm);
            return View(vm);
        }
    }
}
