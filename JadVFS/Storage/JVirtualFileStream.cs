#region Using Directives

using System;
using System.IO;

#endregion

namespace JadEngine.VFS.Storage
{
	/// <summary>
	/// Represents a virtual file. A virtual file is represented by a special stream
	/// that controls the access to a base <see cref="FileStream"/> (the
	/// <see cref="JVirtualFileStream"/> is a decorator).
	/// </summary>
	/// <remarks>
	/// This stream only accepts Read operations
	/// </remarks>
	public sealed class JVirtualFileStream : Stream
	{
		#region Fields

		/// <summary>
		/// Base file stream
		/// </summary>
		private FileStream _baseStream;

		/// <summary>
		/// Start point of the <see cref="JVirtualFileStream"/> around the stream
		/// </summary>
		private long _start;

		/// <summary>
		/// End point of the <see cref="JVirtualFileStream"/> around the stream
		/// </summary>
		private long _end;

		/// <summary>
		/// Length of the <see cref="JVirtualFileStream"/>
		/// </summary>
		private long _length;

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value indicating whether the current stream supports reading.
		/// </summary>
		public override bool CanRead
		{
			get { return _baseStream.CanRead; }
		}

		/// <summary>
		/// Gets a value indicating whether the current stream supports seeking.
		/// </summary>
		public override bool CanSeek
		{
			get { return _baseStream.CanSeek; }
		}

		/// <summary>
		///   	Gets a value indicating whether the current stream supports writing.
		/// </summary>
		/// <remarks>
		/// It always returns false
		/// </remarks>
		public override bool CanWrite
		{
			get { return false; }
		}

		/// <summary>
		/// Gets the length in bytes of the stream.
		/// </summary>
		public override long Length
		{
			get { return _length; }
		}

		/// <summary>
		/// Gets or sets the current position of this stream.
		/// </summary>
		public override long Position
		{
			get { return _baseStream.Position; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("Attempted moving before the beginning of the stream.");

				if (value >= _length)
					throw new EndOfStreamException("Attempted moving after the end of the stream.");

				_baseStream.Position = _start + value;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="baseStream">Base stream decorated by the <see cref="JVirtualFileStream"/>.</param>
		/// <param name="offset">Offset on the base stream where the <see cref="JVirtualFileStream"/> starts.</param>
		/// <param name="length">Length of the <see cref="JVirtualFileStream"/>.</param>
		internal JVirtualFileStream(FileStream baseStream, long offset, long length)
		{
			if (baseStream == null)
				throw new IOException("The base stream for the virtual stream can't be null.");

			_baseStream = baseStream;
			_start = offset;
			_length = length;

			_end = _start + length;
            baseStream.Seek(_start,SeekOrigin.Begin);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Clears all buffers for this stream and causes any buffered data to be written to the file system.
		/// </summary>
		public override void Flush()
		{
			_baseStream.Flush();
		}

		/// <summary>
		/// Reads a block of bytes from the stream and writes the data in a given buffer.
		/// </summary>
		/// <param name="buffer">
		/// When this method returns, contains the specified byte array with the values
		/// between offset and (offset + count - 1) replaced by the bytes read from the
		/// current source.
		/// </param>
		/// <param name="offset">The byte offset in array at which the read bytes will be placed.</param>
		/// <param name="count">The maximum number of bytes to read.</param>
		/// <returns>
		/// The total number of bytes read into the buffer. This might be less than the number 
		/// of bytes requested if that number of bytes are not currently available, or zero if
		/// the end of the stream is reached.
		/// </returns>
		public override int Read(byte[] buffer, int offset, int count)
		{
			// Some checks
			if (offset < 0)
				throw new ArgumentOutOfRangeException("The offset to read in the stream must be equal or greater than 0.");

			if (count < 0)
				throw new ArgumentOutOfRangeException("The count of bytes to read from the stream must be equal or greater than 0.");

			if (_baseStream.Position + offset < _start)
				throw new ArgumentException("Attempted reading before the beginning of the stream.");

			if (_baseStream.Position + offset > _end)
				throw new ArgumentException("Attempted reading after the end of the stream.");

			// If trying to read more than the remaining bytes, truncate it
			if (_baseStream.Position + offset + count > _end)
				count = (int) (_end - (_baseStream.Position + offset));

			return _baseStream.Read(buffer, offset, count);
		}

		/// <summary>
		/// Reads a byte from the stream and advances the read position one byte.
		/// </summary>
		/// <returns>The byte, cast to an Int32, or -1 if the end of the stream has been reached.</returns>
		public override int ReadByte()
		{
			if (_baseStream.Position >= _end)
				return -1;

			return _baseStream.ReadByte();
		}

		/// <summary>
		/// Sets the current position of this stream to the given value.
		/// </summary>
		/// <param name="offset">The point relative to origin from which to begin seeking.</param>
		/// <param name="origin">
		/// Specifies the beginning, the end, or the current position as a reference
		/// point for origin, using a value of type <see cref="SeekOrigin"/>.
		/// </param>
		/// <returns>The new position in the stream.</returns>
		public override long Seek(long offset, SeekOrigin origin)
		{
			if (origin == SeekOrigin.Begin)
			{
				if (offset < 0)
					throw new ArgumentException("Attempted seeking before the beginning of the stream.");

				if (offset >= _length)
					throw new ArgumentException("Attempted seeking after the end of the stream.");

				return _baseStream.Seek(_start + offset, SeekOrigin.Begin);
			}

			if (origin == SeekOrigin.Current)
			{
				if (_baseStream.Position + offset < _start)
					throw new ArgumentException("Attempted seeking before the start of the stream.");

				if (_baseStream.Position + offset >= _end)
					throw new ArgumentException("Attempted seeking after the end of the stream.");

				return _baseStream.Seek(offset, SeekOrigin.Current);
			}

			if (origin == SeekOrigin.End)
			{
				if (offset > 0)
					throw new ArgumentException("Attempted seeking after the end of the stream.");

				if (offset < -_length)
					throw new ArgumentException("Attempted seeking before the start of the stream.");

				return _baseStream.Seek(offset, SeekOrigin.End);
			}

			return 0;
		}

		/// <summary>
		/// Sets the length of this stream to the given value.
		/// </summary>
		/// <param name="value">The new length of the stream.</param>
		/// <remarks>Not supported</remarks>
		public override void SetLength(long value)
		{
			throw new NotSupportedException("The stream does not support resizing");
		}

		/// <summary>
		/// Writes a block of bytes to this stream using data from a buffer.
		/// </summary>
		/// <param name="buffer">The buffer containing data to write to the stream.</param>
		/// <param name="offset">The zero-based byte offset in array at which to begin copying bytes to the current stream.</param>
		/// <param name="count">The maximum number of bytes to be written to the current stream.</param>
		/// <remarks>Not supported</remarks>
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException("The stream does not support writing");
		}

		/// <summary>
		/// Writes a byte to the current position in the file stream.
		/// </summary>
		/// <param name="value">A byte to write to the stream.</param>
		/// <remarks>Not supported</remarks>
		public override void WriteByte(byte value)
		{
			throw new NotSupportedException("The stream does not support writing");
		}

		#endregion
	}
}
