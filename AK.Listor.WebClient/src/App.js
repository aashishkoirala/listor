/*******************************************************************************************************************************
 * AK.Listor.WebClient.App.js
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
import './App.css';
import '../node_modules/bootstrap/dist/css/bootstrap.min.css';
import {invokeApi} from './common';
import {getLists} from './webApi';
import Status from './Status';
import UserName from './UserName';
import ListMenu from './ListMenu';
import List from './List';
import AccountSettings from './AccountSettings';

export default class App extends Component {
  constructor(props) {
    super(props);
    this.onListSelected = this.onListSelected.bind(this);
    this.onNewListAddRequested = this.onNewListAddRequested.bind(this);
    this.onNewListAddCancelled = this.onNewListAddCancelled.bind(this);
    this.onListAddedOrUpdated = this.onListAddedOrUpdated.bind(this);
    this.onListRemoved = this.onListRemoved.bind(this);
    this.onAccountSettingsSelected = this.onAccountSettingsSelected.bind(this);
    this.onUsernameChanged = this.onUsernameChanged.bind(this);
    this.state = {
      isBusy: false,
      error: '',
      success: '',
      mode: '',
      lists: [],
      list: null,
      userName: null,
      isSmall: false
    };
  }

  componentDidMount() {
    let self = this;
    window.addEventListener('resize', e => self.onResize(e.target.innerWidth));
    this.onResize(window.innerWidth);
    invokeApi(this, () => getLists()).then(lists => {
      lists.sort((l1, l2) => l1.name > l2.name);
      self.setState({ lists: lists });
    });
  }

  componentWillUnmount = () => window.removeEventListener('resize');

  onResize (width) {
    if (width < 768 && !this.state.isSmall) {
      this.setState( { isSmall: true });
      return;
    }
    if (width >= 768 && this.state.isSmall) {
      this.setState({ isSmall: false });
    }
  }

  onListSelected = list => this.setState({ list: list, mode: 'list-detail' });
  onNewListAddRequested = () => this.setState({ list: { id: 0, name: ''}, mode: 'list-detail' });
  onNewListAddCancelled = () => this.setState({ mode: '', list: null });

  onListAddedOrUpdated(list) {
    let index = this.state.lists.findIndex(l => l.id === list.id);
    let newList = { id: list.id, name: list.name };
    let newLists = null;
    if (index < 0) {
      newLists = [...this.state.lists, newList];
    } else {
      newLists = [...this.state.lists];
      newLists[index] = newList;
    }
    newLists.sort((l1, l2) => l1.name > l2.name);
    this.setState({ lists: newLists, list: newList });
  }

  onListRemoved(id) {
    let index = this.state.lists.findIndex(l => l.id === id);
    if (index < 0) return;
    let newLists = this.state.lists.filter(l => l.id !== id);
    this.setState({ lists: newLists, list: null, mode: '' });
  }

  onAccountSettingsSelected = name => this.setState({ userName: name, mode:'account-settings' });
  onUsernameChanged = name => this.setState({ userName: name });

  getMainContent() {
    switch (this.state.mode) {
      case 'list-detail':
        return (<List list={this.state.list} onNewListAddCancelled={this.onNewListAddCancelled}
          onListAddedOrUpdated={this.onListAddedOrUpdated} onListRemoved={this.onListRemoved} isSmall={this.state.isSmall}/>);
      case 'account-settings':
        return (<AccountSettings name={this.state.userName} onUsernameChanged={this.onUsernameChanged} isSmall={this.state.isSmall}/>);
      default:
        return this.state.isSmall ? (
          <ListMenu lists={this.state.lists} activeList={this.state.list}
            onListSelected={this.onListSelected} onNewListAddRequested={this.onNewListAddRequested}/>
        ) : (<div></div>);
    }
  }

  render = () => (
    <div className="row full-height">
      {!this.state.isSmall ? (
        <div className="col-sm-4 visible-sm visible-md visible-lg left-panel full-height">
          <div className="app-title">listor</div>
          <Status isBusy={this.state.isBusy} error={this.state.error} success={this.state.success} className="pull-left left-panel-status" />
          <UserName onAccountSettingsSelected={this.onAccountSettingsSelected} userName={this.state.userName} isSmall={false}/>
          <div className="clear-right"></div>
          <hr/>
          <br/>
          <ListMenu lists={this.state.lists} activeList={this.state.list}
            onListSelected={this.onListSelected} onNewListAddRequested={this.onNewListAddRequested}/>
        </div>
      ) : (<div></div>)}
      {!this.state.isSmall ? (
        <div className="col-sm-8">
          {this.getMainContent()}
        </div>
      ) : (
        <div className="col-xs-12">
          <div>
            <div className="title-bar-small">
              <button type="button" className="btn-square btn-home" title="Home"
                onClick={() => this.setState({ mode: '', list: null })}></button>
              <span className="app-title-small">listor</span>
              <UserName onAccountSettingsSelected={this.onAccountSettingsSelected} userName={this.state.userName} isSmall={true}/>
            </div>
            <div className="container-small">
              {this.getMainContent()}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
