/*******************************************************************************************************************************
 * AK.Listor.Controllers.ControllerBase
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
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AK.Listor.Controllers
{
    public abstract class ControllerBase : Controller
    {
        protected int UserId
        {
            get
            {
                var identity = Request.HttpContext.User?.Identity as ClaimsIdentity;
                if (identity == null) return 0;
                return int.TryParse(identity.Name, out int result) ? result : 0;
            }
        }

        protected IActionResult Result(Result result)
        {
            switch (result.Type)
            {
                case ResultType.Success:
                    return new OkObjectResult(new {success = true});
                case ResultType.BadRequest:
                    return new BadRequestObjectResult(new {error = result.ErrorMessage});
                case ResultType.Unauthorized:
                    return new UnauthorizedResult();
                case ResultType.NotFound:
                    return new NotFoundObjectResult(new {error = result.ErrorMessage});
            }
            return new ObjectResult(new {error = result.ErrorMessage});
        }

        protected IActionResult Result<T>(Result<T> result)
        {
            switch (result.Type)
            {
                case ResultType.Success:
                    return new OkObjectResult(result.Value);
                case ResultType.BadRequest:
                    return new BadRequestObjectResult(new { error = result.ErrorMessage });
                case ResultType.Unauthorized:
                    return new UnauthorizedResult();
                case ResultType.NotFound:
                    return new NotFoundObjectResult(new { error = result.ErrorMessage });
            }
            return new ObjectResult(new { error = result.ErrorMessage });
        }
    }
}