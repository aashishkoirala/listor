/*******************************************************************************************************************************
 * AK.Listor.Repositories.ListRepository
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

using AK.Listor.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AK.Listor.Repositories
{
    public class ListRepository
    {
        private readonly ListorContext _ctx;
        private readonly ILogger<ListRepository> _logger;

        public ListRepository(ListorContext ctx, ILogger<ListRepository> logger)
        {
            _ctx = ctx;
            _logger = logger;
        }

        public async Task<Result<List[]>> GetAll(int userId)
        {
            _logger.LogInformation("Getting all lists for user {userId}...", userId);

            var lists = await _ctx.Set<Entities.List>()
                .Where(x => x.UserLists.Any(y => y.User.Id == userId))
                .AsNoTracking()
                .ToArrayAsync();
            return new Result<List[]>(lists.Select(x => new List {Id = x.Id, Name = x.Name}).ToArray());
        }

        public async Task<Result<List>> Get(int userId, int listId)
        {
            _logger.LogInformation("Getting list {listId} for {userId}...", listId, userId);

            var allowed = await _ctx.Set<Entities.UserList>()
                .AsNoTracking()
                .AnyAsync(x => x.User.Id == userId && x.List.Id == listId);
            if (!allowed) return new Result<List>(ResultType.Unauthorized);

            var list = await _ctx.Set<Entities.List>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == listId);
            if (list == null) return new Result<List>(ResultType.NotFound);

            var isShared = await _ctx.Set<Entities.UserList>()
                .AsNoTracking()
                .AnyAsync(x => x.List.Id == list.Id && x.User.Id != userId);

            return new Result<List>(new List {Id = list.Id, Name = list.Name, IsShared = isShared});
        }

        public async Task<Result<List>> CreateNew(int userId, string name)
        {
            _logger.LogInformation("Creating new list {name} for user {userId}...", name, userId);

            var user = await _ctx.Set<Entities.User>().FindAsync(userId);
            if (user == null) return new Result<List>(ResultType.Unauthorized);

            var alreadyExists = await _ctx.Set<Entities.UserList>()
                .AnyAsync(x => x.User.Id == userId && x.List.Name == name);
            if (alreadyExists) return new Result<List>("A list with that name already exists.", ResultType.BadRequest);

            var list = new Entities.List {Name = name, UserLists = new List<Entities.UserList>()};
            list.UserLists.Add(new Entities.UserList {List = list, User = user});
            await _ctx.Set<Entities.List>().AddAsync(list);
            await _ctx.SaveChangesAsync();
            return new Result<List>(new List {Id = list.Id, Name = name});
        }

        public async Task<Result> Rename(int userId, List list)
        {
            _logger.LogInformation("Renaming list {list.Id} to {list.Name} for user {userId}...",
                list.Id, list.Name, userId);

            if (list.Id == 0) return new Result(ResultType.BadRequest);

            var allowed = await _ctx.Set<Entities.UserList>().AnyAsync(x => x.User.Id == userId && x.List.Id == list.Id);
            if (!allowed) return new Result(ResultType.Unauthorized);

            var alreadyExists = await _ctx.Set<Entities.UserList>()
                .AnyAsync(x => x.User.Id == userId && x.List.Name == list.Name && x.List.Id != list.Id);
            if (alreadyExists) return new Result("A list with that name already exists.", ResultType.BadRequest);

            var existingList = await _ctx.Set<Entities.List>().SingleOrDefaultAsync(x => x.Id == list.Id);
            if (existingList == null) return new Result("List not found.", ResultType.NotFound);

            existingList.Name = list.Name;
            await _ctx.SaveChangesAsync();
            return Result.Success;
        }

        public async Task<Result> Share(int userId, int listId, string targetUserName)
        {
            _logger.LogInformation("Sharing list {listId} with {targetUserName} for {userId}...",
                listId, targetUserName, userId);

            var allowed = await _ctx.Set<Entities.UserList>().AnyAsync(x => x.User.Id == userId && x.List.Id == listId);
            if (!allowed) return new Result(ResultType.Unauthorized);

            var list = await _ctx.Set<Entities.List>().SingleOrDefaultAsync(x => x.Id == listId);
            if (list == null) return new Result("List not found.", ResultType.BadRequest);

            var targetUser = await _ctx.Set<Entities.User>()
                .SingleOrDefaultAsync(x => x.Name == targetUserName);
            if (targetUser == null) return new Result("Target user not found.", ResultType.NotFound);

            var alreadyShared = await _ctx.Set<Entities.UserList>()
                .AnyAsync(x => x.User.Id == targetUser.Id && x.List.Id == listId);
            if (alreadyShared)
                return new Result($"The list is already shared with {targetUserName}.", ResultType.BadRequest);

            await _ctx.Set<Entities.UserList>().AddAsync(new Entities.UserList {List = list, User = targetUser});
            await _ctx.SaveChangesAsync();

            return Result.Success;
        }

        public async Task<Result> Disown(int userId, int listId)
        {
            _logger.LogInformation("Disowning list {listId} by {userId}...", listId, userId);

            var userList = await _ctx.Set<Entities.UserList>()
                .SingleOrDefaultAsync(x => x.User.Id == userId && x.List.Id == listId);
            if (userList == null) return new Result(ResultType.Unauthorized);

            var isShared = await _ctx.Set<Entities.UserList>().AnyAsync(x => x.List.Id == listId && x.User.Id != userId);
            if (!isShared) return new Result("You cannot disown a list that is not shared.", ResultType.BadRequest);

            _ctx.Set<Entities.UserList>().Remove(userList);
            await _ctx.SaveChangesAsync();
            return Result.Success;
        }

        public async Task<Result> Delete(int userId, int listId)
        {
            _logger.LogInformation("Deleting list {listId} by {userId}...", listId, userId);

            var userList = await _ctx.Set<Entities.UserList>().SingleOrDefaultAsync(x => x.User.Id == userId && x.List.Id == listId);
            if (userList == null) return new Result(ResultType.Unauthorized);

            var isShared = await _ctx.Set<Entities.UserList>().AnyAsync(x => x.List.Id == listId && x.User.Id != userId);
            if (isShared) return new Result("You cannot delete a list that is shared.", ResultType.BadRequest);

            var list = await _ctx.Set<Entities.List>().SingleOrDefaultAsync(x => x.Id == listId);
            if (list == null) return new Result("List not found.", ResultType.NotFound);

            _ctx.Set<Entities.UserList>().Remove(userList);
            _ctx.Set<Entities.List>().Remove(list);

            await _ctx.SaveChangesAsync();
            return Result.Success;
        }
    }
}