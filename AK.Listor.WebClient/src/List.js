/*******************************************************************************************************************************
 * AK.Listor.WebClient.List.js
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
import './List.css';
import {invokeApi} from './common';
import {getItems, getList} from './webApi';
import ListTitle from './ListTitle';
import ListDelete from './ListDelete';
import ListShare from './ListShare';
import Item from './Item';

export default class List extends Component {
  constructor(props) {
    super(props);
    this.onNewListAddCancelled = this.props.onNewListAddCancelled.bind(this);
    this.onListAddedOrUpdated = this.props.onListAddedOrUpdated.bind(this);
    this.onListRemoved = this.props.onListRemoved.bind(this);
    this.onDeleteModalToggled = this.onDeleteModalToggled.bind(this);
    this.onShareModalToggled = this.onShareModalToggled.bind(this);
    this.onListShared = this.onListShared.bind(this);
    this.onItemsRemoved = this.onItemsRemoved.bind(this);
    this.onItemAdded = this.onItemAdded.bind(this);
    this.onItemRemoved = this.onItemRemoved.bind(this);
    this.onItemsUpdated = this.onItemsUpdated.bind(this);
    this.state = {
      items: [],
      list: Object.assign(props.list),
      isDeleteModalShown: false,
      isShareModalShown: false
    };
  }

  componentDidMount = () => this.loadList(this.props.list);
  componentWillReceiveProps = nextProps => this.loadList(nextProps.list);

  loadList (list) {
    if (list.id === 0) {
      this.setState({
        list: {
          id: 0,
          name: list.name,
          isShared: false
        },
        items: []
      });
      return;
    }

    let self = this;
    invokeApi(this, () => getList(list.id)).then(({id, name, isShared}) =>
      self.setState({ list: { id: id, name: name, isShared: isShared }}, self.loadItems));
  }

  loadItems() {
    let self = this;
    invokeApi(this, () => getItems(self.state.list.id)).then(items => {
      self.setState({ items: [...items] });
    });
  }

  hasCheckedItems = () => this.state.items.filter(i => i.isChecked).length > 0;

  onDeleteModalToggled = () => this.setState(s => ({ isDeleteModalShown: !s.isDeleteModalShown }));
  onShareModalToggled = () => this.setState(s => ({ isShareModalShown: !s.isShareModalShown }));

  onItemsRemoved (list) {
    let self = this;
    this.setState( { isDeleteModalShown: false }, () => self.loadList(list));
  }

  onListShared (id) {
    if (this.state.list.id !== id || this.state.list.isShared) return;
    this.setState(s => ({ list: Object.assign(s.list, { isShared: true }), isShareModalShown: false }));
  }

  onItemAdded = () => this.setState(s => ({ items: [...s.items, { id: 0, listId: s.list.id, description: '', isChecked: false }] }));

  onItemRemoved (id) {
    let index = this.state.items.findIndex(i => i.id === id);
    if (index < 0) return;
    let newItems = this.state.items.filter(i => i.id !== id);
    this.setState({ items: newItems });
  }

  onItemsUpdated (items) {
    let existingItems = this.state.items.filter(i => i.id !== 0);
    items.forEach(i => {
      let index = existingItems.findIndex(ei => ei.id === i.id);
      let newItem = Object.assign(i);
      if (index < 0) existingItems.push(newItem);
      else existingItems[index] = newItem;
    });
    this.setState({ items: existingItems });
  }

  render = () => (
    <div className="full-width">
      <ListTitle list={this.state.list} onNewListAddCancelled={this.onNewListAddCancelled}
        onListAddedOrUpdated={this.onListAddedOrUpdated} onDeleteClicked={this.onDeleteModalToggled}
        onShareClicked={this.onShareModalToggled} isSmall={this.props.isSmall}/>
      {this.state.isDeleteModalShown ? (
        <ListDelete list={this.state.list} hasItems={this.state.items.length > 0}
          hasCheckedItems={this.hasCheckedItems()} onListRemoved={this.onListRemoved}
          onItemsRemoved={this.onItemsRemoved} onClosed={this.onDeleteModalToggled} />
      ) : (<span></span>)}
      {this.state.isShareModalShown ? (
        <ListShare listId={this.state.list.id} onListShared={this.onListShared} onClosed={this.onShareModalToggled} />
      ) : (<span></span>)}
      <div className="full-width">
       {this.state.items.map(i => <Item key={i.id} item={i} onItemsUpdated={this.onItemsUpdated}
         onItemRemoved={this.onItemRemoved} isSmall={this.props.isSmall}/>)}
      </div>
      <br/>
      <button type="button" className="btn-circle btn-add" onClick={this.onItemAdded}></button>
    </div>
  );
}
