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
import './Item.css';
import {postItem, putItem, deleteItem, putItemCheck, putItemUncheck} from './webApi';
import {invokeApi} from './common';
import Status from './Status';

export default class Item extends Component {
  constructor(props) {
    super(props);
    this.onDelete = this.onDelete.bind(this);
    this.onCheckToggled = this.onCheckToggled.bind(this);
    this.onItemEdit = this.onItemEdit.bind(this);
    this.onDescriptionChange = this.onDescriptionChange.bind(this);
    this.onSave = this.onSave.bind(this);
    this.onCancel = this.onCancel.bind(this);
    this.onKeyPress = this.onKeyPress.bind(this);
    this.onKeyDown = this.onKeyDown.bind(this);
    this.state = {
      isBusy: false,
      success: '',
      error: '',
      item: Object.assign(props.item),
      isEdit: props.item.id === 0,
      previousDescription: ''
    };
  }

  onDelete() {
    let self = this;
    let id = this.state.item.id;
    invokeApi(this, () => deleteItem(this.state.item.id)).then(() => self.props.onItemRemoved(id));
  }

  onCheckToggled() {
    let self = this;
    let item = Object.assign(this.state.item);
    let func = item.isChecked ? putItemUncheck : putItemCheck;
    item.isChecked = !item.isChecked;
    invokeApi(this, () => func(item.id), s => s.item = item).then(() => self.props.onItemsUpdated([item]));
  }

  onItemEdit = () => this.setState(s => ({ isEdit: true, previousDescription: s.item.description }), () => this.editBox.select());

  onDescriptionChange (event) {
    let value = event.target.value;
    this.setState(s => ({ item: Object.assign(s.item, { description: value } )}));
  }

  onSave() {
    let self = this;
    let item = Object.assign(this.state.item);
    if (item.id === 0) {
      invokeApi(this, () => postItem(item), s => s.isEdit = false).then(savedItems =>
        self.setState({ item: savedItems[0] }, () => self.props.onItemsUpdated(savedItems)));
      return;
    }

    let newItem = null;
    let descriptions = item.description.split(/[\r|\n]/).filter(d => d.trim() !== '');
    if (descriptions.length > 1) {
      newItem = { id: 0, description: descriptions.slice(1).join('\r\n'), listId: item.listId };
      item.description = descriptions[0];
    }

    invokeApi(this, () => putItem(item), s => s.isEdit = false).then(() => {
      let savedItem = Object.assign(item);
      self.setState({ item: savedItem }, () => {
        if (newItem == null) {
          self.props.onItemsUpdated([savedItem]);
          return;
        }
        invokeApi(this, () => postItem(newItem)).then(savedItems => {
          savedItems.push(savedItem);
          self.props.onItemsUpdated(savedItems);
        });
      });
    });
  }

  onCancel() {
    if (this.state.isBusy) return;
    let self = this;
    let isNew = this.state.item.id === 0;
    this.setState(s => ({ isEdit: false, item: Object.assign(s.item, { description: s.previousDescription}) }), () => {
      if (isNew) self.props.onItemRemoved(0);
    });
  }

  onKeyPress(event) {
    if (event.key !== 'Enter' || event.shiftKey) return;
    if (this.state.isBusy || this.state.item.description == null || this.state.item.description.trim() === '') return;
    this.onSave();
  }

  onKeyDown(event) {
    if (event.keyCode !== 27 || this.state.isBusy) return;
    this.onCancel();
  }

  render = () => (
    <div className="full-width">
      <button type="button" className={"btn-square btn-delete btn-item-action " + (this.state.isEdit ? "btn-item-offset" : "")}
        disabled={this.state.isBusy || this.state.isEdit}
        onClick={this.onDelete} title="Remove"></button>
      <button type="button" title={this.state.item.isChecked ? "Uncheck" : "Check"} disabled={this.state.isBusy || this.state.isEdit}
        className={"btn-square btn-item-action btn-item-" + (this.state.item.isChecked ? "checked" : "unchecked") + " " + (this.state.isEdit ? "btn-item-offset" : "")}
        onClick={this.onCheckToggled}></button>
      {this.state.isEdit ? (
        <div className="item-description-edit-container">
          <textarea value={this.state.item.description} onChange={this.onDescriptionChange} disabled={this.state.isBusy}
            className="item-description-edit" onKeyPress={this.onKeyPress} onKeyDown={this.onKeyDown}
            ref={r => this.editBox = r} autoFocus maxLength={2000}
            placeholder="Enter item description, enter multiple items in separate lines" />
          <button type="button" className={"btn-circle btn-yes " + (this.props.isSmall ? "hidden" : "")} title="Save"
            disabled={this.state.isBusy || this.state.item.description == null || this.state.item.description.trim() === ''}
            onClick={this.onSave}></button>
          <button type="button" className={"btn-circle btn-no " + (this.props.isSmall ? "hidden" : "")} title="Cancel"
            disabled={this.state.isBusy} onClick={this.onCancel}></button>
          <br className={this.props.isSmall ? "hidden" : ""}/>&nbsp;
          <Status isBusy={this.state.isBusy} success={this.state.success} error={this.state.error} className={this.props.isSmall ? "hidden" : ""}/>
        </div>
      ) : (
        <a href="#" onClick={this.onItemEdit} title="Click to edit"
          className={"item-description " + (this.state.item.isChecked ? "item-description-checked" : "") }>
          {this.state.item.description}
        </a>
      )}
    </div>
  );
}
