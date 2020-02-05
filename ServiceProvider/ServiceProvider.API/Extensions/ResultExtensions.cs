using ServiceProvider.API.Entities;
using ServiceProvider.API.Enums;

namespace ServiceProvider.API.Extensions
{
    public static class ResultExtensions
    {
        /// <summary>
        /// The create success.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <typeparam name="T">
        /// Type of parameter.
        /// </typeparam>
        /// <returns>
        /// The <see cref="Result{T}"/>.
        /// </returns>
        public static Result<T> CreateSuccess<T>(this T value)
        {
            return new Result<T> { Status = ResultStatus.Success, Value = value };
        }

        /// <summary>
        /// The create success.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <typeparam name="T">
        /// Type of parameter.
        /// </typeparam>
        /// <returns>
        /// The <see cref="Result{T}"/>.
        /// </returns>
        public static Result<T> CreateFailure<T>(this T value)
        {
            return new Result<T> { Status = ResultStatus.Failure, Value = value };
        }
    }
}
