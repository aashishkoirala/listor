/*******************************************************************************************************************************
 * AK.Listor.WebClient.ListDelete.js
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

import React, {Component} from 'react';
import {invokeApi} from './common';
import {putListDisown, deleteList, deleteItems} from './webApi';
import Status from './Status';

export default class ListDelete extends Component {
  constructor(props) {
    super(props);
    this.state = { isBusy: false, error: '', success: '' };
    this.onDisowned = this.onDisowned.bind(this);
    this.onDeleted = this.onDeleted.bind(this);
    this.onItemsDeleted = this.onItemsDeleted.bind(this);
    this.onClosed = this.onClosed.bind(this);
  }

  onDisowned() {
    if (this.state.isBusy) return;
    let self = this;
    let id = this.props.list.id;
    invokeApi(this, () => putListDisown(id)).then(() => self.props.onListRemoved(id));
  }

  onDeleted() {
    if (this.state.isBusy) return;
    let self = this;
    let id = this.props.list.id;
    invokeApi(this, () => deleteList(id)).then(() => self.props.onListRemoved(id));
  }

  onItemsDeleted(checkedOnly) {
    if (this.state.isBusy) return;
    let self = this;
    let list = this.props.list;
    invokeApi(this, () => deleteItems(list.id, checkedOnly)).then(() => self.props.onItemsRemoved(list));
  }

  onClosed() {
    if (this.state.isBusy) return;
    this.props.onClosed();
  }

  render = () => (
    <div className="list-modal">
      <div className="list-modal-content">
        <button type="button" className="btn-modal-close" onClick={this.onClosed}></button>
        <b className="pull-left">This cannot be undone!</b>
        <span className="pull-right">&nbsp;&nbsp;</span>
        <Status isBusy={this.state.isBusy} success={this.state.success} error={this.state.error} className="pull-right"/>
        <br className="clear-left"/>
        What would you like to do?<br/>
        <br/>
        <ul>
          <li>
            {(this.props.list.isShared ? (
              <span>
                <a href="#" onClick={this.onDisowned}>Disown this list</a><br/>
                The list will stop existing for you, but other people who share it will continue to access it.
              </span>
            ) : (
              <a href="#" onClick={this.onDeleted}>Delete this list</a>
            ))}
          </li>
          {(this.props.hasCheckedItems ? (
            <li>
              <a href="#" onClick={() => this.onItemsDeleted(true) }>Remove all checked items</a>
            </li>
          ) : (''))}
          {(this.props.hasItems ? (
            <li>
              <a href="#" onClick={() => this.onItemsDeleted(false) }>Remove all items</a>
            </li>
          ) : (''))}
          <li><a href="#" onClick={this.onClosed}>Do nothing</a></li>
        </ul>
      </div>
    </div>
  );
}
