using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Runtime.InteropServices;

namespace DanServer
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32")]//返回0表示失败，非0为成功
        private static extern long WritePrivateProfileString(string section, string key,
            string val, string filePath);

        [DllImport("kernel32")]//返回取得字符串缓冲区的长度
        private static extern long GetPrivateProfileString(string section, string key,
            string def, StringBuilder retVal, int size, string filePath);

        Dictionary<string, List<String>> myDictionary11 = new Dictionary<string, List<String>>();
        Dictionary<string, List<String>> myDictionary12 = new Dictionary<string, List<String>>();
        Dictionary<string, List<String>> myDictionary13 = new Dictionary<string, List<String>>();
        Dictionary<string, List<String>> myDictionary14 = new Dictionary<string, List<String>>();
        Dictionary<string, List<String>> myDictionary15 = new Dictionary<string, List<String>>();
        
        Thread threadwatch = null;//负责监听客户端的线程
        Socket socketwatch = null;//负责监听客户端的套接字
        //创建一个和客户端通信的套接字
        //Dictionary<string, Socket> dic = new Dictionary<string, Socket> { };   //定义一个集合，存储客户端信息
        
        string RemoteEndPoint2;         //客户端的网络结点
        Thread threadwatch2 = null;     //负责监听客户端的线程
        Socket socketwatch2 = null;     //负责监听客户端的套接字
        //创建一个和客户端通信的套接字
        Dictionary<string, Socket> dic2 = new Dictionary<string, Socket> { };   //定义一个集合，存储客户端信息

        private delegate void FlushClient(String text); //代理
        private delegate void FlushClient11(string msg, bool bFind); //代理
        private delegate void FlushClient12(string msg, bool bFind); //代理
        private delegate void FlushClient13(string msg, bool bFind); //代理
        private delegate void FlushClient14(string msg, bool bFind); //代理
        private delegate void FlushClient15(string msg, bool bFind); //代理
        private SqlConnection myConnection;

        private int iClear = 0;
        
        //private String str_name1;

        bool m_exit = false;
        bool m_exit2 = true;

        private string str_contains11 = "M8198", str_select11 = "";
        private int i_select11 = 1;

        private string str_contains12 = "M8198", str_select12 = "";
        private int i_select12 = 1;

        private string str_contains13 = "M8198", str_select13 = "";
        private int i_select13 = 1;

        private string str_contains14 = "M8198", str_select14 = "";
        private int i_select14 = 1;

        private string str_contains15 = "M8198", str_select15 = "";
        private int i_select15 = 1;

        private string dbname = "";

        public Form1()
        {
            InitializeComponent();

            string path3 = System.IO.Directory.GetCurrentDirectory();
            path3 += "\\conf.ini";

            StringBuilder temp = new StringBuilder(1024);

            GetPrivateProfileString("CON", "CON1", "", temp, 1024, path3);
            str_contains11 = temp.ToString();
            temp.Clear();

            GetPrivateProfileString("CON", "CON2", "", temp, 1024, path3);
            str_contains12 = temp.ToString();
            temp.Clear();

            GetPrivateProfileString("CON", "CON3", "", temp, 1024, path3);
            str_contains13 = temp.ToString();
            temp.Clear();

            GetPrivateProfileString("CON", "CON4", "", temp, 1024, path3);
            str_contains14 = temp.ToString();
            temp.Clear();

            GetPrivateProfileString("CON", "CON5", "", temp, 1024, path3);
            str_contains15 = temp.ToString();
            temp.Clear();

            GetPrivateProfileString("CON", "DB", "", temp, 1024, path3);
            dbname  = temp.ToString();
            temp.Clear();

            string str_conn = "server="+ dbname + ";database=master;uid=sa;pwd=123456";
            myConnection = new SqlConnection(str_conn);
            try
            {
                myConnection.Open();
                //myConnection.Close();
                System.Console.WriteLine("Well done!");
            }
            catch (SqlException ex)
            {
                System.Console.WriteLine("You failed!" + ex.Message);
            }

        }

        private static int flag = 0;
        //开启服务
        private void button1_Click(object sender, EventArgs e)
        {
            //query();
            if (flag == 0)
            {
                button1.Text = "关闭服务";
                m_exit = false;
                flag = 1;

                init();
            }
            else
            {
                button1.Text = "开启服务";
                m_exit = true;
                flag = 0;

                socketwatch.Close();
                //dic.Clear();
                listBox1.Items.Clear();
            }
        }

        public void init()
        {
            try
            {
                //定义一个套接字用于监听客户端发来的消息，包含三个参数（IP4寻址协议，流式连接，Tcp协议）
                socketwatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //服务端发送信息需要一个IP地址和端口号
                IPAddress address = IPAddress.Parse(textBox1.Text.Trim());//获取文本框输入的IP地址

                //将IP地址和端口号绑定到网络节点point上
                IPEndPoint point = new IPEndPoint(address, int.Parse(textBox2.Text.Trim()));//获取文本框上输入的端口号
                                                                                            //此端口专门用来监听的

                //监听绑定的网络节点
                socketwatch.Bind(point);

                //将套接字的监听队列长度限制为20
                socketwatch.Listen(20);

                //创建一个监听线程
                threadwatch = new Thread(watchconnecting);

                //将窗体线程设置为与后台同步，随着主线程结束而结束
                threadwatch.IsBackground = true;

                //启动线程   
                threadwatch.Start();
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //监听客户端发来的请求
        private void watchconnecting()
        {
            Socket connection = null;
            while (true)  //持续不断监听客户端发来的请求   
            {
                if (m_exit == true) break;

                try
                {
                    connection = socketwatch.Accept();
                }
                catch (Exception ex)
                {
                    //textBox3.AppendText(ex.Message); //提示套接字监听异常   
                    break;
                }

                if (m_exit == true) break;

                //获取客户端的IP和端口号
                IPAddress clientIP = (connection.RemoteEndPoint as IPEndPoint).Address;
                int clientPort = (connection.RemoteEndPoint as IPEndPoint).Port;

                //让客户显示"连接成功的"的信息
                /*
                string sendmsg = "连接服务端成功！\r\n" + "本地IP:" + clientIP + "，本地端口" + clientPort.ToString();
                byte[] arrSendMsg = Encoding.UTF8.GetBytes(sendmsg);
                connection.Send(arrSendMsg);
                */

                //RemoteEndPoint = connection.RemoteEndPoint.ToString(); //客户端网络结点号
                
                //ThreadFunction(RemoteEndPoint);
                //listBox1.Items.Add(RemoteEndPoint);

                //textBox3.AppendText("成功与" + RemoteEndPoint + "客户端建立连接！\t\n");     //显示与客户端连接情况
                //dic.Add(RemoteEndPoint, connection);    //添加客户端信息

                //OnlineList_Disp(RemoteEndPoint);    //显示在线客户端

                //IPEndPoint netpoint = new IPEndPoint(clientIP,clientPort);

                //IPEndPoint netpoint = connection.RemoteEndPoint as IPEndPoint;

                //创建一个通信线程    
                ParameterizedThreadStart pts = new ParameterizedThreadStart(recv);
                Thread thread = new Thread(pts);
                thread.IsBackground = true;//设置为后台线程，随着主线程退出而退出   
                //启动线程
                thread.Start(connection);
            }
        }

        public void init2()
        {
            //定义一个套接字用于监听客户端发来的消息，包含三个参数（IP4寻址协议，流式连接，Tcp协议）
            socketwatch2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //服务端发送信息需要一个IP地址和端口号
            IPAddress address = IPAddress.Parse(textBox1.Text.Trim());//获取文本框输入的IP地址

            //将IP地址和端口号绑定到网络节点point上
            IPEndPoint point = new IPEndPoint(address, 1 + int.Parse(textBox2.Text.Trim()));//获取文本框上输入的端口号
            //此端口专门用来监听的

            //监听绑定的网络节点
            socketwatch2.Bind(point);

            //将套接字的监听队列长度限制为20
            socketwatch2.Listen(20);

            //创建一个监听线程
            threadwatch2 = new Thread(watchconnecting2);

            //将窗体线程设置为与后台同步，随着主线程结束而结束
            threadwatch2.IsBackground = true;

            //启动线程   
            threadwatch2.Start();
        }

        //监听客户端发来的请求
        private void watchconnecting2()
        {
            Socket connection = null;
            while (true)  //持续不断监听客户端发来的请求   
            {
                if (m_exit2 == true) break;

                try
                {
                    connection = socketwatch2.Accept();
                }
                catch (Exception ex)
                {
                    //textBox3.AppendText(ex.Message); //提示套接字监听异常   
                    break;
                }

                if (m_exit2 == true) break;

                //获取客户端的IP和端口号
                IPAddress clientIP = (connection.RemoteEndPoint as IPEndPoint).Address;
                int clientPort = (connection.RemoteEndPoint as IPEndPoint).Port;

                //让客户显示"连接成功的"的信息
                /*
                string sendmsg = "连接服务端成功！\r\n" + "本地IP:" + clientIP + "，本地端口" + clientPort.ToString();
                byte[] arrSendMsg = Encoding.UTF8.GetBytes(sendmsg);
                connection.Send(arrSendMsg);
                */

                RemoteEndPoint2 = connection.RemoteEndPoint.ToString(); //客户端网络结点号
                
                dic2.Add(RemoteEndPoint2, connection);    //添加客户端信息

                //OnlineList_Disp(RemoteEndPoint);    //显示在线客户端

                //IPEndPoint netpoint = new IPEndPoint(clientIP,clientPort);

                IPEndPoint netpoint = connection.RemoteEndPoint as IPEndPoint;

                //创建一个通信线程    
                ParameterizedThreadStart pts = new ParameterizedThreadStart(recv2);
                Thread thread = new Thread(pts);
                thread.IsBackground = true;//设置为后台线程，随着主线程退出而退出   
                //启动线程
                thread.Start(connection);
            }
        }

        private void recv2(object socketclientpara)
        {
            //创建一个内存缓冲区 其大小为1024*1024字节  即1M   
            byte[] arrServerRecMsg = new byte[1024 * 1024];

            Socket socketServer = socketclientpara as Socket;
            String local_name = socketServer.RemoteEndPoint.ToString();
            List<String> list = new List<String>();
            while (true)
            {
                //将接收到的信息存入到内存缓冲区,并返回其字节数组的长度  
                try
                {
                    if (m_exit2 == true) break;

                    int length = socketServer.Receive(arrServerRecMsg);
                    if (length <= 0)
                    {
                        break;
                    }
                                        
                    if (m_exit2 == true) break;

                    //将机器接受到的字节数组转换为人可以读懂的字符串   
                    String strSRecMsg = Encoding.UTF8.GetString(arrServerRecMsg, 0, length).Trim();

                    dealData(socketServer, strSRecMsg);
                    //String[] strs = strSRecMsg.Split("#".ToArray());

                }
                catch (Exception ex)
                {
                    //textBox3.AppendText("客户端" + socketServer.RemoteEndPoint + "已经中断连接" + "\r\n"); //提示套接字监听异常 
                    Console.WriteLine(ex.ToString());
                    socketServer.Close();//关闭之前accept出来的和客户端进行通信的套接字
                    break;
                }
            }

        }

        private void dealData(Socket socketServer, String data)
        {
            Console.WriteLine(data);
            String[] strs = data.Split("#".ToArray());
            if (strs[0].Equals("login"))
            {
                if ((strs[1].Equals("admin") && strs[2].Equals("admin"))
                    || (strs[1].Equals("user") && strs[2].Equals("user"))
                    || (strs[1].Equals("user123") && strs[2].Equals("user123"))
                    || (strs[1].Equals("user1233") && strs[2].Equals("user1233"))
                    )
                {
                    socketServer.Send(System.Text.Encoding.Default.GetBytes("login#ok"));
                }
            }
            else if (strs[0].Equals("user_list"))
            {
                String str = "user_list";
                //Console.WriteLine(dic.Count);
                //foreach (KeyValuePair<String, Socket> keyValuePair in dic)
                foreach (string key in myDictionary11.Keys)
                {
                    //Console.WriteLine("key:{0}\tvalue:{1}", keyValuePair.Key, keyValuePair.Value);
                    str += "#";
                    str += key;
                }
                socketServer.Send(System.Text.Encoding.Default.GetBytes(str));
            }
            else if (strs[0].Equals("user_info"))
            {
                String name = strs[1];
                String sql = "select top 15 id, userinfo, ch1, ch2, ch3, ch4, ch5, ch6, ch7, ch8 from msg where name = '"
                    + name + "' order by id desc";

                SqlCommand comm = new SqlCommand(sql, myConnection);
                //把读取到的数据放到reder中，reder是SqlDataReader。
                SqlDataReader reder = comm.ExecuteReader();
                //前进到下一条记录中，如果下一条记录中没有了就返回false，如果有就一直读完。
                String total = "user_info#";
                while (reder.Read())
                {
                    //读出内容列 ，赋值给str
                    //reder["cname"]是]从reader对象中读取这一行列名为cname的数据，也可以是下标。
                    //Console.WriteLine(reder["id"].ToString());
                    total += reder["userinfo"].ToString(); total += "=";
                    total += reder["ch1"].ToString(); total += "=";
                    total += reder["ch2"].ToString(); total += "=";
                    total += reder["ch3"].ToString(); total += "=";
                    total += reder["ch4"].ToString(); total += "=";
                    total += reder["ch5"].ToString(); total += "=";
                    total += reder["ch6"].ToString(); total += "=";
                    total += reder["ch7"].ToString(); total += "=";
                    total += reder["ch8"].ToString(); total += "#";
                }
                reder.Close();

                socketServer.Send(System.Text.Encoding.Default.GetBytes(total));
            }

            else if (strs[0].Equals("download_total"))
            {
                String sql = "select count(*) n from msg where datediff(d, CONVERT(varchar(100), last_date, 23), CONVERT(varchar(100), GETDATE(), 23)) = 0";
                Console.WriteLine(sql);
                SqlCommand comm = new SqlCommand(sql, myConnection);
                //把读取到的数据放到reder中，reder是SqlDataReader。
                SqlDataReader reder = comm.ExecuteReader();
                //前进到下一条记录中，如果下一条记录中没有了就返回false，如果有就一直读完。

                String n = "0";
                if (reder.Read())
                {
                    n = reder["n"].ToString();
                }

                /*if (iFind == 0)
                {
                    total += "ok";
                }*/

                reder.Close();
                socketServer.Send(System.Text.Encoding.Default.GetBytes(strs[0] + "#" + n));
            }
            else if (strs[0].Equals("download"))
            {
                int start = Int32.Parse(strs[1]);
                int num = Int32.Parse(strs[2]);
                String sql = "select top "+ num + " * from (select top " + start + " * from msg where datediff(d, CONVERT(varchar(100), last_date, 23), CONVERT(varchar(100), GETDATE(), 23)) = 0 order by id asc) a order by id desc";
                Console.WriteLine(sql);
                SqlCommand comm = new SqlCommand(sql, myConnection);
                //把读取到的数据放到reder中，reder是SqlDataReader。
                SqlDataReader reder = comm.ExecuteReader();
                //前进到下一条记录中，如果下一条记录中没有了就返回false，如果有就一直读完。
                String total = "download#" + start + "#";
                
                while (reder.Read())
                {
                    String id = reder["id"].ToString();
                    
                    total += reder["userinfo"].ToString(); total += "=";
                    total += reder["ch1"].ToString(); total += "=";
                    total += reder["ch2"].ToString(); total += "=";
                    total += reder["ch3"].ToString(); total += "=";
                    total += reder["ch4"].ToString(); total += "=";
                    total += reder["ch5"].ToString(); total += "=";
                    total += reder["ch6"].ToString(); total += "=";
                    total += reder["ch7"].ToString(); total += "=";
                    total += reder["ch8"].ToString(); total += "#";
                    
                }
                
                reder.Close();
                socketServer.Send(System.Text.Encoding.Default.GetBytes(total));
            }
        }
        
        private void DisplayName1(String text)
        {
            if (this.listBox1.InvokeRequired)//等待异步
            {
                FlushClient fc = new FlushClient(DisplayName1);
                this.Invoke(fc, new object[] { text }); //通过代理调用刷新方法
            }
            else
            {
                listBox1.Items.Add(text);
            }
        }
        
        //接受客户端发来的信息
        private void recv(object socketclientpara)
        {
            //创建一个内存缓冲区 其大小为1024*1024字节  即1M   
            byte[] arrServerRecMsg = new byte[1024 * 1024];
            
            Socket socketServer = socketclientpara as Socket;
            String local_name = socketServer.RemoteEndPoint.ToString();
            while (true)
            {
                //将接收到的信息存入到内存缓冲区,并返回其字节数组的长度  
                try
                {
                    if (m_exit == true) break;
                    
                    int length = socketServer.Receive(arrServerRecMsg);
                    if (length <= 0)
                    {
                        break;
                    }

                    if (m_exit == true) break;

                    //将机器接受到的字节数组转换为人可以读懂的字符串   
                    String strSRecMsg = Encoding.UTF8.GetString(arrServerRecMsg, 0, length).Trim();
                    insert(strSRecMsg);

                    strSRecMsg += "\n\n";
                    strSRecMsg = strSRecMsg.Trim();
                    MsgType(strSRecMsg);
                    
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    //textBox3.AppendText("客户端" + socketServer.RemoteEndPoint + "已经中断连接" + "\r\n"); //提示套接字监听异常 
                    socketServer.Close();//关闭之前accept出来的和客户端进行通信的套接字
                    break;
                }
            }

        }

        private void MsgType(string strSRecMsg)
        {
            String[] strMsg = strSRecMsg.Split("\n".ToArray());
            if (strMsg.Length > 0)
            {
                String key = strMsg[0];
                if (key.Contains(str_contains11) == true)//
                {
                    List<String> list_local;
                    bool bFind = myDictionary11.TryGetValue(key, out list_local);
                    while (bFind == true && list_local.Count > 15)//每个队列最大保持15个消息显示
                    {
                        list_local.RemoveAt(0);
                    }
                    if (list_local == null)
                    {
                        list_local = new List<String>();
                        myDictionary11[key] = list_local;
                    }
                    list_local.Add(strSRecMsg);

                    ThreadFunctionText11(strMsg[0], bFind); 
                }
                else if (key.Contains(str_contains12) == true)//
                {
                    List<String> list_local;
                    bool bFind = myDictionary12.TryGetValue(key, out list_local);
                    while (bFind == true && list_local.Count > 15)//每个队列最大保持15个消息显示
                    {
                        list_local.RemoveAt(0);
                    }
                    if (list_local == null)
                    {
                        list_local = new List<String>();
                        myDictionary12[key] = list_local;
                    }
                    list_local.Add(strSRecMsg);

                    ThreadFunctionText12(strMsg[0], bFind);
                }
                else if (key.Contains(str_contains13) == true)//
                {
                    List<String> list_local;
                    bool bFind = myDictionary13.TryGetValue(key, out list_local);
                    while (bFind == true && list_local.Count > 15)//每个队列最大保持15个消息显示
                    {
                        list_local.RemoveAt(0);
                    }
                    if (list_local == null)
                    {
                        list_local = new List<String>();
                        myDictionary13[key] = list_local;
                    }
                    list_local.Add(strSRecMsg);

                    ThreadFunctionText13(strMsg[0], bFind);
                }
                else if (key.Contains(str_contains14) == true)//
                {
                    List<String> list_local;
                    bool bFind = myDictionary14.TryGetValue(key, out list_local);
                    while (bFind == true && list_local.Count > 15)//每个队列最大保持15个消息显示
                    {
                        list_local.RemoveAt(0);
                    }
                    if (list_local == null)
                    {
                        list_local = new List<String>();
                        myDictionary14[key] = list_local;
                    }
                    list_local.Add(strSRecMsg);

                    ThreadFunctionText14(strMsg[0], bFind);
                }
                else if (key.Contains(str_contains15) == true)//
                {
                    List<String> list_local;
                    bool bFind = myDictionary15.TryGetValue(key, out list_local);
                    while (bFind == true && list_local.Count > 15)//每个队列最大保持15个消息显示
                    {
                        list_local.RemoveAt(0);
                    }
                    if (list_local == null)
                    {
                        list_local = new List<String>();
                        myDictionary15[key] = list_local;
                    }
                    list_local.Add(strSRecMsg);

                    ThreadFunctionText15(strMsg[0], bFind);
                }
            }
        }

        private void ThreadFunctionText11(string msg, bool bFind)
        {
            if (this.listBox1.InvokeRequired)//等待异步
            {
                FlushClient11 fc = new FlushClient11(ThreadFunctionText11);
                this.Invoke(fc, new object[] { msg, bFind }); //通过代理调用刷新方法
            }
            else
            {
                if (bFind == false)//如果没找到说明是新来的，要在列表中加入
                {
                    listBox1.Items.Add(msg);
                }

                if (i_select11 == 1)
                {
                    str_select11 = msg;
                    i_select11++;
                }

                if (msg.CompareTo(str_select11) != 0)
                {
                    return;
                }

                List<String> list;
                myDictionary11.TryGetValue(str_select11, out list);

                richTextBox1.Clear();
                richTextBox2.Clear();
                richTextBox3.Clear();
                richTextBox4.Clear();
                richTextBox5.Clear();
                richTextBox6.Clear();
                richTextBox7.Clear();
                richTextBox8.Clear();

                double zong = 0.0;
                for (int i = 0; i < list.Count; i++)
                {
                    //解析第一行为用户信息
                    int index = 1;

                    String text = list.ElementAt(i);
                    text = text.Trim();
                    String[] strMsg = text.Split("\n".ToArray());
                    if (strMsg.Length == 8)
                    {
                        index = 0;
                        textBox3.Text = "";
                    }
                    else
                    {
                        textBox3.Text = strMsg[0];
                    }

                    String[] strMsg2 = strMsg[index++].Split(":".ToArray())[1].Split(",".ToArray());
                    double pinglv1 = 0;
                    if (strMsg2.Length == 2)
                    {
                        Double.TryParse(strMsg2[0], out pinglv1);
                        richTextBox1.AppendText("频率：" + strMsg2[0] + "Hz\t\t温度：" + strMsg2[1].Trim() + "℃\n");
                    }
                    else
                    {
                        richTextBox1.AppendText("频率：无\t\t温度：无\n");
                    }

                    strMsg2 = strMsg[index++].Split(":".ToArray())[1].Split(",".ToArray());
                    double pinglv2 = 0;
                    if (strMsg2.Length == 2)
                    {
                        Double.TryParse(strMsg2[0], out pinglv2);
                        richTextBox2.AppendText("频率：" + strMsg2[0] + "Hz\t\t温度：" + strMsg2[1].Trim() + "℃\n");
                    }
                    else
                    {
                        richTextBox2.AppendText("频率：无\t\t温度：无\n");
                    }

                    strMsg2 = strMsg[index++].Split(":".ToArray())[1].Split(",".ToArray());
                    double pinglv3 = 0;
                    if (strMsg2.Length == 2)
                    {
                        Double.TryParse(strMsg2[0], out pinglv3);
                        richTextBox3.AppendText("频率：" + strMsg2[0] + "Hz\t\t温度：" + strMsg2[1].Trim() + "℃\n");
                    }
                    else
                    {
                        richTextBox3.AppendText("频率：无\t\t温度：无\n");
                    }

                    strMsg2 = strMsg[index++].Split(":".ToArray())[1].Split(",".ToArray());
                    double pinglv4 = 0;
                    if (strMsg2.Length == 2)
                    {
                        Double.TryParse(strMsg2[0], out pinglv4);
                        richTextBox4.AppendText("频率：" + strMsg2[0] + "Hz\t\t温度：" + strMsg2[1].Trim() + "℃\n");
                    }
                    else
                    {
                        richTextBox4.AppendText("频率：无\t\t温度：无\n");
                    }

                    strMsg2 = strMsg[index++].Split(":".ToArray())[1].Split(",".ToArray());
                    double pinglv5 = 0;
                    if (strMsg2.Length == 2)
                    {
                        Double.TryParse(strMsg2[0], out pinglv5);
                        richTextBox5.AppendText("频率：" + strMsg2[0] + "Hz\t\t温度：" + strMsg2[1].Trim() + "℃\n");
                    }
                    else
                    {
                        richTextBox5.AppendText("频率：无\t\t温度：无\n");
                    }

                    strMsg2 = strMsg[index++].Split(":".ToArray())[1].Split(",".ToArray());
                    double pinglv6 = 0;
                    if (strMsg2.Length == 2)
                    {
                        Double.TryParse(strMsg2[0], out pinglv6);
                        richTextBox6.AppendText("频率：" + strMsg2[0] + "Hz\t\t温度：" + strMsg2[1].Trim() + "℃\n");
                    }
                    else
                    {
                        richTextBox6.AppendText("频率：无\t\t温度：无\n");
                    }

                    strMsg2 = strMsg[index++].Split(":".ToArray())[1].Split(",".ToArray());
                    double pinglv7 = 0;
                    if (strMsg2.Length == 2)
                    {
                        Double.TryParse(strMsg2[0], out pinglv7);
                        richTextBox7.AppendText("频率：" + strMsg2[0] + "Hz\t\t温度：" + strMsg2[1].Trim() + "℃\n");
                    }
                    else
                    {
                        richTextBox7.AppendText("频率：无\t\t温度：无\n");
                    }

                    strMsg2 = strMsg[index++].Split(":".ToArray())[1].Split(",".ToArray());
                    double pinglv8 = 0;
                    if (strMsg2.Length == 2)
                    {
                        Double.TryParse(strMsg2[0], out pinglv8);
                        richTextBox8.AppendText("频率：" + strMsg2[0] + "Hz\t\t温度：" + strMsg2[1].Trim() + "℃\n");
                    }
                    else
                    {
                        richTextBox8.AppendText("频率：无\t\t温度：无\n");
                    }

                    //添加位置信息
                    label13.Text = strMsg[index++].Split(":".ToArray())[1];

                    zong = (pinglv8 + pinglv8 + pinglv8 + pinglv8 + pinglv8 + pinglv8 + pinglv8 + pinglv8) / 8;
                    zong = -0.748057 * (zong - 6582);
                }

                label15.Text = zong.ToString();

                scrollBottom(richTextBox1);
                scrollBottom(richTextBox2);
                scrollBottom(richTextBox3);
                scrollBottom(richTextBox4);
                scrollBottom(richTextBox5);
                scrollBottom(richTextBox6);
                scrollBottom(richTextBox7);
                scrollBottom(richTextBox8);
            }
        }

        private void ThreadFunctionText12(string msg, bool bFind)
        {
            if (this.listBox2.InvokeRequired)//等待异步
            {
                FlushClient12 fc = new FlushClient12(ThreadFunctionText12);
                this.Invoke(fc, new object[] { msg, bFind }); //通过代理调用刷新方法
            }
            else
            {
                if (bFind == false)//如果没找到说明是新来的，要在列表中加入
                {
                    listBox2.Items.Add(msg);
                }

                if (i_select12 == 1)
                {
                    str_select12 = msg;
                    i_select12++;
                }

                if (msg.CompareTo(str_select12) != 0)
                {
                    return;
                }

                List<String> list;
                myDictionary12.TryGetValue(str_select12, out list);

                richTextBox9.Clear();
                richTextBox10.Clear();
                
                for (int i = 0; i < list.Count; i++)
                {
                    //解析第一行为用户信息
                    int index = 1;

                    String text = list.ElementAt(i);
                    text = text.Trim();
                    String[] strMsg = text.Split("\n".ToArray());

                    String strMsg2 = strMsg[index++].Split(":".ToArray())[1];
                    richTextBox9.AppendText("位移：" + strMsg2 + "mm\n");

                    //添加位置信息
                    label20.Text = strMsg[index++].Split(":".ToArray())[1];
                    
                }
                
                scrollBottom(richTextBox9);
                scrollBottom(richTextBox10);
            }
        }

        private void ThreadFunctionText13(string msg, bool bFind)
        {
            if (this.listBox2.InvokeRequired)//等待异步
            {
                FlushClient13 fc = new FlushClient13(ThreadFunctionText13);
                this.Invoke(fc, new object[] { msg, bFind }); //通过代理调用刷新方法
            }
            else
            {
                if (bFind == false)//如果没找到说明是新来的，要在列表中加入
                {
                    listBox3.Items.Add(msg);
                }

                if (i_select13 == 1)
                {
                    str_select13 = msg;
                    i_select13++;
                }

                if (msg.CompareTo(str_select13) != 0)
                {
                    return;
                }

                List<String> list;
                myDictionary13.TryGetValue(str_select13, out list);

                richTextBox12.Clear();

                for (int i = 0; i < list.Count; i++)
                {
                    //解析第一行为用户信息
                    int index = 1;

                    String text = list.ElementAt(i);
                    text = text.Trim();
                    String[] strMsg = text.Split("\n".ToArray());

                    String strMsg2 = strMsg[index++].Split(":".ToArray())[1];
                    richTextBox12.AppendText("风速：" + strMsg2 + "m/s\n");

                    //添加位置信息
                    label21.Text = strMsg[index++].Split(":".ToArray())[1];

                }

                scrollBottom(richTextBox12);
            }
        }

        private void ThreadFunctionText14(string msg, bool bFind)
        {
            if (this.listBox4.InvokeRequired)//等待异步
            {
                FlushClient14 fc = new FlushClient14(ThreadFunctionText14);
                this.Invoke(fc, new object[] { msg, bFind }); //通过代理调用刷新方法
            }
            else
            {
                if (bFind == false)//如果没找到说明是新来的，要在列表中加入
                {
                    listBox4.Items.Add(msg);
                }

                if (i_select14 == 1)
                {
                    str_select14 = msg;
                    i_select14++;
                }

                if (msg.CompareTo(str_select14) != 0)
                {
                    return;
                }

                List<String> list;
                myDictionary14.TryGetValue(str_select14, out list);

                richTextBox13.Clear();
                richTextBox11.Clear();

                for (int i = 0; i < list.Count; i++)
                {
                    //解析第一行为用户信息
                    int index = 1;

                    String text = list.ElementAt(i);
                    text = text.Trim();
                    String[] strMsg = text.Split("\n".ToArray());

                    String strMsg2 = strMsg[index++].Split(":".ToArray())[1];
                    richTextBox13.AppendText("电压：" + strMsg2 + "mV\n");

                    double a = 0.0;
                    Double.TryParse(strMsg2, out a);
                    a = a / 90 * 10;
                    string str_a = string.Format("%.2f", a);
                    richTextBox11.AppendText("振动加速度：" + str_a + "m/s平方\n");

                    //  a=200mV/(90(mV/g))=2.2g=22m/s2

                    //添加位置信息
                    label22.Text = strMsg[index++].Split(":".ToArray())[1];

                }

                scrollBottom(richTextBox13);
                scrollBottom(richTextBox11);
            }
        }

        private void ThreadFunctionText15(string msg, bool bFind)
        {
            if (this.listBox5.InvokeRequired)//等待异步
            {
                FlushClient15 fc = new FlushClient15(ThreadFunctionText15);
                this.Invoke(fc, new object[] { msg, bFind }); //通过代理调用刷新方法
            }
            else
            {
                if (bFind == false)//如果没找到说明是新来的，要在列表中加入
                {
                    listBox5.Items.Add(msg);
                }

                if (i_select15 == 1)
                {
                    str_select15 = msg;
                    i_select15++;
                }

                if (msg.CompareTo(str_select15) != 0)
                {
                    return;
                }

                List<String> list;
                myDictionary15.TryGetValue(str_select15, out list);

                richTextBox15.Clear();
                richTextBox14.Clear();

                for (int i = 0; i < list.Count; i++)
                {
                    //解析第一行为用户信息
                    int index = 1;

                    String text = list.ElementAt(i);
                    text = text.Trim();
                    String[] strMsg = text.Split("\n".ToArray());
                    
                    String[] strMsg2 = strMsg[index++].Split(":".ToArray())[1].Split(",".ToArray());
                    richTextBox15.AppendText("频率：" + strMsg2[0] + "温度：" + strMsg2[1] + "\n");

                    double T = 0.0;
                    Double.TryParse(strMsg2[1], out T);
                    double e = 0.3568 * (2200 * 2200 / 1000 - 2000 * 2000 / 1000) + (12.2 - 10.4) *(T - 20);
                    string e_str = string.Format("%.2f", e);

                    richTextBox15.AppendText("应变：" + e_str + "\n");

                    //添加位置信息
                    label30.Text = strMsg[index++].Split(":".ToArray())[1];

                }

                scrollBottom(richTextBox15);
                scrollBottom(richTextBox14);
            }
        }

        private void scrollBottom(RichTextBox r)
        {
            //让文本框获取焦点
            r.Focus();
            //设置光标的位置到文本尾
            r.Select(r.TextLength, 0);
            //滚动到控件光标处
            r.ScrollToCaret();
        }

        private void query()
        {
            SqlDataAdapter sqlDa = new SqlDataAdapter("select * from msg", myConnection);
            DataTable dt = new DataTable();
            sqlDa.Fill(dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Console.Write(dt.Rows[i]["ch1"].ToString());
                Console.Write(dt.Rows[i]["ch2"].ToString());
                Console.Write(dt.Rows[i]["ch3"].ToString());
                Console.Write(dt.Rows[i]["ch4"].ToString());
                Console.Write(dt.Rows[i]["ch5"].ToString());
                Console.Write(dt.Rows[i]["ch6"].ToString());
                Console.Write(dt.Rows[i]["ch7"].ToString());
                Console.Write(dt.Rows[i]["ch8"].ToString());
                Console.WriteLine();
            }
        }

        private void insert(String text)
        {
            int index = 1;
            String[] strMsg = text.Split("\n".ToArray());
            String userMsg = "";
            if (strMsg.Length == 8)
            {
                index = 0;
            }
            else
            {
                userMsg = strMsg[0];
            }
            String sql = "insert into msg(userinfo,ch1,ch2,ch3,ch4,ch5,ch6,ch7,ch8)" +
                " values('" + userMsg + "', '"
                + strMsg[index++] + "', '" 
                + strMsg[index++] + "', '" 
                + strMsg[index++] + "', '" 
                + strMsg[index++] + "', '" 
                + strMsg[index++] + "', '" 
                + strMsg[index++] + "', '" 
                + strMsg[index++] + "', '" 
                + strMsg[index++] + "')";
            SqlCommand sqlCmd = new SqlCommand(sql, myConnection);
            sqlCmd.ExecuteNonQuery();
        }

        //双击时，弹出具体的某个用户数据
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            String selected = (String) listBox1.SelectedItem;
            Console.WriteLine(selected);
            this.str_select11 = selected;
            /*richTextBox1.Clear();
            richTextBox2.Clear();
            richTextBox3.Clear();
            richTextBox4.Clear();
            richTextBox5.Clear();
            richTextBox6.Clear();
            richTextBox7.Clear();
            richTextBox8.Clear();*/
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            richTextBox2.Clear();
            richTextBox3.Clear();
            richTextBox4.Clear();
            richTextBox5.Clear();
            richTextBox6.Clear();
            richTextBox7.Clear();
            richTextBox8.Clear();
            iClear = 1;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (m_exit2 == false)
            {
                m_exit2 = true;
                button3.Text = "手机连接";
                socketwatch2.Close();
            }
            else
            {
                m_exit2 = false;
                button3.Text = "手机关闭";
                init2();
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void label18_Click(object sender, EventArgs e)
        {

        }

        private void label19_Click(object sender, EventArgs e)
        {

        }

        private void label20_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox10_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void tabPage4_Click(object sender, EventArgs e)
        {

        }

        private void tabPage5_Click(object sender, EventArgs e)
        {

        }
    }
}
