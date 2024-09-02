using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using mct_timer.Models;

    public class WebSettingsContext : DbContext
    {
        public WebSettingsContext (DbContextOptions<WebSettingsContext> options)
            : base(options)
        {
        }

        public DbSet<WebSettings> WebSettings { get; set; } = default!;
    }
