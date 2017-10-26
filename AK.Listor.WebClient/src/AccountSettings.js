/*******************************************************************************************************************************
 * AK.Listor.WebClient.Item.js
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
import './AccountSettings.css';
import {invokeApi} from './common';
import {putUser, putUserChangePassword, deleteUser} from './webApi';
import Status from './Status';

export default class AccountSettings extends Component {
  constructor(props) {
    super(props);
    this.onTextEntered = this.onTextEntered.bind(this);
    this.onKeyPress = this.onKeyPress.bind(this);
    this.onKeyDown = this.onKeyDown.bind(this);
    this.onChangeUsernameClick = this.onChangeUsernameClick.bind(this);
    this.onChangePasswordClick = this.onChangePasswordClick.bind(this);
    this.onDeleteAccountClick = this.onDeleteAccountClick.bind(this);
    this.onDeleteAccountConfirmClick = this.onDeleteAccountConfirmClick.bind(this);
    this.onDeleteAccountCancelClick = this.onDeleteAccountCancelClick.bind(this);
    this.state = {
      newUserName: '',
      currentPassword: '',
      newPassword: '',
      retypeNewPassword: '',
      isBusy: false,
      error: '',
      success: '',
      changePasswordAllowed: false,
      showDeleteAccount: true
    };
  }

  onTextEntered(stateMember, event) {
    let newState = {};
    newState[stateMember] = event.target.value;
    this.setState(newState, () => {
      if (stateMember !== 'newUserName') this.updateChangePasswordAllowed();
    });
  }

  updateChangePasswordAllowed() {
    let allowed = false;
    let error = '';

    if (this.state.currentPassword && this.state.newPassword &&
      this.state.retypeNewPassword) {

      if (this.state.newPassword.length < 12) {
        error = 'Password can be anything you want, but needs to be at least 12 characters.';
      } else if (this.state.newPassword !== this.state.retypeNewPassword) {
        error = 'Passwords do not match.';
      }

      allowed = error === '';
    }

    this.setState({ changePasswordAllowed: allowed, error: error });
  }

  onChangeUsernameClick() {
    let self = this;
    let user = { name: this.state.newUserName };
    invokeApi(this, () => putUser(user), s => {
      s.newUserName = '';
      s.success = 'Username changed successfully.';
    })
    .then(() => self.props.onUsernameChanged(user.name));
  }

  onChangePasswordClick() {
    let change = {
      currentPassword: this.state.currentPassword,
      newPassword: this.state.newPassword,
      retypeNewPassword: this.state.retypeNewPassword
    };
    invokeApi(this, () => putUserChangePassword(change),
      s => {
        s.currentPassword = '';
        s.newPassword = '';
        s.retypeNewPassword = '';
        s.success = 'Password changed successfully.';
      });
  }

  onKeyPress(member, event) {
    if (event.key !== 'Enter') return;
    switch (member) {
      case 'newUserName':
        if (this.state.isBusy || !this.state.newUserName || (this.state.newUserName === this.props.name)) return;
        this.onChangeUsernameClick();
        break;
      case 'currentPassword':
      case 'newPassword':
      case 'retypeNewPassword':
        if (this.state.isBusy || !this.state.changePasswordAllowed) return;
        this.onChangePasswordClick();
        break;
    }
  }

  onKeyDown(member, event) {
    if (event.keyCode !== 27) return;
    var newState = {};
    newState[member] = '';
    this.setState(newState);
  }

  onDeleteAccountClick = () => this.setState({ showDeleteAccount: false });
  onDeleteAccountCancelClick = () => this.setState({ showDeleteAccount: true });

  onDeleteAccountConfirmClick() {
    let self = this;
    invokeApi(this, () => deleteUser(), s => s.success = 'Account deleted, logging out...')
      .then(({redirectUrl}) => window.location.href = redirectUrl);
  }

  getDeleteAccountContent = () => this.state.showDeleteAccount ? (
    <button type="button" className="btn btn-sm btn-danger" onClick={this.onDeleteAccountClick}>Delete Account</button>) : (
    <div>
      Are you sure?&nbsp;&nbsp;
      <button type="button" className="btn-circle btn-yes" title="Confirm account deletion" onClick={this.onDeleteAccountConfirmClick}></button>
      <button type="button" className="btn-circle btn-no" title="Cancel account deletion" onClick={this.onDeleteAccountCancelClick}></button>
    </div>
  );

  render = () => (
    <div>
      {this.props.isSmall ? (<span></span>) : (<h3 className="pull-left">Account Settings - {this.props.name}</h3>)}
      {this.props.isSmall ? (<span></span>) : (<span className="pull-left">&nbsp;&nbsp;</span>)}
      <a className="pull-left account-settings-signout-link" href="account/logout">Sign Out</a>
      <span className="pull-left">&nbsp;&nbsp;</span>
      <Status isBusy={this.state.isBusy} error={this.state.error} success={this.state.success} className="pull-left account-settings-status" />
      <br className="clear-left"/>
      <h4>Change Username</h4>
      <input type="text" className="account-settings-textbox" placeholder="Enter new username"
        maxLength={25}
        disabled={this.state.isBusy} value={this.state.newUserName}
        onChange={e => this.onTextEntered('newUserName', e)}
        onKeyPress={e => this.onKeyPress('newUserName', e)}
        onKeyDown={e => this.onKeyDown('newUserName', e)}/>
        &nbsp;&nbsp;
      <button type="button" className="btn-circle btn-yes" title="Change Username"
        disabled={this.state.isBusy || !this.state.newUserName || (this.state.newUserName === this.props.name)}
        onClick={this.onChangeUsernameClick}></button>
      <br/><br/>
      <h4>Change Password</h4>
      <input type="password" className="account-settings-textbox" placeholder="Current password"
        maxLength={50}
        disabled={this.state.isBusy} value={this.state.currentPassword}
        onChange={e => this.onTextEntered('currentPassword', e)}
        onKeyPress={e => this.onKeyPress('currentPassword', e)}
        onKeyDown={e => this.onKeyDown('currentPassword', e)}/>
      <br/><br/>
      <input type="password" className="account-settings-textbox" placeholder="New password"
        maxLength={50}
        disabled={this.state.isBusy} value={this.state.newPassword}
        onChange={e => this.onTextEntered('newPassword', e)}
        onKeyPress={e => this.onKeyPress('newPassword', e)}
        onKeyDown={e => this.onKeyDown('newPassword', e)}/>
      <br/><br/>
      <input type="password" className="account-settings-textbox" placeholder="Retype new password"
        maxLength={50}
        disabled={this.state.isBusy} value={this.state.retypeNewPassword}
        onChange={e => this.onTextEntered('retypeNewPassword', e)}
        onKeyPress={e => this.onKeyPress('retypeNewPassword', e)}
        onKeyDown={e => this.onKeyDown('retypeNewPassword', e)}/>
      &nbsp;&nbsp;
      <button type="button" className="btn-circle btn-yes" title="Change Password"
        disabled={this.state.isBusy || !this.state.changePasswordAllowed} onClick={this.onChangePasswordClick}></button>
      <br/><br/>
      <h4>Delete Account</h4>
      <span>
        When you delete your account, all your information including your private lists and everything within
        them will be permanently deleted. This cannot be undone.<br/>
        Your shared lists will continue to exist for users that they are shared with.
      </span>
      <br/><br/>
      {this.getDeleteAccountContent()}
    </div>
  );
}
