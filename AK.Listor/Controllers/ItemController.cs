/*******************************************************************************************************************************
 * AK.Listor.Controllers.ItemController
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
    public class ItemController : ControllerBase
    {
        private readonly ItemRepository _itemRepository;

        public ItemController(ItemRepository itemRepository)
        {
            _itemRepository = itemRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int listId) => Result(await _itemRepository.GetAll(UserId, listId));

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id) => Result(await _itemRepository.Get(UserId, id));

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Item item)
        {
            item.Id = 0;
            return Result(await _itemRepository.CreateNew(UserId, item));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Item item)
            => Result(await _itemRepository.UpdateDescription(UserId, id, item.Description));

        [HttpPut("{id}/{status}")]
        public async Task<IActionResult> UpdateIsChecked(int id, string status)
        {
            bool isChecked;
            if (status == "check") isChecked = true;
            else if (status == "uncheck") isChecked = false;
            else return NotFound();

            return Result(await _itemRepository.UpdateIsChecked(UserId, id, isChecked));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) => Result(await _itemRepository.Delete(UserId, id));

        [HttpDelete]
        public async Task<IActionResult> DeleteAll(int listId, bool checkedOnly = false)
            => Result(await _itemRepository.DeleteAll(UserId, listId, checkedOnly));
    }
}