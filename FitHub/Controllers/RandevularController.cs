using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitHub.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using FitHub.Filters;

namespace FitHub.Controllers
{
    [AdminAuthorize]
    public class RandevularController : Controller
    {
        // ...existing code...
    }
}
