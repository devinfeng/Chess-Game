using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace DemoSimulate
{
    class ClientSocket
    {

        private TcpClient client = null;
        private NetworkStream outStream = null;
        private MemoryStream memStream;
        private BinaryReader reader;

        private const int MAX_READ = 8192;
        private byte[] byteBuffer = new byte[MAX_READ];
        public static bool loggedIn = false;

        /// <summary>
        /// 连接服务器
        /// </summary>
        public void ConnectServer(string host, int port)
        {
            client = new TcpClient();
            client.SendTimeout = 1000;
            client.ReceiveTimeout = 1000;
            client.NoDelay = true;
            try
            {
                client.BeginConnect(host, port, new AsyncCallback(OnConnect), null);
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// 连接上服务器
        /// </summary>
        void OnConnect(IAsyncResult asr)
        {
            outStream = client.GetStream();
            client.GetStream().BeginRead(byteBuffer, 0, MAX_READ, new AsyncCallback(OnRead), null);
        }

        /// <summary>
        /// 写数据
        /// </summary>
        public void SendMessage(int msgID, string msg)
        {
            byte[] idbyte = BitConverter.GetBytes(msgID);
            byte[] message = Encoding.UTF8.GetBytes(msg);
            //byte[] buff = new byte[idbyte.Length + message.Length];
            //Array.Copy(idbyte,buff,4);
            //Array.Copy(message,0, buff,4, message.Length);
            MemoryStream ms = null;
            using (ms = new MemoryStream())
            {
                ms.Position = 0;
                BinaryWriter writer = new BinaryWriter(ms);
                int msglen = (int)idbyte.Length + message.Length;
                writer.Write(msglen);
                writer.Write(idbyte);
                writer.Write(message);
                writer.Flush();
                if (client != null && client.Connected)
                {
                    //NetworkStream stream = client.GetStream(); 
                    byte[] payload = ms.ToArray();
                    outStream.BeginWrite(payload, 0, payload.Length, new AsyncCallback(OnWrite), null);
                }
                else
                {
                }
            }
        }

        /// <summary>
        /// 读取消息
        /// </summary>
        void OnRead(IAsyncResult asr)
        {
            int bytesRead = 0;
            try
            {
                lock (client.GetStream())
                {         //读取字节流到缓冲区
                    bytesRead = client.GetStream().EndRead(asr);
                }
                if (bytesRead < 1)
                {                //包尺寸有问题，断线处理
                    
                    return;
                }
                OnReceive(byteBuffer, bytesRead);   //分析数据包内容，抛给逻辑层
                lock (client.GetStream())
                {         //分析完，再次监听服务器发过来的新消息
                    Array.Clear(byteBuffer, 0, byteBuffer.Length);   //清空数组
                    client.GetStream().BeginRead(byteBuffer, 0, MAX_READ, new AsyncCallback(OnRead), null);
                }
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// 向链接写入数据流
        /// </summary>
        void OnWrite(IAsyncResult r)
        {
            try
            {
                outStream.EndWrite(r);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 接收到消息
        /// </summary>
        void OnReceive(byte[] bytes, int length)
        {
            memStream.Seek(0, SeekOrigin.End);
            memStream.Write(bytes, 0, length);
            //Reset to beginning
            memStream.Seek(0, SeekOrigin.Begin);
            while (RemainingBytes() > 2)
            {
                int messageLen = reader.ReadInt32();
                if (RemainingBytes() >= messageLen)
                {
                    MemoryStream ms = new MemoryStream();
                    BinaryWriter writer = new BinaryWriter(ms);
                    writer.Write(reader.ReadBytes(messageLen));
                    ms.Seek(0, SeekOrigin.Begin);
                }
                else
                {
                    //Back up the position two bytes
                    memStream.Position = memStream.Position - 2;
                    break;
                }
            }
            //Create a new stream with any leftover bytes
            byte[] leftover = reader.ReadBytes((int)RemainingBytes());
            memStream.SetLength(0);     //Clear
            memStream.Write(leftover, 0, leftover.Length);
        }

        private long RemainingBytes()
        {
            return memStream.Length - memStream.Position;
        }

        public void DisConnect()
        {
            client.Close();
        }
    }
}
