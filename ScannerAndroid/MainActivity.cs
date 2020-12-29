using System;
using System.Text;
using System.Threading.Tasks;

using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

using Newtonsoft.Json;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using ZXing.Mobile;

[assembly: UsesPermission(Android.Manifest.Permission.Flashlight)]
namespace ScannerAndroid
{
    class Constants
    {
        public static readonly string Exchange = "demo.exchange.topic.task";
        public static readonly string Queue = "demo.queue.durable.task";
    }

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private readonly IConnection _conn;
        private readonly IModel _channel;

        public MainActivity() : base()
        {
            _conn = InitConnection();

            _channel = InitChannel(_conn);
        }

        private IConnection InitConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "157.245.49.108",
                UserName = "guest",
                Password = "guest",
                Port = 5672,
                VirtualHost = "/",
            };

            var conn = factory.CreateConnection();
            return conn;
        }

        private IModel InitChannel(IConnection conn)
        {
            var channel = conn.CreateModel();
            channel.ExchangeDeclare(Constants.Exchange, ExchangeType.Topic);
            channel.QueueDeclare(Constants.Queue, true, false, false, null);
            channel.QueueBind(Constants.Queue, Constants.Exchange, "*.queue.durable.task", null);
            channel.BasicQos(0, 1, false);
            return channel;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            var btn = FindViewById<Button>(Resource.Id.buttonScan);
            btn.Click += ScanOnClick;

            Xamarin.Essentials.Platform.Init(Application);
            ZXing.Net.Mobile.Forms.Android.Platform.Init();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private async void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Purge queue", Snackbar.LengthLong)
                .SetAction("Action", (View.IOnClickListener)null).Show();

            _channel.QueuePurge(Constants.Queue);
        }

        private async void ScanOnClick(object sender, EventArgs eventArgs)
        {
            // Initialize the scanner first so it can track the current context
            MobileBarcodeScanner.Initialize(Application);

            var scanner = new MobileBarcodeScanner();

            var result = await scanner.Scan();

            if (result != null)
            {
                Console.WriteLine("Scanned Barcode: " + result.Text);
                Toast.MakeText(Application.Context, "Scanned Barcode: " + result.Text, ToastLength.Short).Show();

                var message = new MqMsg
                {
                    TaskName = "scan",
                    TaskType = 2,
                    TaskData = result.Text,
                };
                var serialized = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(serialized);
                _channel.BasicPublish(Constants.Exchange, Constants.Queue, null, body);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
