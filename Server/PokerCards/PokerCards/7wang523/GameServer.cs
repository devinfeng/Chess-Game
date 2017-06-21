using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using ProtoBuf;
using System.Threading.Tasks;

namespace PokerCards._7wang523
{
    class GameServer : ManagerBase<GameServer>
    {
        private Socket mSocket;
        private static int connections = 0;
        private List<ClientService> mTatolClients = new List<ClientService>();
        private Dictionary<int, KeyValuePair<Type, Action<object>>> mMessageEvents = new Dictionary<int, KeyValuePair<Type, Action<object>>>();


        public void Init()
        {
            IPAddress local = IPAddress.Parse("127.0.0.1");
            IPEndPoint iep = new IPEndPoint(local, 13000);
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,ProtocolType.Tcp);
            mSocket.Bind(iep); 
            mSocket.Listen(200);
            LogMgr.Log("服务器已启动等待客户端连接......");
            Task.Run(() => ServiceWork()).ContinueWith(OnException, TaskContinuationOptions.OnlyOnFaulted);
        }

        private void ServiceWork()
        {
            while (true)
            {
                ClientService c_server;
                try
                {
                    // 得到一条新的连接
                    //tcpClient = await _listener.AcceptTcpClientAsync();
                    Socket client = mSocket.Accept();
                    c_server = new ClientService(client);
                    mTatolClients.Add(c_server);
                    connections = mTatolClients.Count;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException ex)
                {
                    LogMgr.Error(string.Format("ServiceWork {0}:{1}:{2}", ex.GetType(), ex.ErrorCode, ex.Message));
                    if (ex.ErrorCode == 10054)
                    {
                        // 这种情况是由于链接的三次握手还没有完成，对方接断开连接导致的异常 errcode 10054
                        continue;
                    }
                    else
                    {
                        throw ex;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        private void OnException(Task t)
        {
            Exception e = t.Exception;
            LogMgr.Error("ServiceWork Task is Wrong! " + e.Message);
        }

        // 关闭某一个客户连接
        public void CloseSocket(ClientService client)
        {
            if (mTatolClients.Contains(client))
            {
                client.Close();
                LogMgr.Warning(string.Format("玩家{0}断开了链接！", client.mID));
                mTatolClients.Remove(client);
                connections = mTatolClients.Count;
                LogicMgr.Instance.RemoveUser(client);
            }
        }

        // 发送消息
        public void SendMsg<T>(ClientService client,int msgID,T msg)
        {
            if (client != null && mTatolClients.Contains(client))
            {
                MemoryStream ms = new MemoryStream();
                Serializer.Serialize<T>(ms, msg);
                byte[] buf = ms.GetBuffer();
                byte[] msgid = BitConverter.GetBytes(msgID);
                List<byte> buff = new List<byte>();
                buff.AddRange(msgid);
                buff.AddRange(buf);
                int len = buf.Length + msgid.Length;
                List<byte> temp = new List<byte>(BitConverter.GetBytes(len));
                temp.AddRange(buff);
                byte[] msgBuff = temp.ToArray();
                client.OnSend(msgBuff);
            }
        }

        // 注册消息
        public void RegisterMsg(int msgID, Type type, Action<object> events)
        {
            if (!mMessageEvents.ContainsKey(msgID))
                mMessageEvents.Add(msgID, new KeyValuePair<Type, Action<object>>(type, events));
        }

        public KeyValuePair<Type, Action<object>> GetMessageEvent(int msgID)
        {
            if (GameServer.Instance.mMessageEvents.ContainsKey(msgID))
                return GameServer.Instance.mMessageEvents[msgID];
            return new KeyValuePair<Type, Action<object>>();
        }
    }

    public class ClientService
    {
        private const int MAX_READ = 8192;
        private const int MaxMsgLength = 1024;
        byte[] buffer = new byte[MaxMsgLength];
        private Socket socket;
        public int mID { get; set; }

        public ClientService(Socket soc)
        {
            this.socket = soc;
            UserInfo user = new UserInfo(10, this);
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), socket);
        }

        private void OnReceive(IAsyncResult asy)
        {
            int len = 0;
            try
            {
                Socket ts = (Socket)asy.AsyncState;
                ts.EndReceive(asy);
                asy.AsyncWaitHandle.Close();
                if (asy.IsCompleted)
                {
                    len = BitConverter.ToInt32(buffer, 0);
                    byte[] buf = new byte[len];
                    Array.Copy(buffer, 4, buf, 0, len);
                    int msgId = BitConverter.ToInt32(buf, 0);
                    byte[] msg = new byte[buf.Length - 4];
                    Array.Copy(buf, 4, msg, 0, buf.Length - 4);
                    DealMessage(msgId, msg);

                    LogMgr.Log(string.Format("玩家{0}新消息{1}：{2}", mID, msgId, Encoding.UTF8.GetString(msg)));
                    buffer = new byte[MaxMsgLength];
                    socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), socket);
                }

            }
            catch (SocketException ex)
            {
                LogMgr.Error("OnReceive SocketException --> " + ex.Message);
            }
            catch (System.Exception ex)
            {
                LogMgr.Error("OnReceive Exception --> " + ex.Message);
                GameServer.Instance.CloseSocket(this);
            }
        }

        public void OnSend(byte[] buffer)
        {
            if (socket != null && socket.Connected)
            {
                socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(Send), socket);
            }
        }

        private void Send(IAsyncResult asy)
        {
            if (asy.AsyncState == socket && asy.IsCompleted)
            {
                try
                {
                    socket.EndSend(asy);
                    LogMgr.Log("消息发送成功！");
                }
                catch (System.Exception ex)
                {
                    LogMgr.Error("OnSend--->>>" + ex.Message);
                }
            }
        }

        // 处理消息
        public void DealMessage(int msgId, byte[] buf)
        {
            KeyValuePair<Type, Action<object>> curEvent = new KeyValuePair<Type, Action<object>>();
            curEvent = GameServer.Instance.GetMessageEvent(msgId);
            if (curEvent.Value != null)
            {
                Type type = curEvent.Key;
                try
                {
                    MemoryStream ms1 = new MemoryStream(buf);
                    Object msgObj = Serializer.NonGeneric.Deserialize(type, ms1);
                    curEvent.Value(msgObj);
                }
                catch (System.Exception ex)
                {
                    LogMgr.Error("DealMessage -->> " + ex.Message);
                }

            }
        }

        public void Close()
        {
            socket.Close();
        }
    }
}
