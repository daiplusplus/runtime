// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    // This class implements a text writer that writes to a string buffer and allows
    // the resulting sequence of characters to be presented as a string.
    public class StringWriter : TextWriter
    {
        private static volatile UnicodeEncoding? s_encoding;

        private readonly StringBuilder _sb;
        private bool _isOpen;

        // Constructs a new StringWriter. A new StringBuilder is automatically
        // created and associated with the new StringWriter.
        public StringWriter()
            : this(new StringBuilder(), CultureInfo.CurrentCulture)
        {
        }

        public StringWriter(IFormatProvider? formatProvider)
            : this(new StringBuilder(), formatProvider)
        {
        }

        // Constructs a new StringWriter that writes to the given StringBuilder.
        //
        public StringWriter(StringBuilder sb) : this(sb, CultureInfo.CurrentCulture)
        {
        }

        public StringWriter(StringBuilder sb!!, IFormatProvider? formatProvider) : base(formatProvider)
        {
            _sb = sb;
            _isOpen = true;
        }

        public override void Close()
        {
            Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            // Do not destroy _sb, so that we can extract this after we are
            // done writing (similar to MemoryStream's GetBuffer & ToArray methods)
            _isOpen = false;
            base.Dispose(disposing);
        }


        public override Encoding Encoding
        {
            get
            {
                if (s_encoding == null)
                {
                    s_encoding = new UnicodeEncoding(false, false);
                }
                return s_encoding;
            }
        }

        // Returns the underlying StringBuilder. This is either the StringBuilder
        // that was passed to the constructor, or the StringBuilder that was
        // automatically created.
        //
        public virtual StringBuilder GetStringBuilder()
        {
            return _sb;
        }

        // Writes a character to the underlying string buffer.
        //
        public override void Write(char value)
        {
            if (!_isOpen)
            {
                throw new ObjectDisposedException(null, SR.ObjectDisposed_WriterClosed);
            }

            _sb.Append(value);
        }

        // Writes a range of a character array to the underlying string buffer.
        // This method will write count characters of data into this
        // StringWriter from the buffer character array starting at position
        // index.
        //
        public override void Write(char[] buffer!!, int index, int count)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), SR.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), SR.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (buffer.Length - index < count)
            {
                throw new ArgumentException(SR.Argument_InvalidOffLen);
            }
            if (!_isOpen)
            {
                throw new ObjectDisposedException(null, SR.ObjectDisposed_WriterClosed);
            }

            _sb.Append(buffer, index, count);
        }

        public override void Write(ReadOnlySpan<char> buffer)
        {
            if (GetType() != typeof(StringWriter))
            {
                // This overload was added after the Write(char[], ...) overload, and so in case
                // a derived type may have overridden it, we need to delegate to it, which the base does.
                base.Write(buffer);
                return;
            }

            if (!_isOpen)
            {
                throw new ObjectDisposedException(null, SR.ObjectDisposed_WriterClosed);
            }

            _sb.Append(buffer);
        }

        // Writes a string to the underlying string buffer. If the given string is
        // null, nothing is written.
        //
        public override void Write(string? value)
        {
            if (!_isOpen)
            {
                throw new ObjectDisposedException(null, SR.ObjectDisposed_WriterClosed);
            }

            if (value != null)
            {
                _sb.Append(value);
            }
        }

        public override void Write(StringBuilder? value)
        {
            // This overload was added after the Write(char[], ...) overload, and so in case
            // a derived type may have overridden it, we need to delegate to it, which the base does.
            if (UseBase())
            {
                base.Write(value);
            }
            else
            {
                _sb.Append(value);
            }
        }

        #region Write(format) + WriteLine(format)

        [System.Runtime.CompilerServices.MethodImpl(Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool UseBase()
        {
            if (!_isOpen)
            {
                throw new ObjectDisposedException(null, SR.ObjectDisposed_WriterClosed);
            }

            return GetType() != typeof(StringWriter);
        }

        public override void Write(string format, params object?[] arg)
        {
            if (UseBase())
            {
                base.Write(format, arg: arg);
            }
            else
            {
                _sb.AppendFormat(format, args: arg);
            }
        }

        public override void Write(string format, object? arg0)
        {
            if (UseBase())
            {
                base.Write(format, arg0: arg0);
            }
            else
            {
                _sb.AppendFormat(format, arg0: arg0);
            }
        }

        public override void Write(string format, object? arg0, object? arg1)
        {
            if (UseBase())
            {
                base.Write(format, arg0: arg0, arg1: arg1);
            }
            else
            {
                _sb.AppendFormat(format, arg0: arg0, arg1: arg1);
            }
        }

        public override void Write(string format, object? arg0, object? arg1, object? arg2)
        {
            if (UseBase())
            {
                base.Write(format, arg0: arg0, arg1: arg1, arg2: arg2);
            }
            else
            {
                _sb.AppendFormat(format, arg0: arg0, arg1: arg1, arg2: arg2);
            }
        }

        public override void WriteLine(string format, params object?[] arg)
        {
            if (UseBase())
            {
                base.WriteLine(format, arg: arg);
            }
            else
            {
                _sb.AppendFormat(format, args: arg);
                _sb.AppendLine();
            }
        }

        public override void WriteLine(string format, object? arg0)
        {
            if (UseBase())
            {
                base.WriteLine(format, arg0: arg0);
            }
            else
            {
                _sb.AppendFormat(format, arg0: arg0);
                _sb.AppendLine();
            }
        }

        public override void WriteLine(string format, object? arg0, object? arg1)
        {
            if (UseBase())
            {
                base.WriteLine(format, arg0: arg0, arg1: arg1);
            }
            else
            {
                _sb.AppendFormat(format, arg0: arg0, arg1: arg1);
                _sb.AppendLine();
            }
        }

        public override void WriteLine(string format, object? arg0, object? arg1, object? arg2)
        {
            if (UseBase())
            {
                base.WriteLine(format, arg0: arg0, arg1: arg1, arg2: arg2);
            }
            else
            {
                _sb.AppendFormat(format, arg0: arg0, arg1: arg1, arg2: arg2);
                _sb.AppendLine();
            }
        }

        #endregion

        public override void WriteLine(ReadOnlySpan<char> buffer)
        {
            if (GetType() != typeof(StringWriter))
            {
                // This overload was added after the WriteLine(char[], ...) overload, and so in case
                // a derived type may have overridden it, we need to delegate to it, which the base does.
                base.WriteLine(buffer);
                return;
            }

            if (!_isOpen)
            {
                throw new ObjectDisposedException(null, SR.ObjectDisposed_WriterClosed);
            }

            _sb.Append(buffer);
            WriteLine();
        }

        public override void WriteLine(StringBuilder? value)
        {
            if (GetType() != typeof(StringWriter))
            {
                // This overload was added after the WriteLine(char[], ...) overload, and so in case
                // a derived type may have overridden it, we need to delegate to it, which the base does.
                base.WriteLine(value);
                return;
            }

            if (!_isOpen)
            {
                throw new ObjectDisposedException(null, SR.ObjectDisposed_WriterClosed);
            }

            _sb.Append(value);
            WriteLine();
        }

        #region Task based Async APIs

        public override Task WriteAsync(char value)
        {
            Write(value);
            return Task.CompletedTask;
        }

        public override Task WriteAsync(string? value)
        {
            Write(value);
            return Task.CompletedTask;
        }

        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            Write(buffer, index, count);
            return Task.CompletedTask;
        }

        public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            Write(buffer.Span);
            return Task.CompletedTask;
        }

        public override Task WriteAsync(StringBuilder? value, CancellationToken cancellationToken = default)
        {
            if (GetType() != typeof(StringWriter))
            {
                // This overload was added after the WriteAsync(char[], ...) overload, and so in case
                // a derived type may have overridden it, we need to delegate to it, which the base does.
                return base.WriteAsync(value, cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            if (!_isOpen)
            {
                throw new ObjectDisposedException(null, SR.ObjectDisposed_WriterClosed);
            }

            _sb.Append(value);
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync(char value)
        {
            WriteLine(value);
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync(string? value)
        {
            WriteLine(value);
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync(StringBuilder? value, CancellationToken cancellationToken = default)
        {
            if (GetType() != typeof(StringWriter))
            {
                // This overload was added after the WriteLineAsync(char[], ...) overload, and so in case
                // a derived type may have overridden it, we need to delegate to it, which the base does.
                return base.WriteLineAsync(value, cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            if (!_isOpen)
            {
                throw new ObjectDisposedException(null, SR.ObjectDisposed_WriterClosed);
            }

            _sb.Append(value);
            WriteLine();
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync(char[] buffer, int index, int count)
        {
            WriteLine(buffer, index, count);
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            WriteLine(buffer.Span);
            return Task.CompletedTask;
        }

        public override Task FlushAsync()
        {
            return Task.CompletedTask;
        }

        #endregion

        // Returns a string containing the characters written to this TextWriter so far.
        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}
