/*******************************************************************************************************************************
 * AK.Listor.Entities.List
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

using System.Collections.Generic;

namespace AK.Listor.Entities
{
    public class List
    {
        public int Id { get; set; }
        public ICollection<UserList> UserLists { get; set; }
        public string Name { get; set; }
        public ICollection<Item> Items { get; set; }
    }
}