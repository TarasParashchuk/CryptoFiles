using Android.Widget;
using System;
using Android.Util;
using Android.Support.V4.App;
using Android;
using Android.Content;
using CryptoFiles.Model;
using Newtonsoft.Json;
using Android.Support.V7.App;
using Android.App;
using Android.OS;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CryptoFiles
{
    class SupportCryptoFunction
    {
        private Context context;
        private ModelForService DataForService;
        private Intent intent;
        private DataBaseHelp db;

        public SupportCryptoFunction(Context context)
        {
            this.context = context;

            DataBaseConnection();
        }

        public DataBaseHelp DataBaseConnection()
        {
            var conn = new DataBase().SQLiteConnection();
            db = new DataBaseHelp(conn);
            return db;
        }

        public List<ModelDataFile> GetTableFiles(int id)
        {
            return db.GetItems<ModelDataFile>().Where(t => t.id_users == id).ToList();
        }

        public List<ModelCategory> GetTabelCategory()
        {
            return db.GetItems<ModelCategory>();
        }

        public ModelCategory GetItemCategory(int id)
        {
            return db.GetItems<ModelCategory>().Where(t => t.id == id).First();
        }

        public void CryptoSupportStartServices(string path, string password, int key_size, bool flag_crypto)
        {
            intent = new Intent(context, typeof(EncryptionDecryptionService));
            IsStoragePermissionGranted();
            if (flag_crypto)
            {
                DataForService = new ModelForService() { Name_input_file = path + ".aes", Name_output_file = path, Password = password, Key_size = key_size };
                intent.SetAction(EncryptionDecryptionService.ActionDecryptionFile);
                Toast.MakeText(context, "Дешифрование начато", ToastLength.Long).Show();
            }
            else
            {
                DataForService = new ModelForService() { Name_input_file = path, Name_output_file = path + ".aes", Password = password, Key_size = key_size };
                intent.SetAction(EncryptionDecryptionService.ActionEcryptionFile);
                Toast.MakeText(context, "Шифрование начато", ToastLength.Long).Show();
            }
            intent.PutExtra("about_file", JsonConvert.SerializeObject(DataForService));
            context.StartService(intent);
        }

        public void CryptoSupportStopServices(ModelDataFile information_about_file, string SHA, bool flag_crypto)
        {
            if (flag_crypto)
            {
                db.DeleteItem<ModelDataFile>(information_about_file.id);
                File.Delete(information_about_file.FilePath + ".aes");

                Toast.MakeText(context, "Файл розшифрован", ToastLength.Long).Show();

                SetCountFiles(information_about_file.id_users, true);
                Show_Dialog_SHA(information_about_file.FileSHA, SHA);
            }
            else
            {
                information_about_file.FileSHA = SHA;
                db.SaveItem(information_about_file, false);
                File.Delete(information_about_file.FilePath);
                
                Toast.MakeText(context, "Файл зашифрован", ToastLength.Long).Show();

                SetCountFiles(information_about_file.id_users, false);
            }

            context.StopService(intent);
        }

        public void CryptoSupportDelete(ModelDataFile information_about_file)
        {
            File.Delete(information_about_file.FilePath + ".aes");
            Toast.MakeText(context, "Файл удален", ToastLength.Short).Show();
            db.DeleteItem<ModelDataFile>(information_about_file.id);
            SetCountFiles(information_about_file.id_users, true);
        }

        private void SetCountFiles(int id, bool flag)
        {
            var item = GetItemCategory(id);
            if(flag)
                item.Count -= 1;
            else item.Count += 1;
            db.SaveItem(item, true);
        }

        public bool IsStoragePermissionGranted()
        {
            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.M)
            {
                if (context.CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Android.Content.PM.Permission.Granted)
                {
                    Log.Verbose("Verbose", "Permission is granted");
                    return true;
                }
                else
                {
                    Log.Verbose("Verbose", "Permission is revoked");
                    ActivityCompat.RequestPermissions((Activity)context, new String[] { Manifest.Permission.WriteExternalStorage }, 1);
                    return false;
                }
            }
            else
            {
                Log.Verbose("Verbose", "Permission is granted");
                return true;
            }
        }

        private void Show_Dialog_SHA(string file_sha, string new_file_sha)
        {
            var activity = (AppCompatActivity)context;
            var ft = activity.SupportFragmentManager.BeginTransaction();
            var bundle = new Bundle();
            bundle.PutString("sha_crypto", file_sha);
            bundle.PutString("sha_decrypto", new_file_sha);
            var dialogFragment = DialogSHA.NewInstace(bundle);
            ft.Add(dialogFragment, "dialog_SHA");
            ft.CommitAllowingStateLoss();
        }

    }
}