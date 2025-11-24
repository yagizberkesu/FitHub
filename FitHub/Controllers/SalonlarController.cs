using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitHub.Models;
using FitHub.Filters;

namespace FitHub.Controllers
{
    [AdminAuthorize]
    public class SalonlarController : Controller
    {
        // ...existing code...
    }
}
