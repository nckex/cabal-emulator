using System.Buffers;
using System.Net.Sockets;
using System.IO.Pipelines;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace HPT.Network
{
    public abstract class TcpConnection : ITcpConnection
    {
        protected readonly Socket _socket;

        private readonly Pipe _pipe;
        private readonly CancellationTokenSource _cancellationTokenSource;

        protected CancellationToken CancellationToken
        {
            get
            {
                return _cancellationTokenSource.Token;
            }
        }

        protected TcpConnection(Socket socket)
        {
            _socket = socket;
            _pipe = new Pipe();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async ValueTask<int> SendAsync(ArraySegment<byte> buffer)
        {
            return await _socket.SendAsync(buffer, SocketFlags.None, CancellationToken);
        }

        public void Disconnect()
        {
            if (_pipe != null)
            {
                _pipe.Reader.CancelPendingRead();
                _pipe.Writer.CancelPendingFlush();
            }

            if (_cancellationTokenSource.Token.CanBeCanceled)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        protected abstract int GetMessageSize(in ReadOnlySequence<byte> headerBytes);
        protected abstract Task ProcessMessageReceivedAsync(ReadOnlySequence<byte> messageBytes);

        protected virtual async Task OnConnect()
        {
            await Task.FromResult(0);
        }

        protected virtual async Task OnDisconnect()
        {
            await Task.FromResult(0);
        }

        public async Task ProcessPipeAsync(int headerSize)
        {
            await OnConnect();

            var writing = FillPipeAsync();
            var reading = ReadPipeAsync(headerSize);

            await Task.WhenAll(writing, reading);

            await OnDisconnect();
        }

        private async Task FillPipeAsync()
        {
            const int minimumBufferSize = 126;

            try
            {
                while (true)
                {
                    // Allocate at least 512 bytes from the PipeWriter.
                    Memory<byte> memory = _pipe.Writer.GetMemory(minimumBufferSize);

                    try
                    {
                        int bytesRead = await _socket.ReceiveAsync(memory, SocketFlags.None, CancellationToken);
                        if (bytesRead == 0)
                        {
                            // Socket disconnected.
                            break;
                        }

                        // Tell the PipeWriter how much was read from the Socket.
                        _pipe.Writer.Advance(bytesRead);
                    }
                    catch
                    {
                        break;
                    }

                    // Make the data available to the PipeReader.
                    FlushResult result = await _pipe.Writer.FlushAsync();

                    if (result.IsCanceled || result.IsCompleted)
                    {
                        break;
                    }
                }
            }
            finally
            {
                await _pipe.Writer.CompleteAsync();
            }
        }

        private async Task ReadPipeAsync(int headerSize)
        {
            try
            {
                while (true)
                {
                    ReadResult result = await _pipe.Reader.ReadAsync();
                    ReadOnlySequence<byte> buffer = result.Buffer;

                    try
                    {
                        if (result.IsCanceled)
                        {
                            // Read canceled
                            break;
                        }

                        while (TryGetMessage(ref buffer, headerSize, out var messageBytes))
                        {
                            await ProcessMessageReceivedAsync(messageBytes);
                        }

                        if (result.IsCompleted)
                        {
                            // There's no more data to be processed
                            break;
                        }
                    }
                    finally
                    {
                        _pipe.Reader.AdvanceTo(buffer.Start, buffer.End);
                    }
                }
            }
            finally
            {
                await _pipe.Reader.CompleteAsync();
            }
        }

        private bool TryGetMessage(ref ReadOnlySequence<byte> buffer, in int headerSize, out ReadOnlySequence<byte> message)
        {
            if (buffer.Length < headerSize)
            {
                message = default;
                return false;
            }

            var headerBytes = buffer.Slice(buffer.Start, buffer.GetPosition(headerSize));
            var reportedMessageSize = GetMessageSize(headerBytes);
            if (reportedMessageSize <= 0 || buffer.Length < reportedMessageSize)
            {
                message = default;
                return false;
            }

            var next = buffer.GetPosition(reportedMessageSize);

            message = buffer.Slice(buffer.Start, next);
            buffer = buffer.Slice(next);

            return true;
        }
    }
}
