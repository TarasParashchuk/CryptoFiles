 using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using System.Collections.Generic;
using Android.Support.Design.Widget;
using System;
using System.IO;
using Android.Content;
using CryptoFiles.Model;
using Newtonsoft.Json;
using System.Threading;
using Java.Lang.Reflect;
using AlertDialog = Android.App.AlertDialog;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Views;
using System.Linq;
using Android.Provider;
using Android.Database;

namespace CryptoFiles
{
    [Activity(Label = "MainActivity", MainLauncher = false, Theme = "@style/Theme.AppCompat.Light.NoActionBar")]
    public class MainActivity : AppCompatActivity, CompleteReceiver.CompleteCreateCryptoFile
    {
        private List<ModelDataFile> list;
        private ListView List_Files;
        private TextView info_text;
        private CompleteReceiver receiver;
        private ListViewAdapter ListAdapter;
        private ModelDataFile information_about_file;
        private SupportCryptoFunction support_func;
        private bool flag_crypto;
        private int key_size = 128;
        private string password;
        private int id;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            receiver = new CompleteReceiver();

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Главная";

            id = Intent.GetIntExtra("id", 0);
            password = Intent.GetStringExtra("key");

            List_Files = FindViewById<ListView>(Resource.Id.ListFiles);
            var Add_Button = FindViewById<FloatingActionButton>(Resource.Id.AddButton);
            info_text = FindViewById<TextView>(Resource.Id.text_informal);
            Add_Button.Click += AddButtonOnClick;
            List_Files.ItemClick += List_Files_ItemClick;
            List_Files.ItemLongClick += List_Files_ItemLongClick;

            support_func = new SupportCryptoFunction(this);
            LoadData();
        }

        protected override void OnResume()
        {
            base.OnResume();
            RegisterReceiver(receiver, new IntentFilter(CompleteReceiver.const_action_progress_encrypt));
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

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_files, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var id = item.ItemId;

            if (id == Resource.Id.menu_name_file)
                list = list.OrderByDescending(u => u.FileName).ToList();
            else if (id == Resource.Id.menu_size)
                list = list.OrderByDescending(u => u.FileSize).ToList();
            else if (id == Resource.Id.menu_time)
                list = list.OrderByDescending(u => DateTime.ParseExact(u.FileDate, "dd.MM.yyyy", null)).ToList();

            ListAdapter = new ListViewAdapter(this, list);
            List_Files.Adapter = ListAdapter;
            Toast.MakeText(this, "Сортировка по: " + item.TitleFormatted, ToastLength.Short).Show();
            return base.OnOptionsItemSelected(item);
        }

        private void LoadData()
        {
            list = support_func.GetTableFiles(id);
            if (list.Count != 0)
            {
                ListAdapter = new ListViewAdapter(this, list);
                List_Files.Adapter = ListAdapter;
                info_text.Text = string.Empty;
            }
            else
            {
                List_Files.SetAdapter(null);
                info_text.Text = "Добавите файл";
            }
        }

        private void List_Files_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            information_about_file = list[e.Position];
            var popupMenu = new PopupMenu(this, e.View);

            try
            {
                var fields = popupMenu.Class.GetDeclaredFields();
                foreach (Field field in fields)
                {
                    if ("mPopup".Equals(field.Name))
                    {
                        field.Accessible = true;
                        Java.Lang.Object menuPopupHelper = field.Get(popupMenu);
                        Method setForceIcons = menuPopupHelper.Class.GetDeclaredMethod("setForceShowIcon", Java.Lang.Boolean.Type);
                        setForceIcons.Invoke(menuPopupHelper, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            popupMenu.Inflate(Resource.Layout.menu_itemlist);
            popupMenu.Show();
            popupMenu.MenuItemClick += PopupMenu_MenuItemClick;
        }

        private void PopupMenu_MenuItemClick(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.menuDelete:
                    {
                        support_func.CryptoSupportDelete(information_about_file);
                        LoadData();
                        break;
                    }
                case Resource.Id.menuDecrypt:
                    {
                        flag_crypto = true;
                        support_func.CryptoSupportStartServices(information_about_file.FilePath, password, information_about_file.Key_size, flag_crypto);
                        break;
                    }
            }
        }

        private void List_Files_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(DetailActivity));
                intent.PutExtra("about_file", JsonConvert.SerializeObject(list[e.Position]));
                intent.PutExtra("key", password);
                StartActivity(intent);
                Finish();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void AddButtonOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                new Thread(new ThreadStart(delegate
                {
                    var fileData = UploadFile.OnUpload().Result;
                    RunOnUiThread(() =>
                    {
                        if (fileData != null)
                        {
                            information_about_file = new ModelDataFile()
                            {
                                FileName = fileData.FileName,
                                FileSize = fileData.GetStream().Length,
                                FileType = Path.GetExtension(fileData.FileName),
                                FileDate = DateTime.Now.ToShortDateString(),
                                FilePath = fileData.FilePath,
                                FileSHA = string.Empty,
                                Key_size = key_size,
                                id_users = id
                            };
                            ShowDialogSelectKeySize();
                        }
                        else Toast.MakeText(this, "Файл не выбран", ToastLength.Long).Show();
                    });
                })).Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void ProgressCryptoFile(bool complete_flag, string SHA)
        {
            if (complete_flag)
            {
                support_func.CryptoSupportStopServices(information_about_file, SHA, flag_crypto);
                LoadData();
            }
        }

        private void ShowDialogSelectKeySize()
        {
            string[] key_options = Resources.GetStringArray(Resource.Array.keyArray);
            var builder = new AlertDialog.Builder(this)
                .SetTitle("Выберите размер ключа")
                .SetNegativeButton("Отмена", CancelClicked)
                .SetPositiveButton("Oк", OKClicked)
                .SetSingleChoiceItems(key_options, 0, (o, e) =>
            {
                switch (e.Which)
                {
                    case 0: key_size = Convert.ToInt32(key_options[0]); break;
                    case 1: key_size = Convert.ToInt32(key_options[1]); break;
                    case 2: key_size = Convert.ToInt32(key_options[2]); break;
                    default: break;
                }
            });
            builder.Create();
            builder.Show();
        }

        private void CancelClicked(object sender, DialogClickEventArgs e)
        {
            ((Dialog)sender).Dismiss();
        }

        private void OKClicked(object sender, DialogClickEventArgs e)
        {
            information_about_file.Key_size = key_size;
            flag_crypto = false;
            support_func.CryptoSupportStartServices(information_about_file.FilePath, password, key_size, flag_crypto);
        }

        public override void OnBackPressed()
        {
            StartActivity(typeof(CategoryActivity));
            Finish();
        }
    }
}