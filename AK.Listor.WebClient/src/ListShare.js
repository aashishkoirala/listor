/*******************************************************************************************************************************
 * AK.Listor.WebClient.ListShare.js
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
import {putListShare} from './webApi';
import Status from './Status';

export default class ListShare extends Component {
  constructor(props) {
    super(props);
    this.state = { isBusy: false, success: '', error: '', userName: '' };
    this.onChange = this.onChange.bind(this);
    this.onShared = this.onShared.bind(this);
    this.onKeyPress = this.onKeyPress.bind(this);
    this.onKeyDown = this.onKeyDown.bind(this);
    this.onClosed = this.onClosed.bind(this);
  }

  onChange = event => this.setState({ userName: event.target.value });

  onShared() {
    if (this.state.isBusy) return;
    let self = this;
    let userName = this.state.userName;
    invokeApi(this, () => putListShare(this.props.listId, userName), s => {
      s.success = 'List successfully shared with ' + userName + '.';
      s.userName = '';
    }, s => s.userName = '').then(() => self.props.onListShared(self.props.listId));
  }

  onClosed() {
    if (this.state.isBusy) return;
    this.props.onClosed();
  }

  onKeyPress(event) {
    if (event.key !== 'Enter') return;
    if (this.state.isBusy || this.state.userName.trim() === '') return;
    this.onShared();
  }

  onKeyDown(event) {
    if (event.keyCode !== 27) return;
    this.setState({ userName: '' });
  }

  render = () => (
    <div className="list-modal">
      <div className="list-modal-content">
        <button type="button" className="btn-modal-close" onClick={this.onClosed} disabled={this.state.isBusy}></button>
        <b className="pull-left">Share this list</b>
        <span className="pull-right">&nbsp;&nbsp;</span>
        <Status isBusy={this.state.isBusy} success={this.state.success} error={this.state.error} className="pull-right"/>
        <br className="clear-left"/>
        Type in the username of the person you want to share this list with.<br/>
        They will co-own the list with you and will have equal access.
        <br/><br/>
        <input type="text" className="list-share-textbox" autoFocus value={this.state.userName}
          onChange={this.onChange} onKeyPress={this.onKeyPress} onKeyDown={this.onKeyDown}
          placeholder="Enter username to share with" disabled={this.state.isBusy} maxLength={25}/>
        <button type="button" className="btn-circle btn-yes"
          disabled={this.state.userName.trim() === '' || this.state.isBusy}
          title="Share list" onClick={this.onShared}></button>
        <button type="button" className="btn-circle btn-no" title="Cancel" disabled={this.state.isBusy} onClick={this.onClosed}></button>
      </div>
    </div>
  );
}
