/*******************************************************************************************************************************
 * AK.Listor.Controllers.UserController
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
using AK.Listor.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AK.Listor.Controllers
{
    [Route("api/[controller]")]    
    public class UserController : ControllerBase
    {
        private readonly UserRepository _userRepository;

        public UserController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = LoginConstants.CookieName)]
        public async Task<IActionResult> Get() => Result(await _userRepository.Get(UserId));

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] User user)
        {
            user.Id = 0;
            return Result(await _userRepository.Save(user));
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = LoginConstants.CookieName)]
        public async Task<IActionResult> Put([FromBody] User user)
        {
            user.Id = UserId;
            return Result(await _userRepository.Save(user));
        }

        [HttpPut("changePassword")]
        [Authorize(AuthenticationSchemes = LoginConstants.CookieName)]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordChange change)
        {
            change.UserId = UserId;
            return Result(await _userRepository.ChangePassword(change));
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            var result = await _userRepository.Delete(UserId);
            if (!result.IsSuccess) return Result(result);

            await Request.HttpContext.SignOutAsync(LoginConstants.CookieName);
            return Ok(new {redirectUrl = Url.Content("/")});
        }

        [HttpGet("availability/{userName}")]
        public async Task<IActionResult> IsAvailable(string userName)
            => Result(await _userRepository.IsAvailable(userName));
    }
}