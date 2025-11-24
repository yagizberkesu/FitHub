using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitHub.Models;
using Microsoft.AspNetCore.Http;
using FitHub.Filters;

namespace FitHub.Controllers
{
    [AdminAuthorize]
    public class EgitmenlerController : Controller
    {
        // ...existing code...
    }
}
