/*******************************************************************************************************************************
 * AK.Listor.WebClient.Status.js
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

const getStyle = (isBusy, success, error) => {
  if (isBusy || success !== '') return 'text-info';
  else return error !== '' ? 'text-danger' : '';
};

const getText = (isBusy, success, error) => {
  if (success !== '') return success;
  if (isBusy) return 'Working...';
  return error !== '' ? error : '';
};

export default ({ isBusy, success, error, className }) => (
  <span className={className + " " + getStyle(isBusy, success, error)}>
    {getText(isBusy, success, error)}
  </span>
);
