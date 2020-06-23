using System;
using System.Text;

namespace FlightSimulatorApp.Model {
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    class TelnetClient : ITelnetClient {
        private static Mutex mut = new Mutex();

        /// <summary>The size</summary>
        protected const short Size = 512;

        /// <summary>The client</summary>
        protected TcpClient client;

        /// <inheritdoc />
        public bool IsConnected => this.client.Connected;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public TelnetClient() {
            this.client = new TcpClient(AddressFamily.InterNetwork);
        }

        /// <summary>
        /// Connects the client.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <param name="port">The port.</param>
        public void ConnectAsync(string ip, int port) {
            if (mut.WaitOne(5000) && !this.IsConnected) {
                // if (this.IsConnected) {
                // return;
                // }
                try {
                    var asyncResult = this.client.BeginConnect(
                            IPAddress.Parse(ip),
                            port,
                            ar => {
                                if (ar.IsCompleted) {
                                    Console.WriteLine("connection made");
                                } else {
                                    Console.WriteLine("connection failed");
                                }
                            },
                            this);
                    while (!asyncResult.IsCompleted) {
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    throw;
                } finally {
                    mut.ReleaseMutex();
                }
            }
        }

        /// <inheritdoc />
        public void Connect(string ip, int port) {
            this.client.Connect(IPAddress.Parse(ip), port);
        }

        /// <summary>
        /// Disconnects the client.
        /// </summary>
        public void Disconnect() {
            if (this.IsConnected) {
                this.client.Close();
            }
            this.client = new TcpClient(AddressFamily.InterNetwork);
        }

        /// <summary>
        /// Sends the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <exception cref="IOException"></exception>
        public async Task SendAsync(string data) {
            try {
                if (this.IsConnected) {
                    NetworkStream networkStream = this.client.GetStream();
                    byte[] sendBytes = Encoding.ASCII.GetBytes(data);
                    await networkStream.WriteAsync(sendBytes, 0, sendBytes.Length);
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        /// <inheritdoc />
        public void Send(string data) {
            try {
                if (this.IsConnected) {
                    NetworkStream networkStream = this.client.GetStream();
                    byte[] sendBytes = Encoding.ASCII.GetBytes(data);
                    networkStream.Write(sendBytes, 0, sendBytes.Length);
                }
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Flushes the stream.
        /// </summary>
        public void Flush() {
            this.client.GetStream().Flush();
        }

        /// <inheritdoc />
        public Task FlushAsync() {
            return this.client.GetStream().FlushAsync();
        }

        /// <summary>
        /// Reads the data.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        public async Task<string> ReadAsync() {
            string dataToReturn = string.Empty;
            if (this.IsConnected) {
                NetworkStream ns = this.client.GetStream();
                try {
                    byte[] dataBytes = new byte[Size];
                    int bytesRead = await ns.ReadAsync(dataBytes, 0, Size);
                    dataToReturn = Encoding.ASCII.GetString(dataBytes, 0, bytesRead);
                    return dataToReturn;
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                }
            }
            return dataToReturn;
        }

        /// <inheritdoc />
        public string Read() {
            string dataToReturn = string.Empty;
            if (this.IsConnected) {
                NetworkStream ns = this.client.GetStream();
                try {
                    byte[] dataBytes = new byte[Size];
                    int bytesRead = ns.Read(dataBytes, 0, Size);
                    dataToReturn = Encoding.ASCII.GetString(dataBytes, 0, bytesRead);
                    return dataToReturn;
                } catch (Exception e) {
                    Console.WriteLine(e);
                }
            }
            return dataToReturn;
        }
    }
}
