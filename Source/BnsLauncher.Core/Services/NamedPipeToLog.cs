using System;
using System.IO.Pipes;
using System.Text;
using BnsLauncher.Core.Abstractions;

namespace BnsLauncher.Core.Services
{
    public class NamedPipeToLog : IDisposable
    {
        private NamedPipeServerStream _pipeServer;
        private readonly ILogger _logger;
        private readonly byte[] _bytes = new byte[4096];
        private readonly string _pipeName;

        public NamedPipeToLog(ILogger logger, string pipeName)
        {
            _logger = logger;
            _pipeName = pipeName;
        }

        public string LogPrefix { get; set; } = "";

        public void StartLogging()
        {
            if (_pipeServer != null)
                throw new InvalidOperationException($"Do not use the same {nameof(NamedPipeToLog)} twice");

            _pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.In, 2, PipeTransmissionMode.Message,
                PipeOptions.Asynchronous);

            _pipeServer.BeginWaitForConnection(ConnectionCallback, null);
        }

        public void Close()
        {
            _logger.Log("Pipe server closing: " + _pipeName);
            _pipeServer?.Close();
        }

        public void Dispose()
        {
            Close();
        }

        private void ConnectionCallback(IAsyncResult ar)
        {
            try
            {
                _pipeServer.EndWaitForConnection(ar);
                _pipeServer.BeginRead(_bytes, 0, _bytes.Length, Callback, null);
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
            catch (Exception exception)
            {
                _logger.Log(exception);
            }
        }

        private void Callback(IAsyncResult ar)
        {
            try
            {
                var read = _pipeServer.EndRead(ar);

                if (read == 0)
                    return;

                _logger.Log(LogPrefix + Encoding.ASCII.GetString(_bytes, 0, read));

                _pipeServer.BeginRead(_bytes, 0, _bytes.Length, Callback, null);
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
            catch (Exception exception)
            {
                _logger.Log(exception);
            }
        }
    }
}