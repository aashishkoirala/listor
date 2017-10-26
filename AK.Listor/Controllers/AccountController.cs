/*******************************************************************************************************************************
 * AK.Listor.Controllers.AccountController
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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AK.Listor.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private readonly UserRepository _userRepository;

        public AccountController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            var login = new Login {ReturnUrl = returnUrl};
            var userId = UserId;
            if (userId == 0) return View(login);

            var result = await _userRepository.Get(userId);
            if (!result.IsSuccess) return View(login);

            login.UserName = result.Value.Name;
            login.RememberMe = true;
            return View(login);
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromForm] Login login)
        {
            var result = await _userRepository.Authenticate(login.UserName, login.Password);
            if (!result.IsSuccess)
            {
                login.ErrorMessage = result.ErrorMessage;
                return View(login);
            }
            var userId = result.Value;

            var identity = new ClaimsIdentity(new[] {new Claim(LoginConstants.UserIdClaim, userId.ToString())},
                CookieAuthenticationDefaults.AuthenticationScheme, LoginConstants.UserIdClaim, "unused");
            await Request.HttpContext.SignInAsync(LoginConstants.CookieName, new ClaimsPrincipal(identity),
                new AuthenticationProperties {IsPersistent = login.RememberMe});
            return Redirect(login.ReturnUrl ?? Url.Content("/"));
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await Request.HttpContext.SignOutAsync(LoginConstants.CookieName);
            return Redirect(Url.Content("/"));
        }

        [HttpGet]
        public IActionResult Register() => View(new Register());

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] Register register)
        {
            if (!register.Password.Equals(register.RetypePassword, StringComparison.CurrentCulture))
            {
                register.ErrorMessage = "Passwords do not match.";
                return View(register);
            }

            var user = new User {Name = register.UserName, Password = register.Password};
            var result = await _userRepository.Save(user);

            if (!result.IsSuccess)
            {
                register.ErrorMessage = result.ErrorMessage;
                return View(register);
            }

            var identity = new ClaimsIdentity(new[] {new Claim(LoginConstants.UserIdClaim, result.Value.ToString())},
                CookieAuthenticationDefaults.AuthenticationScheme, LoginConstants.UserIdClaim, "unused");
            await Request.HttpContext.SignInAsync(LoginConstants.CookieName, new ClaimsPrincipal(identity),
                new AuthenticationProperties {IsPersistent = false});
            return Redirect(Url.Content("/"));
        }
    }
}