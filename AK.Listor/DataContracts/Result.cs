/*******************************************************************************************************************************
 * AK.Listor.DataContracts.Result
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

namespace AK.Listor.DataContracts
{
    public enum ResultType
    {
        Success,
        Unauthorized,
        BadRequest,
        NotFound,
        Error
    }

    public class Result
    {
        public ResultType Type { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsSuccess => Type == ResultType.Success;

        public Result()
        {
            Type = ResultType.Success;
        }

        public static Result Success = new Result();

        public Result(ResultType type) : this("An error has occurred.", type)
        {
        }

        public Result(string errorMessage, ResultType type = ResultType.Error)
        {
            Type = type;
            ErrorMessage = errorMessage;
            if (Type == ResultType.Success) Type = ResultType.Error;
        }
    }

    public class Result<T> : Result
    {
        public T Value { get; set; }

        public Result(T value)
        {
            Value = value;
        }

        public Result(string errorMessage, ResultType type) : base(errorMessage, type)
        {
        }

        public Result(ResultType type) : this("An error has occurred.", type)
        {
        }

        public Result(Result error) : this(error.ErrorMessage, error.Type) { }
    }
}