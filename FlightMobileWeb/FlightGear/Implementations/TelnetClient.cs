using System;
using System.Text;

namespace FlightSimulatorApp.Model {
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Threading.Tasks;

    class TelnetClient : ITelnetClient {
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
        public async Task ConnectAsync(string ip, int port) {
            await this.client.ConnectAsync(IPAddress.Parse(ip), port);
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
                throw new IOException(e.Message);
            }
        }

        /// <summary>
        /// Flushes the stream.
        /// </summary>
        public void Flush() {
            this.client.GetStream().Flush();
        }

        /// <inheritdoc />
        public async Task FlushAsync() {
            await this.client.GetStream().FlushAsync();
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
                    int bytesRead = await ns.ReadAsync(dataBytes, 0, Size).ConfigureAwait(false);
                    dataToReturn = Encoding.ASCII.GetString(dataBytes, 0, bytesRead);
                    return dataToReturn;
                }
                catch (Exception e) {
                    throw new IOException(e.Message);
                }
            }

            return dataToReturn;
        }
    }
}
