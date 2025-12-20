using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitHub.Models;
using FitHub.Filters;

namespace FitHub.Controllers;

[AdminAuthorize]
public class SalonlarController : Controller
{
    private readonly FitHubContext _context;

    public SalonlarController(FitHubContext context)
    {
        _context = context;
    }

    // GET: /Salonlar
    public async Task<IActionResult> Index()
    {
        var salonlar = await _context.Set<Salon>()
            .AsNoTracking()
            .ToListAsync();

        return View(salonlar);
    }

    // GET: /Salonlar/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var salon = await _context.Set<Salon>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (salon == null) return NotFound();
        return View(salon);
    }

    // GET: /Salonlar/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Salonlar/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Salon salon)
    {
        if (!ModelState.IsValid) return View(salon);

        _context.Add(salon);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Salonlar/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var salon = await _context.Set<Salon>().FindAsync(id);
        if (salon == null) return NotFound();

        return View(salon);
    }

    // POST: /Salonlar/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Salon salon)
    {
        if (id != salon.Id) return NotFound();
        if (!ModelState.IsValid) return View(salon);

        _context.Update(salon);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Salonlar/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var salon = await _context.Set<Salon>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (salon == null) return NotFound();
        return View(salon);
    }

    // POST: /Salonlar/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var salon = await _context.Set<Salon>().FindAsync(id);
        if (salon != null)
        {
            _context.Remove(salon);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
