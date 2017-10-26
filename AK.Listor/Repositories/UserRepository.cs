/*******************************************************************************************************************************
 * AK.Listor.Repositories.UserRepository
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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AK.Listor.Repositories
{
    public class UserRepository
    {
        private readonly ListorContext _ctx;
        private readonly ILogger<UserRepository> _logger;
        private static readonly Regex UsernameRegex = new Regex("^[A-Za-z0-9]+$");

        public UserRepository(ListorContext ctx, ILogger<UserRepository> logger)
        {
            _ctx = ctx;
            _logger = logger;
        }

        public async Task<Result<bool>> IsAvailable(string userName)
        {
            _logger.LogInformation("Check availability of username {userName}...", userName);

            var exists = await _ctx.Set<Entities.User>().AsNoTracking().AnyAsync(x => x.Name == userName);
            return new Result<bool>(!exists);
        }

        public async Task<Result<int>> Authenticate(string userName, string password)
        {
            _logger.LogInformation("Authenticating user {userName}...", userName);

            var unauthorized = new Result<int>("Invalid username/password.", ResultType.Unauthorized);
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password)) return unauthorized;

            var user = await _ctx.Set<Entities.User>().AsNoTracking().SingleOrDefaultAsync(x => x.Name == userName);
            if (user == null) return unauthorized;

            var success = VerifySaltedPasswordHash(password, user.Password);
            return success ? new Result<int>(user.Id) : unauthorized;
        }

        public async Task<Result<User>> Get(int id)
        {
            _logger.LogInformation("Getting user {id}...", id);

            var user = await _ctx.Set<Entities.User>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
            return user != null
                ? new Result<User>(new User {Id = user.Id, Name = user.Name})
                : new Result<User>("User not found.", ResultType.NotFound);
        }

        public async Task<Result> ChangePassword(PasswordChange change)
        {
            _logger.LogInformation("Changing password for user {change.UserId}", change.UserId);

            var valid = IsValidPassword(change.CurrentPassword);
            if (!valid.IsSuccess) return valid;

            valid = IsValidPassword(change.NewPassword);
            if (!valid.IsSuccess) return valid;

            if (!change.NewPassword.Equals(change.RetypeNewPassword, StringComparison.CurrentCulture))
                return new Result("Passwords do not match.", ResultType.BadRequest);

            var user = await _ctx.Set<Entities.User>().SingleOrDefaultAsync(x => x.Id == change.UserId);
            if (user == null) return new Result("User not found.", ResultType.NotFound);

            if (!VerifySaltedPasswordHash(change.CurrentPassword, user.Password))
                return new Result("Invalid password.", ResultType.Unauthorized);

            user.Password = SaltAndHashPassword(change.NewPassword);
            await _ctx.SaveChangesAsync();

            return Result.Success;
        }

        public async Task<Result<int>> Save(User user)
        {
            _logger.LogInformation("Saving user {user.Name}...", user.Name);

            var valid = await IsValidUserName(user.Name, user.Id);
            if (!valid.IsSuccess) return new Result<int>(valid);

            byte[] saltedPasswordHash = null;
            if (user.Id == 0)
            {
                valid = IsValidPassword(user.Password);
                if (!valid.IsSuccess) return new Result<int>(valid);

                saltedPasswordHash = SaltAndHashPassword(user.Password);
            }

            Entities.User entity;
            if (user.Id <= 0)
            {
                entity = new Entities.User {Password = saltedPasswordHash};
                await _ctx.Set<Entities.User>().AddAsync(entity);
            }
            else entity = await _ctx.Set<Entities.User>().SingleOrDefaultAsync(x => x.Id == user.Id);
            if (entity == null) return new Result<int>("User not found.", ResultType.NotFound);

            entity.Name = user.Name;
            await _ctx.SaveChangesAsync();

            user.Id = entity.Id;
            return new Result<int>(user.Id);
        }

        public async Task<Result> Delete(int id)
        {
            _logger.LogInformation("Deleting user {id}...", id);

            var listIdsToDelete = await GetListsToDelete(id);
            var user = await _ctx.Set<Entities.User>().SingleOrDefaultAsync(x => x.Id == id);
            if (user != null) _ctx.Remove(user);
            _ctx.RemoveRange(listIdsToDelete.Select(x => new Entities.List {Id = x}));
            await _ctx.SaveChangesAsync();
            return Result.Success;
        }

        private async Task<int[]> GetListsToDelete(int userId)
        {
            const string listIdsToDeleteQuery =
                "SELECT L.Id FROM [List] L INNER JOIN [UserList] UL ON UL.ListId = L.Id " +
                "WHERE UL.UserId = @UserId AND NOT EXISTS (" +
                "SELECT 1 FROM [UserList] UL2 WHERE UL2.UserId <> @UserId)";

            var connection = _ctx.Database.GetDbConnection(); // Cannot dispose here because we need to execute other stuff against EF.

            await connection.OpenAsync();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = listIdsToDeleteQuery;
                var parameter = command.CreateParameter();
                parameter.DbType = DbType.Int32;
                parameter.Value = userId;
                parameter.ParameterName = "@UserId";
                command.Parameters.Add(parameter);
                var result = new List<int>();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read()) result.Add(reader.GetInt32(0));
                }
                return result.ToArray();
            }
        }

        private async Task<Result> IsValidUserName(string name, int currentUserId)
        {
            name = name?.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(name))
                return new Result("Username must not be empty or all spaces.", ResultType.BadRequest);
            if (name.Length > 20)
                return new Result("Username cannot be more than 20 characters.", ResultType.BadRequest);
            if (!UsernameRegex.IsMatch(name))
                return new Result("Username must be alphanumeric.", ResultType.BadRequest);

            var alreadyTaken = await _ctx.Set<Entities.User>()
                .AsNoTracking()
                .AnyAsync(x => x.Name == name && x.Id != currentUserId);
            return alreadyTaken ? new Result("This username is not available.", ResultType.BadRequest) : Result.Success;
        }

        private static Result IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return new Result("You must specify a password.", ResultType.BadRequest);

            return password.Length < 12
                ? new Result("The password must be at least 12 characters.", ResultType.BadRequest)
                : Result.Success;
        }

        private static byte[] SaltAndHashPassword(string password)
        {
            var salt = new byte[8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(salt);
            }
            var source = salt.Concat(Encoding.UTF8.GetBytes(password)).ToArray();            
            using (var hash = SHA256.Create())
            {
                var hashed = hash.ComputeHash(source);
                return salt.Concat(hashed).ToArray();
            }
        }

        private static bool VerifySaltedPasswordHash(string password, IReadOnlyCollection<byte> saltedHash)
        {
            if (saltedHash == null || saltedHash.Count != 40) return false;
            var salt = saltedHash.Take(8).ToArray();
            var source = salt.Concat(Encoding.UTF8.GetBytes(password)).ToArray();
            byte[] expectedSaltedHash;
            using (var hash = SHA256.Create())
            {
                expectedSaltedHash = salt.Concat(hash.ComputeHash(source)).ToArray();
            }
            return expectedSaltedHash.SequenceEqual(saltedHash);
        }
    }
}