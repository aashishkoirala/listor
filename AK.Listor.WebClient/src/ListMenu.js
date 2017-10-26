/*******************************************************************************************************************************
 * AK.Listor.WebClient.ListMenu.js
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

import React from 'react';
import './ListMenu.css';

const ListMenuItem = ({list, isActive, onListSelected}) => (
  <li>
    <a className={"list-link" + (isActive ? " active" : "")} href="#" onClick={() => onListSelected(list)}>
      <span className="glyphicon glyphicon-hand-right"></span>
      {list.name}
    </a>
  </li>
);

export default ({ lists, activeList, onListSelected, onNewListAddRequested }) => (
  <div>
    <h4 className="pull-left">My Lists</h4>
    <button type="button" className="btn-circle btn-add pull-right" onClick={onNewListAddRequested}></button>
    <br className="clear-left"/>
    <br/>
    <ul className="nav nav-pills nav-stacked">
      {lists.map(l => <ListMenuItem list={l} isActive={activeList != null && activeList.id === l.id}
        onListSelected={onListSelected} key={l.id} />)}
    </ul>
  </div>
);
