using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitHub.Models;
using FitHub.Filters;

namespace FitHub.Controllers;

[AdminAuthorize]
public class EgitmenlerController : Controller
{
    private readonly FitHubContext _context;

    public EgitmenlerController(FitHubContext context)
    {
        _context = context;
    }

    // GET: /Egitmenler
    public async Task<IActionResult> Index()
    {
        var egitmenler = await _context.Set<Egitmen>()
            .Include(e => e.Salon)
            .AsNoTracking()
            .ToListAsync();

        return View(egitmenler);
    }

    // GET: /Egitmenler/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var egitmen = await _context.Set<Egitmen>()
            .Include(e => e.Salon)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (egitmen == null) return NotFound();
        return View(egitmen);
    }

    // GET: /Egitmenler/Create
    public IActionResult Create()
    {
        ViewData["SalonId"] = SalonSelectList();
        return View();
    }

    // POST: /Egitmenler/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Egitmen egitmen)
    {
        if (!ModelState.IsValid)
        {
            ViewData["SalonId"] = SalonSelectList(egitmen.SalonId);
            return View(egitmen);
        }

        _context.Add(egitmen);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Egitmenler/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var egitmen = await _context.Set<Egitmen>().FindAsync(id);
        if (egitmen == null) return NotFound();

        ViewData["SalonId"] = SalonSelectList(egitmen.SalonId);
        return View(egitmen);
    }

    // POST: /Egitmenler/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Egitmen egitmen)
    {
        if (id != egitmen.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewData["SalonId"] = SalonSelectList(egitmen.SalonId);
            return View(egitmen);
        }

        _context.Update(egitmen);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Egitmenler/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var egitmen = await _context.Set<Egitmen>()
            .Include(e => e.Salon)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (egitmen == null) return NotFound();
        return View(egitmen);
    }

    // POST: /Egitmenler/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var egitmen = await _context.Set<Egitmen>().FindAsync(id);
        if (egitmen != null)
        {
            _context.Remove(egitmen);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private SelectList SalonSelectList(int? selectedId = null)
    {
        // Salon adý alanýn "Ad" deðilse (SalonAdi vb.) burayý deðiþtir.
        var salonlar = _context.Set<Salon>()
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .ToList();

        return new SelectList(salonlar, "Id", "Ad", selectedId);
    }
}
