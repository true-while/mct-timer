using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using mct_timer.Models;
using NuGet.DependencyResolver;
using System.Reflection.Emit;

    public class UsersContext : DbContext
    {
        public UsersContext(DbContextOptions<UsersContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
                base.OnModelCreating(builder);
                builder.HasDefaultContainer("Users");

                builder.Entity<User>().ToContainer("Users");

           
        }

}
