/*******************************************************************************************************************************
 * AK.Listor.Repositories.ItemRepository
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
using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace AK.Listor.Repositories
{
    public class ItemRepository
    {
        private readonly ListorContext _ctx;
        private readonly ILogger<ItemRepository> _logger;

        public ItemRepository(ListorContext ctx, ILogger<ItemRepository> logger)
        {
            _ctx = ctx;
            _logger = logger;
        }

        public async Task<Result<Item[]>> GetAll(int userId, int listId)
        {
            _logger.LogInformation("Getting all items in list {listId} for user {userId}...", listId, userId);

            var allowed = await _ctx.Set<Entities.UserList>()
                .AsNoTracking()
                .AnyAsync(x => x.User.Id == userId && x.List.Id == listId);
            if (!allowed) return new Result<Item[]>(ResultType.Unauthorized);

            var entities = await _ctx.Set<Entities.Item>()
                .Where(x => x.List.Id == listId)
                .Include(x => x.List)
                .AsNoTracking()
                .ToArrayAsync();

            var items = entities
                .Select(x =>
                    new Item {Id = x.Id, Description = x.Description, IsChecked = x.IsChecked, ListId = x.List.Id})
                .ToArray();

            return new Result<Item[]>(items);
        }

        public async Task<Result<Item>> Get(int userId, int itemId)
        {
            _logger.LogInformation("Getting item {itemId} for user {userId}...", itemId, userId);

            var entity = await _ctx.Set<Entities.Item>()
                .Include(x => x.List)
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == itemId);

            var allowed = await _ctx.Set<Entities.UserList>()
                .AsNoTracking()
                .AnyAsync(x => x.User.Id == userId && x.List.Id == entity.List.Id);
            if (!allowed) return new Result<Item>(ResultType.Unauthorized);

            return new Result<Item>(new Item
            {
                Id = entity.Id,
                Description = entity.Description,
                IsChecked = entity.IsChecked,
                ListId = entity.List.Id
            });
        }

        public async Task<Result<Item[]>> CreateNew(int userId, Item item)
        {
            _logger.LogInformation("Creating new item for user {userId}...", userId);

            var allowed = await _ctx.Set<Entities.UserList>()
                .AsNoTracking()
                .AnyAsync(x => x.User.Id == userId && x.List.Id == item.ListId);
            if (!allowed) return new Result<Item[]>(ResultType.Unauthorized);

            var list = await _ctx.Set<Entities.List>().SingleOrDefaultAsync(x => x.Id == item.ListId);
            if (list == null) return new Result<Item[]>("List not found.", ResultType.NotFound);

            var description = item.Description?.Trim().Replace("\r", "").Replace("\n", "").Trim();

            if (string.IsNullOrWhiteSpace(description))
                return new Result<Item[]>("Description cannot be empty.", ResultType.BadRequest);

            var entities = item.Description
                .Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => new Entities.Item
                {
                    Description = x.Trim(),
                    IsChecked = false,
                    List = list
                })
                .ToArray();

            foreach (var entity in entities)
            {
                if (entity.Description.Length > 200)
                    entity.Description = entity.Description.Substring(0, 200).Trim();
            }

            await _ctx.Set<Entities.Item>().AddRangeAsync(entities);
            await _ctx.SaveChangesAsync();

            return new Result<Item[]>(entities
                .Select(x => new Item
                {
                    Id = x.Id,
                    Description = x.Description,
                    ListId = x.List.Id
                })
                .ToArray());
        }

        public async Task<Result> UpdateDescription(int userId, int itemId, string description)
        {
            _logger.LogInformation("Updating item {itemId} description for user {userId}...", itemId, userId);

            description = description?.Trim().Replace("\r", "").Replace("\n", "").Trim();
            if (string.IsNullOrWhiteSpace(description))
                return new Result("Description cannot be empty.", ResultType.BadRequest);

            if (description.Length > 200) description = description.Substring(0, 200).Trim();
            if (string.IsNullOrWhiteSpace(description))
                return new Result("Description cannot be empty.", ResultType.BadRequest);

            return await Execute("UPDATE I SET [Description] = @Description FROM [Item] I INNER JOIN " +
                                 "[UserList] UL ON UL.ListId = I.ListId WHERE UL.UserId = @UserId AND I.Id = @ItemId",
                userId, null, itemId,
                cmd =>
                {
                    var parameter = cmd.CreateParameter();
                    parameter.ParameterName = "@Description";
                    parameter.Value = description;
                    cmd.Parameters.Add(parameter);
                });
        }

        public async Task<Result> UpdateIsChecked(int userId, int itemId, bool isChecked)
        {
            var status = isChecked ? "checked" : "unchecked";
            _logger.LogInformation("Updating item {itemId} to {status} for user {userId}...", itemId, status, userId);

            return await Execute("UPDATE I SET [IsChecked] = @IsChecked FROM [Item] I INNER JOIN " +
                                 "[UserList] UL ON UL.ListId = I.ListId WHERE UL.UserId = @UserId AND I.Id = @ItemId",
                userId, null, itemId,
                cmd =>
                {
                    var parameter = cmd.CreateParameter();
                    parameter.ParameterName = "@IsChecked";
                    parameter.Value = isChecked;
                    cmd.Parameters.Add(parameter);
                });
        }

        public Task<Result> Delete(int userId, int itemId)
        {
            _logger.LogInformation("Deleting item {itemId} description for user {userId}...", itemId, userId);

            return Execute("DELETE I FROM [Item] I INNER JOIN " +
                           "[UserList] UL ON UL.ListId = I.ListId WHERE UL.UserId = @UserId AND I.Id = @ItemId",
                userId, null, itemId, null);
        }

        public async Task<Result> DeleteAll(int userId, int listId, bool checkedOnly)
        {
            var status = checkedOnly ? "all checked" : "all";
            _logger.LogInformation("Deleting {status} items from list {listId} for user {userId}...",
                status, listId, userId);

            var allowed = await _ctx.Set<Entities.UserList>()
                .AsNoTracking()
                .AnyAsync(x => x.User.Id == userId && x.List.Id == listId);
            if (!allowed) return new Result(ResultType.Unauthorized);

            return await Execute("DELETE FROM [Item] WHERE ListId = @ListId AND (@CheckedOnly = 0 OR IsChecked = 1)",
                userId, listId, null,
                cmd =>
                {
                    var parameter = cmd.CreateParameter();
                    parameter.ParameterName = "@CheckedOnly";
                    parameter.Value = checkedOnly;
                    cmd.Parameters.Add(parameter);
                });
        }

        private async Task<Result> Execute(string sql, int? userId, int? listId, int? itemId,
            Action<DbCommand> commandAction)
        {
            using (var connection = _ctx.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;

                    if (userId.HasValue)
                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = "@UserId";
                        parameter.Value = userId.Value;
                        command.Parameters.Add(parameter);
                    }

                    if (listId.HasValue)
                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = "@ListId";
                        parameter.Value = listId.Value;
                        command.Parameters.Add(parameter);
                    }

                    if (itemId.HasValue)
                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = "@ItemId";
                        parameter.Value = itemId.Value;
                        command.Parameters.Add(parameter);
                    }

                    commandAction?.Invoke(command);

                    var rows = await command.ExecuteNonQueryAsync();
                    return rows > 0 ? Result.Success : new Result(ResultType.NotFound);
                }
            }
        }
    }
}