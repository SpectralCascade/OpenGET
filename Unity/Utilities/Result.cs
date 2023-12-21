using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

	/// <summary>
	/// Generic error package class.
	/// </summary>
	public class Result<ValueType>
	{
		/// <summary>
		/// Create an error.
		/// </summary>
		public Result(string error, System.Exception exception = null)
		{
			this.error = error;
			this.exception = exception;
			this.value = default(ValueType);
		}

		/// <summary>
		/// Create a non-error result.
		/// </summary>
		public Result(ValueType value)
		{
			this.value = value;
			this.error = null;
			this.exception = null;
		}

		/// <summary>
		/// Is there an error? Does not necessarily mean there was an exception, only an error.
		/// </summary>
		public bool hasError { get { return !string.IsNullOrEmpty(error); } }

		/// <summary>
		/// In the case where there is an error, is there an exception?
		/// </summary>
		public bool hasException { get { return exception != null; } }

		/// <summary>
		/// Is there a non-error value?
		/// </summary>
		public bool hasValue { get { return !hasError; } }

		/// <summary>
		/// The error message, if any.
		/// </summary>
		public readonly string error;

		/// <summary>
		/// The exception, if any.
		/// </summary>
		public readonly System.Exception exception;

		/// <summary>
		/// The value returned if there was no error. If there was an error, this is a default value.
		/// </summary>
		public readonly ValueType value;

	}

	/// <summary>
	/// Override for generic success/failure results.
	/// </summary>
	public class Result : Result<bool>
	{
		public Result(string error, System.Exception exception = null) : base(error, exception) { }
		public Result(bool value = true) : base(value) { }

	}

}
