using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using mtt_timer.Models;

namespace mtt_timer.Controllers
{
    public class WebSettingsController : Controller
    {
        private readonly WebSettingsContext _context;
        private readonly IOptions<ConfigMng> _config;
        private readonly IDalleGenerator _gen;
        private readonly IBlobRepo _repo;

        public WebSettingsController(
            IDalleGenerator gen,
            IBlobRepo repo,
            WebSettingsContext context, 
            IOptions<ConfigMng> config)
        {
            _context = context;
            _config = config;
            _gen = gen;
            _repo = repo;
        }

        // GET: WebSettings
        public async Task<IActionResult> Index()
        {
            await _context.Database.EnsureCreatedAsync();


            var list = await _context.WebSettings.ToListAsync();
            list.ForEach(x => x.Path = _repo.GetImageSASLink(x.GetFileName()));

            return View(list);
        }

        // GET: WebSettings/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var webSettings = await _context.WebSettings
                .FirstOrDefaultAsync(m => m.ID == id);

            if (webSettings == null)
            {
                return NotFound();
            }

            webSettings.Path = _repo.GetImageSASLink(webSettings.GetFileName());

            return View(webSettings);
        }

        // GET: WebSettings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: WebSettings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Prompt","Name")] WebSettings webSettings)
        {
            webSettings.ID = Guid.NewGuid();
            webSettings.User = "aivanov";            

            //gen image
            var imggen = await _gen.GenerateImage(webSettings.Prompt);
            webSettings.Description = imggen.RevisedPrompt;

            //sage image
            var mdata = new Dictionary<string, string>();
            mdata["user"] = webSettings.User;
            mdata["when"] = DateTime.Now.ToString();
            mdata["prompt"] = webSettings.Prompt;
            
           
            webSettings.Path = await _repo.SaveImage(webSettings.GetFileName(),  imggen.ImageBytes, mdata);

            //if (ModelState.IsValid)
            //{
                _context.Add(webSettings);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            //}
            return View(webSettings);
        }

        // GET: WebSettings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var webSettings = await _context.WebSettings.FindAsync(id);
            if (webSettings == null)
            {
                return NotFound();
            }
            return View(webSettings);
        }

        // POST: WebSettings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("ID,User,Prompt,Path")] WebSettings webSettings)
        {
            if (id != webSettings.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(webSettings);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WebSettingsExists(webSettings.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(webSettings);
        }

        // GET: WebSettings/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var webSettings = await _context.WebSettings
                .FirstOrDefaultAsync(m => m.ID == id);
            if (webSettings == null)
            {
                return NotFound();
            }

            return View(webSettings);
        }

        // POST: WebSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var webSettings = await _context.WebSettings.FindAsync(id);
            if (webSettings != null)
            {
                _context.WebSettings.Remove(webSettings);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WebSettingsExists(Guid id)
        {
            return _context.WebSettings.Any(e => e.ID == id);
        }
    }
}
