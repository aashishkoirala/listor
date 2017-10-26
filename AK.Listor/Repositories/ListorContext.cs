/*******************************************************************************************************************************
 * AK.Listor.Repositories.ListorContext
 * Copyright © 2017 Aashish Koirala <http://aashishkoirala.github.io>
 * 
 * This file is part of Aashish Koirala's Listor.
 *  
 * Listor is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * Listor is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with Listor.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *******************************************************************************************************************************/

using AK.Listor.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AK.Listor.Repositories
{
    public class ListorContext : DbContext
    {
        private readonly IConfiguration _config;
        private readonly ILogger<ListorContext> _logger;

        public ListorContext(IConfiguration config, ILogger<ListorContext> logger)
        {
            _config = config;
            _logger = logger;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            _logger.LogDebug("Configuring SQL Server connection...");

            options.UseSqlServer(_config.GetConnectionString("Main"));
        }

        protected override void OnModelCreating(ModelBuilder model)
        {
            _logger.LogDebug("Configuring EF mappings...");

            model.Entity<User>().HasKey(x => x.Id);
            model.Entity<User>().HasMany(x => x.UserLists).WithOne(x => x.User).OnDelete(DeleteBehavior.Cascade);
            model.Entity<List>().HasKey(x => x.Id);
            model.Entity<List>().HasMany(x => x.UserLists).WithOne(x => x.List).OnDelete(DeleteBehavior.Cascade);
            model.Entity<List>().HasMany(x => x.Items).WithOne(x => x.List).OnDelete(DeleteBehavior.Cascade);
            model.Entity<Item>().HasKey(x => x.Id);
            model.Entity<UserList>().HasKey(x => x.Id);
        }
    }
}