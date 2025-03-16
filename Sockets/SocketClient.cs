using MiuLibrary.Constants;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MiuLibrary.Sockets
{
    public class SocketClient : IDisposable
    {
        public Socket client;
        public IPEndPoint IP { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }

        // Kích thước buffer mặc định
        private int bufferSize = 1024 * 5000;

        // Token để hủy các task đang chạy
        private CancellationTokenSource cancellationSource;

        // Sự kiện khi nhận được dữ liệu
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        // Sự kiện khi có lỗi xảy ra
        public event EventHandler<SocketErrorEventArgs> ErrorOccurred;

        // Trạng thái kết nối
        public bool IsConnected => client != null && client.Connected;

        // Constructor
        public SocketClient(string ipAddress, int port)
        {
            this.IpAddress = ipAddress;
            this.Port = port;
            this.cancellationSource = new CancellationTokenSource();
        }

        // Constructor bổ sung cho phép tùy chỉnh kích thước buffer
        public SocketClient(string ipAddress, int port, int bufferSize) : this(ipAddress, port)
        {
            this.bufferSize = bufferSize;
        }

        /// <summary>
        /// Kết nối tới server
        /// </summary>
        public void Connect()
        {
            if (IsConnected) return;

            try
            {
                IP = new IPEndPoint(IPAddress.Parse(IpAddress), Port);
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                client.Connect(IP);
            }
            catch (Exception er)
            {
                OnErrorOccurred(new SocketErrorEventArgs(er, "Connect"));
                throw new InvalidOperationException($"Error At: {er.Message}\n{er.StackTrace}");
            }
        }

        /// <summary>
        /// Kết nối bất đồng bộ tới server
        /// </summary>
        public async Task ConnectAsync()
        {
            if (IsConnected) return;

            try
            {
                IP = new IPEndPoint(IPAddress.Parse(IpAddress), Port);
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                await client.ConnectAsync(IP);
            }
            catch (Exception er)
            {
                OnErrorOccurred(new SocketErrorEventArgs(er, "ConnectAsync"));
                throw new InvalidOperationException($"Error At: {er.Message}\n{er.StackTrace}");
            }
        }

        /// <summary>
        /// Bắt đầu nhận dữ liệu trong luồng riêng biệt
        /// </summary>
        public void StartReceiving()
        {
            if (!IsConnected)
            {
                OnErrorOccurred(new SocketErrorEventArgs(new InvalidOperationException("Socket is not connected"), "StartReceiving"));
                return;
            }

            Task.Run(() => ReceiveLoop());
        }

        /// <summary>
        /// Bắt đầu nhận dữ liệu bất đồng bộ
        /// </summary>
        public Task StartReceivingAsync()
        {
            if (!IsConnected)
            {
                OnErrorOccurred(new SocketErrorEventArgs(new InvalidOperationException("Socket is not connected"), "StartReceivingAsync"));
                return Task.CompletedTask;
            }

            return Task.Run(() => ReceiveLoop());
        }

        /// <summary>
        /// Vòng lặp nhận dữ liệu
        /// </summary>
        private void ReceiveLoop()
        {
            try
            {
                while (!cancellationSource.Token.IsCancellationRequested && IsConnected)
                {
                    byte[] data = new byte[bufferSize];
                    int received = client.Receive(data);

                    if (received > 0)
                    {
                        byte[] validData = new byte[received];
                        Array.Copy(data, validData, received);

                        try
                        {
                            // Thay đổi: Kiểm tra MessageType không phải Content
                            var message = Deserialize<Models.Message>(validData);
                            if (message != null && !string.IsNullOrEmpty(message.MessageType))
                            {
                                OnMessageReceived(new MessageReceivedEventArgs(message, validData));
                            }
                        }
                        catch (Exception ex)
                        {
                            OnMessageReceived(new MessageReceivedEventArgs(null, validData));
                            OnErrorOccurred(new SocketErrorEventArgs(ex, "Deserialize"));
                        }
                    }
                }
            }
            catch (Exception er)
            {
                OnErrorOccurred(new SocketErrorEventArgs(er, "ReceiveLoop"));

                if (IsConnected)
                {
                    throw new InvalidOperationException($"Error At: {er.Message}\n{er.StackTrace}");
                }
            }
        }
        /// <summary>
        /// Nhận dữ liệu một lần và trả về kết quả
        /// </summary>
        public byte[] ReceiveOnce()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Socket is not connected");

            try
            {
                byte[] data = new byte[bufferSize];
                int received = client.Receive(data);

                if (received > 0)
                {
                    byte[] validData = new byte[received];
                    Array.Copy(data, validData, received);
                    return validData;
                }

                return null;
            }
            catch (Exception er)
            {
                OnErrorOccurred(new SocketErrorEventArgs(er, "ReceiveOnce"));
                throw new InvalidOperationException($"Error At: {er.Message}\n{er.StackTrace}");
            }
        }

        /// <summary>
        /// Nhận dữ liệu một lần và trả về kết quả bất đồng bộ
        /// </summary>
        public async Task<byte[]> ReceiveOnceAsync()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Socket is not connected");

            try
            {
                byte[] data = new byte[bufferSize];
                var received = await client.ReceiveAsync(data, SocketFlags.None);

                if (received > 0)
                {
                    byte[] validData = new byte[received];
                    Array.Copy(data, validData, received);
                    return validData;
                }

                return null;
            }
            catch (Exception er)
            {
                OnErrorOccurred(new SocketErrorEventArgs(er, "ReceiveOnceAsync"));
                throw new InvalidOperationException($"Error At: {er.Message}\n{er.StackTrace}");
            }
        }

        /// <summary>
        /// Dừng quá trình nhận dữ liệu
        /// </summary>
        public void StopReceiving()
        {
            cancellationSource.Cancel();
            // Tạo token mới để có thể bắt đầu lại sau này
            cancellationSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Đóng kết nối và giải phóng tài nguyên
        /// </summary>
        public void Close()
        {
            StopReceiving();

            if (client != null)
            {
                if (client.Connected)
                {
                    client.Shutdown(SocketShutdown.Both);
                }
                client.Close();
                client = null;
            }
        }

        /// <summary>
        /// Gửi dữ liệu cho Server
        /// </summary>
        /// <param name="obj">Giá trị dữ liệu gửi đi</param>
        public void Send(object obj)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Socket is not connected");

            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
            {
                try
                {
                    byte[] data = Serialize(obj);
                    client.Send(data);
                }
                catch (Exception ex)
                {
                    OnErrorOccurred(new SocketErrorEventArgs(ex, "Send"));
                    throw;
                }
            }
        }

        /// <summary>
        /// Gửi dữ liệu bất đồng bộ cho Server
        /// </summary>
        /// <param name="obj">Giá trị dữ liệu gửi đi</param>
        public async Task SendAsync(object obj)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Socket is not connected");

            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
            {
                try
                {
                    byte[] data = Serialize(obj);
                    await client.SendAsync(data, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    OnErrorOccurred(new SocketErrorEventArgs(ex, "SendAsync"));
                    throw;
                }
            }
        }

        /// <summary>
        /// Chuyển đổi kiểu dữ liệu Json về dạng một đối tượng T cụ thể
        /// </summary>
        private T DeserializeFromJsonElement<T>(object data)
        {
            if (data is JsonElement jsonElement)
            {
                return JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
            }
            throw new InvalidCastException("Cannot cast the object to the specified type.");
        }

        /// <summary>
        /// Chuyển đối tượng T về mảng byte
        /// </summary>
        public byte[] Serialize<T>(T obj)
        {
            return JsonSerializer.SerializeToUtf8Bytes(obj);
        }

        /// <summary>
        /// Chuyển mảng byte về đối tượng T
        /// </summary>
        public T Deserialize<T>(byte[] data)
        {
            return JsonSerializer.Deserialize<T>(data);
        }

        /// <summary>
        /// Phát sự kiện khi nhận được tin nhắn
        /// </summary>
        protected virtual void OnMessageReceived(MessageReceivedEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        /// <summary>
        /// Phát sự kiện khi có lỗi
        /// </summary>
        protected virtual void OnErrorOccurred(SocketErrorEventArgs e)
        {
            ErrorOccurred?.Invoke(this, e);
        }

        /// <summary>
        /// Giải phóng tài nguyên
        /// </summary>
        public void Dispose()
        {
            Close();
            cancellationSource.Dispose();
        }
    }

    /// <summary>
    /// Class chứa thông tin khi nhận được tin nhắn
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        public Models.Message Message { get; }
        public byte[] RawData { get; }

        public MessageReceivedEventArgs(Models.Message message, byte[] rawData)
        {
            Message = message;
            RawData = rawData;
        }
    }

    /// <summary>
    /// Class chứa thông tin khi có lỗi
    /// </summary>
    public class SocketErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }
        public string Operation { get; }

        public SocketErrorEventArgs(Exception exception, string operation)
        {
            Exception = exception;
            Operation = operation;
        }
    }
}