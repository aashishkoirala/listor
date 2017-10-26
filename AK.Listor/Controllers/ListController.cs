/*******************************************************************************************************************************
 * AK.Listor.Controllers.ListController
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AK.Listor.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = LoginConstants.CookieName)]
    public class ListController : ControllerBase
    {
        private readonly ListRepository _listRepository;

        public ListController(ListRepository listRepository)
        {
            _listRepository = listRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get() => Result(await _listRepository.GetAll(UserId));

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id) => Result(await _listRepository.Get(UserId, id));

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List list)
            => Result(await _listRepository.CreateNew(UserId, list.Name));

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] List list)
        {
            list.Id = id;
            return Result(await _listRepository.Rename(UserId, list));
        }

        [HttpPut("{id}/shareWith/{userName}")]
        public async Task<IActionResult> Share(int id, string userName) => Result(await _listRepository.Share(UserId, id, userName));

        [HttpPut("{id}/disown")]
        public async Task<IActionResult> Disown(int id) => Result(await _listRepository.Disown(UserId, id));

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) => Result(await _listRepository.Delete(UserId, id));
    }
}