/* Provided by xmetrix on livecoding. */

/*namespace lctv_bot
{
    //private XmppClientConnection conn;


    public partial class Form1 : Form
    {
        private XmppClientConnection conn;
        Jid chatjid;
        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            Console.WriteLine("loaded");

            //set the default chatroom we want to join 
            chatjid = new Jid("10ktactics@chat.livecoding.tv");
            //create a new client connection
            conn = new XmppClientConnection();
            //add handlers for various shits 
            // some of these handlers are events that trigger other events. onLogin triggers disco
            conn.OnLogin += conn_OnLogin;
            conn.OnMessage += conn_OnMessage;
        }

        void conn_OnMessage(object sender, agsXMPP.protocol.client.Message msg)
        {
            Console.WriteLine("{0}: {1}", msg.From, msg.Body);
        }


        void conn_OnLogin(object sender)
        {
            Console.WriteLine("SUP on login!");
            //discovery manager for the current connection
            DiscoManager dm = new DiscoManager(conn);
            dm.DiscoverItems(new Jid(conn.Server), new IqCB(onDisco), null);

        }

        //disco result 
        private void onDisco(object sender, IQ iq, object data)
        {
            if (iq.Type == IqType.result)
            {
                Element e = iq.Query;
                if (e != null && e.GetType() == typeof(DiscoItems))
                {
                    DiscoItems di = e as DiscoItems;
                    DiscoItem[] ditems = di.GetDiscoItems();
                    DiscoManager dm = new DiscoManager(conn);
                    foreach (DiscoItem i in ditems)
                    {
                        //gets a list of items chat.livecoding.tv pubsub.livecoding.tv and vjud.livecoding.tv so lets get info for each item
                        //Console.WriteLine(i.ToString());
                        dm.DiscoverInformation(i.Jid, new IqCB(onDiscoInfo), i);
                    }
                }
            }
        }

        private void onDiscoInfo(object sender, IQ iq, object data)
        {
            Console.WriteLine(iq.ToString());
        }
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            Jid jid = new Jid("10kbot@livecoding.tv");
            conn.Server = jid.Server;
            conn.Username = jid.User;
            conn.Password = "";
            conn.Resource = null;
            conn.Priority = 10;
            conn.Port = 5222;
            conn.UseSSL = false;
            conn.AutoResolveConnectServer = true;
            conn.UseStartTLS = true;
            conn.Open();
            Console.WriteLine("Connection attempted/opened");
            await Task.Delay(2000);
            //return if disconnected
            if (conn.XmppConnectionState == XmppConnectionState.Disconnected)
                return;
            Presence pres = new Presence();
            chatjid.Resource = "lctvbot"; //your bot's name
            pres.To = chatjid;
            conn.Send(pres);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (txtMsg.TextLength == 0)
                return;

            agsXMPP.protocol.client.Message msg = new agsXMPP.protocol.client.Message();
            msg.Type = MessageType.chat;
            msg.To = chatjid;
            msg.Body = txtMsg.Text;
            conn.Send(msg);
            txtMsg.Text = "";

        }
    }
}*/