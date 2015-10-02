using Falafel.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Telerik.Charting;
using Windows.UI.Core;

namespace PhotonOscope.Classes
{
    public class MainViewModel : ViewModelBase
    {
        const int TIMEOUT_MILLISECONDS = 5000;
        const int MAX_BUFFER_SIZE = 2048;

        Socket _socket = null;
        static ManualResetEvent _clientDone = new ManualResetEvent(false);
        SocketAsyncEventArgs socketEventArg;
        Timer receiveTimer;

        public MainViewModel()
        {
            _Connect = new DelegateCommand((x) => {
                DnsEndPoint hostEntry = new DnsEndPoint(this.IP, this.Port);
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
                socketEventArg.RemoteEndPoint = hostEntry;
                socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(async delegate (object s, SocketAsyncEventArgs e)
                {
                    // Retrieve the result of this request
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        this.Status = e.SocketError.ToString();
                        RaisePropertyChanged("Connect");
                        RaisePropertyChanged("Receive");
                        RaisePropertyChanged("Disconnect");
                    });
                    Receive.Execute(null);
                });
                _socket.ConnectAsync(socketEventArg);
            }, (y) => { return _socket == null || (_socket != null && !_socket.Connected); });
            _Disconnect = new DelegateCommand(async (x) =>
            {
                _socket.Shutdown(SocketShutdown.Both);
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    this.Status = "Disconnected";
                    RaisePropertyChanged("Connect");
                    RaisePropertyChanged("Receive");
                    RaisePropertyChanged("Disconnect");
                });
            }, (y) => { return _socket != null && _socket.Connected; });
            _Receive = new DelegateCommand(async (x) =>
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    this.Status = "Listening";
                });
                receiveTimer = new Timer(this.Receive_Timer_Tick, null, 0, System.Threading.Timeout.Infinite);
            }, (y) => { return _socket != null && _socket.Connected; });
        }

        private void Receive_Timer_Tick(object state)
        {
            receiveTimer.Dispose();  
            bool go = false;          
            if (_socket != null && _socket.Connected)
            {
                if (socketEventArg == null)
                {
                    socketEventArg = new SocketAsyncEventArgs();
                    socketEventArg.RemoteEndPoint = _socket.RemoteEndPoint;
                    socketEventArg.SetBuffer(new Byte[MAX_BUFFER_SIZE], 0, MAX_BUFFER_SIZE);
                    socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(async delegate (object s, SocketAsyncEventArgs e)
                    {
                        if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
                        {
                            //First 2 ints contain the trigger index offset and the sample rate.
                            var triggerIndex = (Int16)((e.Buffer[1] << 8) | e.Buffer[0]);
                            var samplerate = (Int16)((e.Buffer[3] << 8) | e.Buffer[2]);

                            Int16[] data = new Int16[(e.BytesTransferred - 4) / 2];
                            for (int i = 2; i < e.BytesTransferred / 2; i++)
                            {
                                int bi = i * 2;
                                byte upper = e.Buffer[bi + 1];
                                byte lower = e.Buffer[bi];
                                data[i - 2] = (Int16)((upper << 8) | lower);
                            }
                            int index = -triggerIndex;

                            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                this.Trigger = triggerIndex;
                                this.SampleTime = samplerate;
                                this.Status = string.Format("{0} Points Received", data.Length);
                                this.Points = null;
                                this.Points = data.Select(d => new ScatterDataPoint()
                                {
                                    XValue = samplerate * index++,
                                    YValue = d,
                                });
                            });
                        }                        
                        else
                        {
                            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                this.Status = e.BytesTransferred > 0 ? e.SocketError.ToString() : "No data received";
                            });
                        }

                        _clientDone.Set();
                    });
                }
                _clientDone.Reset();
                _socket.ReceiveAsync(socketEventArg);
                go = _clientDone.WaitOne(TIMEOUT_MILLISECONDS);
                if (!go)
                {
                    this.Disconnect.Execute(null);
                }
            }
            if (go && _socket != null && _socket.Connected)
            {
                receiveTimer = new Timer(this.Receive_Timer_Tick, null, 0, System.Threading.Timeout.Infinite);
            }
        }

        public CoreDispatcher Dispatcher { get; set; }

        IEnumerable<ScatterDataPoint> _Points;
        public IEnumerable<ScatterDataPoint> Points
        {
            get
            {
                return _Points;
            }
            set
            {
                _Points = value;
                RaisePropertyChanged("Points");
            }
        }

        string _IP = "192.168.1.6";
        public string IP
        {
            get
            {
                return _IP;
            }
            set
            {
                _IP = value;
                RaisePropertyChanged("IP");
            }
        }

        int _port = 8007;
        public int Port
        {
            get
            {
                return _port;
            }
            set
            {
                _port = value;
                RaisePropertyChanged("Port");
            }
        }

        int _trigger;
        public int Trigger
        {
            get
            {
                return _trigger;
            }
            set
            {
                _trigger = value;
                RaisePropertyChanged("Trigger");
            }
        }

        int _SampleTime;
        public int SampleTime
        {
            get
            {
                return _SampleTime;
            }
            set
            {
                _SampleTime = value;
                RaisePropertyChanged("SampleTime");
            }
        }

        string _Status;
        public string Status
        {
            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
                RaisePropertyChanged("Status");
            }
        }

        bool _LineSeries = true;
        public bool LineSeries
        {
            get
            {
                return _LineSeries;
            }
            set
            {
                _LineSeries = value;
                RaisePropertyChanged("LineSeries");
            }
        }

        int _SampleSize;
        public int SampleSize
        {
            get
            {
                return _SampleSize;
            }
            set
            {
                _SampleSize = value;
                RaisePropertyChanged("SampleSize");
            }
        }

        DelegateCommand _Connect;
        public DelegateCommand Connect
        {
            get
            {
                return _Connect;
            }
        }

        DelegateCommand _Disconnect;
        public DelegateCommand Disconnect
        {
            get
            {
                return _Disconnect;
            }
        }

        DelegateCommand _Receive;
        public DelegateCommand Receive
        {
            get
            {
                return _Receive;
            }
        }
    }
}
