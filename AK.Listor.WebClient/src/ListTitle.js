/*******************************************************************************************************************************
 * AK.Listor.WebClient.ListTitle.js
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
import './ListTitle.css';
import {invokeApi} from './common';
import {postList, putList} from './webApi';
import Status from './Status';

const SharedIndicator = ({isShared}) => isShared ? (
  <span className="list-title pull-left shared-indicator" title="This list is shared with others">&nbsp;
  </span>
) : (<span></span>);

export default class ListTitle extends Component {
  constructor(props) {
    super(props);
    this.onRename = this.onRename.bind(this);
    this.onChange = this.onChange.bind(this);
    this.onSave = this.onSave.bind(this);
    this.onCancel = this.onCancel.bind(this);
    this.onDeleteClicked = props.onDeleteClicked.bind(this);
    this.onShareClicked = props.onShareClicked.bind(this);
    this.onKeyPress = this.onKeyPress.bind(this);
    this.onKeyDown = this.onKeyDown.bind(this);
    this.state = {
      isBusy: false,
      error: '',
      success: '',
      list: Object.assign(props.list),
      isEdit: props.list.id === 0
    };
  }

  componentWillReceiveProps = nextProps =>
    this.setState({ list: Object.assign(nextProps.list), isEdit: nextProps.list.id === 0});

  onRename() {
    let self = this;
    this.setState(s => ({ isEdit: true, previousName: s.list.name }), () => self.textbox.select());
  }

  onChange = e => {
    let value = e.target.value;
    this.setState(s => ({ list: Object.assign(s.list, {name: value}) }));
  }

  onSave() {
    let self = this;
    let list = Object.assign(this.state.list);
    if (list.id === 0) {
      invokeApi(this, () => postList(list), s => {
        s.success = 'List saved successfully.';
        s.isEdit = false;
      }).then(({id}) => {
        list.id = id;
        self.props.onListAddedOrUpdated(list);
        self.setState({ list: list });
      });
    } else {
      invokeApi(this, () => putList(list), s => s.isEdit = false).then(
        () => self.props.onListAddedOrUpdated(list));
    }
  }

  onCancel() {
    if (this.state.list.id === 0) this.props.onNewListAddCancelled();
    else this.setState(s => ({ isEdit: false, list: Object.assign(s.list, { name: s.previousName }) }));
  }

  onKeyPress(event) {
    if (event.key !== 'Enter') return;
    if (this.state.isBusy || this.state.list.name == null || this.state.list.name.trim() === '') return;
    this.onSave();
  }

  onKeyDown(event) {
    if (event.keyCode !== 27) return;
    this.onCancel();
  }

  render = () => (
    <div>
        {this.state.isEdit ? (
        <div className="pull-left">
          <input autoFocus ref={t => this.textbox = t} type="textbox" className="list-title-edit" value={this.state.list.name}
            placeholder="Enter list name" onChange={this.onChange} disabled={this.state.isBusy}
            onKeyPress={this.onKeyPress} onKeyDown={this.onKeyDown} maxLength={25}/>
          <button type="button" className={"btn-circle btn-yes " + (this.props.isSmall ? "hidden" : "")} title="Save"
            disabled={this.state.isBusy || this.state.list.name == null || this.state.list.name.trim() === ''}
            onClick={this.onSave}></button>
          <button type="button" className={"btn-circle btn-no " + (this.props.isSmall ? "hidden" : "")} title="Cancel"
            disabled={this.state.isBusy} onClick={this.onCancel}></button>
        </div>
        ) : (
        <div className="pull-left">
          <SharedIndicator isShared={this.state.list.isShared} />
          <h3 className="pull-left">
            <a href="#" className="list-title" title="Click to rename" onClick={this.onRename}>{this.state.list.name}</a>
          </h3>
          <div className="pull-left list-title-button-container">
            <button type="button" className="btn-square btn-delete" title="See Options for Delete" disabled={this.state.isBusy}
              onClick={this.onDeleteClicked}></button>
            <button type="button" className="btn-square btn-share" title="Share List" disabled={this.state.isBusy}
              onClick={this.onShareClicked}></button>
          </div>
        </div>
      )}
      <Status isBusy={this.state.isBusy} success={this.state.success} error={this.state.error} className="pull-right list-title-status" />
      <br className="clear-left" />
      <br className="clear-right"/>
    </div>
  );
}
