/*******************************************************************************************************************************
 * AK.Listor.WebClient.webApi.js
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

const apiBase = () =>
  window.location.hostname === 'dev.loc' ? 'http://dev.loc:5858/api/' : '/api/';

const handleResponse = r => {
  if (r.ok) return r.json();
  let ct = r.headers.get("content-type");
  if (ct && ct.includes("application/json")) {
    return r.json().then(d => {
      throw { error: d.error };
    });
  } else {
    return new Promise(() => {
      throw { error: 'Sorry, something went wrong.' };
    });
  }
};

const handleError = e => {
  let s = 'Sorry, something went wrong.';
  if (e.error != null && typeof e.error === 'string') s = e.error;
  else console.log(e);
  return new Promise(() => {
    throw { error: s };
  });
};

const getJson = url =>
  fetch(apiBase() + url, {
    credentials: 'include',
    headers: {
      'Accept': 'application/json'
    }
  }).then(handleResponse).catch(handleError);

const sendJson = (url, method, body) =>
  fetch(apiBase() + url, {
    credentials: 'include',
    method: method,
    body: body != null ? JSON.stringify(body) : null,
    headers: {
      'Accept': 'application/json',
      'Content-Type': 'application/json'
    }
  }).then(handleResponse).catch(handleError);

export const getUser = () => getJson('user');
export const putUser = user => sendJson('user', 'PUT', user);
export const putUserChangePassword = change => sendJson('user/changePassword', 'PUT', change);
export const deleteUser = () => sendJson('user', 'DELETE', null);

export const getLists = () => getJson('list');
export const getList = id => getJson('list/' + id);
export const postList = list => sendJson('list', 'POST', list);
export const putList = list => sendJson('list/' + list.id, 'PUT', list);
export const putListShare = (listId, userName) => sendJson('list/' + listId + '/shareWith/' + userName, 'PUT', null);
export const putListDisown = listId => sendJson('list/' + listId + '/disown', 'PUT', null);
export const deleteList = listId => sendJson('list/' + listId, 'DELETE', null);

export const getItems = listId => getJson('item?listId=' + listId);
export const getItem = id => getJson('item/' + id);
export const postItem = item => sendJson('item', 'POST', item);
export const putItem = item => sendJson('item/' + item.id, 'PUT', item);
export const putItemCheck = id => sendJson('item/' + id + '/check', 'PUT', null);
export const putItemUncheck = id => sendJson('item/' + id + '/uncheck', 'PUT', null);
export const deleteItem = id => sendJson('item/' + id, 'DELETE', null);
export const deleteItems = (listId, checkedOnly) => sendJson('item?listId=' + listId + '&checkedOnly=' + (checkedOnly ? 'true' : 'false'), 'DELETE', null);
