/*******************************************************************************************************************************
 * AK.Listor.WebClient.UserName.js
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

import React, { Component } from 'react';
import './UserName.css';
import {getUser} from './webApi';

export default class UserName extends Component {
  constructor(props) {
    super(props);
    this.onAccountSettingsSelected = this.onAccountSettingsSelected.bind(this);
    this.state = { userName: props.userName };
  }

  componentDidMount() {
    let self = this;
    getUser()
    .then(u => self.setState({ userName: u.name }))
    .catch(e => {
      self.setState({ userName: 'N/A' });
      console.log(e);
    });
  }

  componentWillReceiveProps(nextProps) {
    if (nextProps.userName == null) return;
    if (nextProps.userName === this.state.userName) return;
    this.setState({ userName: nextProps.userName });
  }

  onAccountSettingsSelected = () => this.props.onAccountSettingsSelected(this.state.userName);

  render = () => this.props.isSmall ? (
      <a href="#" className="pull-right user-name-small" onClick={this.onAccountSettingsSelected}>{this.state.userName || 'N/A'}</a>
    ) : (
      <div className="pull-right user-name-container">
        <div className="user-name text-right">
          <a href="#" onClick={this.onAccountSettingsSelected}>{this.state.userName || 'N/A'}</a>
        </div>
      </div>
  );
}
