using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using CryptoFiles.Model;
using Newtonsoft.Json;

namespace CryptoFiles
{
    [Activity(Label = "DetailActivity", MainLauncher = false)]
    class DetailActivity : AppCompatActivity, CompleteReceiver.CompleteCreateCryptoFile
    {
        private ModelDataFile information_about_file;
        private SupportCryptoFunction support_func;
        private CompleteReceiver receiver;
        private string password = string.Empty;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.layout_detailpage);

            receiver = new CompleteReceiver();

            Title = "Описание";
            information_about_file = JsonConvert.DeserializeObject<ModelDataFile>(Intent.GetStringExtra("about_file"));
            password = Intent.GetStringExtra("key");

            var File_Name = FindViewById<TextView>(Resource.Id.Name_File);
            File_Name.Text = information_about_file.FileName;
            var File_Type = FindViewById<TextView>(Resource.Id.Type_File);
            File_Type.Text = information_about_file.FileType;
            var File_Size = FindViewById<TextView>(Resource.Id.Size_File);
            File_Size.Text = EncryptionDecryptionService.ConvertToSizeWithName(information_about_file.FileSize);
            var File_Date = FindViewById<TextView>(Resource.Id.Date_File);
            File_Date.Text = information_about_file.FileDate;
            var File_SHA = FindViewById<TextView>(Resource.Id.File_SHA);
            File_SHA.Text = information_about_file.FileSHA;
            var File_Pash = FindViewById<TextView>(Resource.Id.File_Path);
            File_Pash.Text = information_about_file.FilePath;

            var Delete_Button = FindViewById<Button>(Resource.Id.Button_Delete);
            var Decrypto_Button = FindViewById<Button>(Resource.Id.Button_Decrypto);
            Delete_Button.Click += Delete_Button_Click;
            Decrypto_Button.Click += Decrypto_Button_Click;

            support_func = new SupportCryptoFunction(this);
        }

        protected override void OnResume()
        {
            base.OnResume();
            RegisterReceiver(receiver, new IntentFilter(CompleteReceiver.const_action_progress_decrypt));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (receiver != null)
            {
                UnregisterReceiver(receiver);
                receiver = null;
            }
        }

        private void Decrypto_Button_Click(object sender, System.EventArgs e)
        {
            support_func.CryptoSupportStartServices(information_about_file.FilePath, password, information_about_file.Key_size, true);
        }

        private void Delete_Button_Click(object sender, System.EventArgs e)
        {
            support_func.CryptoSupportDelete(information_about_file);
            var intentActivity = new Intent(this, typeof(MainActivity));
            intentActivity.PutExtra("id", information_about_file.id_users);
            StartActivity(intentActivity);
        }
 
        public void ProgressCryptoFile(bool complete_flag, string SHA)
        {
            if (complete_flag)
            {
                support_func.CryptoSupportStopServices(information_about_file, SHA, true);

                var intentActivity = new Intent(this, typeof(MainActivity));
                intentActivity.PutExtra("id", information_about_file.id_users);
                intentActivity.PutExtra("key", password);
                StartActivity(intentActivity);
                Finish();
            }
        }

        public override void OnBackPressed()
        {
            var intentActivity = new Intent(this, typeof(MainActivity));
            intentActivity.PutExtra("id", information_about_file.id_users);
            intentActivity.PutExtra("key", password);
            StartActivity(intentActivity);
            Finish();
        }
    }
}