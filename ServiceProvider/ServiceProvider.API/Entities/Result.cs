using ServiceProvider.API.Enums;

namespace ServiceProvider.API.Entities
{
    public class Result<T>
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public ResultStatus Status { get; set; }

        /// <summary>
        ///     Gets a value indicating whether is success.
        /// </summary>
        public bool IsSuccess => this.Status == ResultStatus.Success;

        /// <summary>
        ///     Gets a value indicating whether is failure.
        /// </summary>
        public bool IsFailure => this.Status == ResultStatus.Failure;
    }
}
