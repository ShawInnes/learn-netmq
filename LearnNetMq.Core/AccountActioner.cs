using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Serilog;

namespace LearnNetMq.Core
{
    public class AccountActioner
    {
        public class ShimHandler : IShimHandler
        {
            private PairSocket shim;
            private NetMQPoller poller;

            public void Initialise(object state)
            {
            }

            public void Run(PairSocket shim)
            {
                this.shim = shim;
                shim.ReceiveReady += OnShimReady;
                shim.SignalOK();
                poller = new NetMQPoller {shim};
                poller.Run();
            }

            private void OnShimReady(object sender, NetMQSocketEventArgs e)
            {
                string command = e.Socket.ReceiveFrameString();
                switch (command)
                {
                    case NetMQActor.EndShimMessage:
                        Log.Information("Actor received EndShimMessage");
                        poller.Stop();
                        break;
                    case "AmendAccount":
                        Log.Information("Actor received AmendAccount message");
                        string accountJson = e.Socket.ReceiveFrameString();
                        Account account
                            = JsonConvert.DeserializeObject<Account>(accountJson);
                        string accountActionJson = e.Socket.ReceiveFrameString();
                        AccountAction accountAction
                            = JsonConvert.DeserializeObject<AccountAction>(
                                accountActionJson);
                        Log.Information("Incoming Account details: {Account}", account);
                        AmendAccount(account, accountAction);
                        shim.SendFrame(JsonConvert.SerializeObject(account));
                        break;
                }
            }

            private void AmendAccount(Account account, AccountAction accountAction)
            {
                switch (accountAction.TransactionType)
                {
                    case TransactionType.Credit:
                        account.Balance += accountAction.Amount;
                        break;
                    case TransactionType.Debit:
                        account.Balance -= accountAction.Amount;
                        break;
                }
            }
        }

        private NetMQActor _actor;

        public void Start()
        {
            if (_actor != null)
                return;
            _actor = NetMQActor.Create(new ShimHandler());
        }

        public void Stop()
        {
            if (_actor != null)
            {
                _actor.Dispose();
                _actor = null;
            }
        }

        public void SendPayload(Account account, AccountAction accountAction)
        {
            if (_actor == null)
                return;
            Log.Information("About to send person to Actor");
            var message = new NetMQMessage();
            message.Append("AmendAccount");
            message.Append(JsonConvert.SerializeObject(account));
            message.Append(JsonConvert.SerializeObject(accountAction));
            _actor.SendMultipartMessage(message);
        }

        public Account GetPayLoad()
        {
            var frameString = _actor.ReceiveFrameString();

            Log.Information("FrameString: {FrameString}", frameString);

            return JsonConvert.DeserializeObject<Account>(frameString);
        }
    }
}
