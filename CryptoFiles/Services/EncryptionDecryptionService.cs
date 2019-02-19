using System;
using Android.App;
using Android.Content;
using Android.OS;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;
using CryptoFiles.Model;
using System.Threading;
using Android.Support.V4.App;
using System.Text;
using System.Linq;

namespace CryptoFiles
{
    [Service]
    public class EncryptionDecryptionService : Service
    {
        public const string ActionEcryptionFile = "action.EcryptionFile";
        public const string ActionDecryptionFile = "action.Decryption.File";
        static readonly string[] SizeNames = { "bytes", "KB", "MB", "GB" };

        private ModelForService info;
        private int read;
        private string FileSHA = string.Empty;
        private NotificationManagerCompat notificationManager;
        private NotificationCompat.Builder builder;
        static readonly int NOTIFICATION_ID = 1000;
        static readonly string CHANNEL_ID = "location_notification";

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            info = JsonConvert.DeserializeObject<ModelForService>(intent.GetStringExtra("about_file"));
            switch (intent.Action)
            {
                case ActionEcryptionFile: MainFunctionCryptFileAsync(false); break;
                case ActionDecryptionFile: MainFunctionCryptFileAsync(true); break;
            }
            return StartCommandResult.Sticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public void MainFunctionCryptFileAsync(bool flag)
        {
            var title_notification = string.Empty;
            var sourceFilename = info.Name_input_file;
            var destinationFilename = info.Name_output_file;
            var password = info.Password;
            var key_size = info.Key_size;

            using (var aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.KeySize = key_size;
                aesAlg.BlockSize = aesAlg.LegalBlockSizes[0].MinSize;
                aesAlg.Key = SizeKeyandIV(Encoding.UTF8.GetBytes(password), aesAlg.KeySize);
                aesAlg.IV = SizeKeyandIV(Encoding.UTF8.GetBytes(password), aesAlg.BlockSize);

                CreateNotificationChannel();

                ICryptoTransform transform;
                if (flag)
                {
                    transform = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    title_notification = "Дешифрования файла";
                }
                else
                {
                    transform = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                    title_notification = "Шифрования файла";
                }

                new Thread(new ThreadStart(delegate
                {
                    if (File.Exists(destinationFilename))
                        File.Delete(destinationFilename);

                    using (FileStream destination = new FileStream(destinationFilename, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(destination, transform, CryptoStreamMode.Write))
                        {
                            try
                            {
                                using (FileStream source = new FileStream(sourceFilename, FileMode.Open, FileAccess.Read))
                                {
                                    notificationManager = NotificationManagerCompat.From(this);
                                    builder = new NotificationCompat.Builder(this, CHANNEL_ID)
                                    .SetContentTitle(title_notification)
                                    .SetSmallIcon(Resource.Drawable.circlelock);

                                    float i = 0;
                                    byte[] buffer = new byte[1024 * 10240];

                                    while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        cryptoStream.Write(buffer, 0, read);

                                        i += (float)read / source.Length * 100;
                                        var progress = (int)i;

                                        builder.SetProgress(100, progress, false);
                                        builder.SetContentText(progress + "%");
                                        notificationManager.Notify(NOTIFICATION_ID, builder.Build());
                                        Thread.Sleep(200);
                                    }
                                }
                            }
                            catch (CryptographicException exception)
                        {
                            if (exception.Message == "Padding is invalid and cannot be removed.")
                                throw new ApplicationException("Universal Microsoft Cryptographic Exception (Not to be believed!)", exception);
                            else
                                throw;
                        }
                    }
                }

                builder.SetContentTitle("Получение хеш суми файла");
                notificationManager.Notify(NOTIFICATION_ID, builder.Build());
                var intent = new Intent();
                if (flag)
                {
                    FileSHA = GetSHA(destinationFilename, true);
                    intent.SetAction(CompleteReceiver.const_action_progress_decrypt);
                    intent.PutExtra("complete_flag_decrypt", true);
                }
                else
                {
                    FileSHA = GetSHA(sourceFilename, true);
                    intent.SetAction(CompleteReceiver.const_action_progress_encrypt);
                    intent.PutExtra("complete_flag_encrypt", true);
                }

                intent.PutExtra("file_sha", FileSHA);
                intent.PutExtra("flag_crypto", flag);
                builder.SetContentTitle(title_notification).SetContentText("Процесс завершен").SetProgress(0, 0, false);
                notificationManager.Notify(NOTIFICATION_ID, builder.Build());
                SendBroadcast(intent);
            })).Start();
        }
    }

    void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;

        var name = Resources.GetString(Resource.String.channel_name);
        var description = GetString(Resource.String.channel_description);
        var channel = new NotificationChannel(CHANNEL_ID, name, NotificationImportance.Low)
        {
            Description = description
        };

        var notificationManager = (NotificationManager)GetSystemService(NotificationService);
        notificationManager.CreateNotificationChannel(channel);
    }

    public static string GetSHA(string str, bool flag)
    {
        byte[] hash;
        using (var SHA256 = new SHA256CryptoServiceProvider())
        {
            if (flag)
            {
                using (var stream = new BufferedStream(File.OpenRead(str), 1024 * 1024))
                {
                    hash = SHA256.ComputeHash(stream);
                }
            }
            else
            {
                hash = SHA256.ComputeHash(Encoding.UTF8.GetBytes(str));
            }
        }
        return BitConverter.ToString(hash);
    }

    public static string ConvertToSizeWithName(long byte_size_file)
    {
        var index = 0;
        var const_size = 1000;
        var size_file = byte_size_file;

        while (size_file > const_size)
        {
            size_file = size_file / const_size;
            index++;
        }
        return String.Format("{0} {1}", String.Format("{0:0.00}", byte_size_file / Math.Pow(const_size, index)), SizeNames[index]);
    }

    private byte[] SizeKeyandIV(byte[] str, int alg_size_key)
    {
        byte[] new_str = null;
        var length = str.Length;

        if (length * 8 > alg_size_key)
        {
            new_str = str.Take(alg_size_key / 8).ToArray();
        }
        else
        {
            new_str = str;
            for (var i = length * 8; i < alg_size_key; i += length * 8)
            {
                if (i + length * 8 > alg_size_key)
                {
                    new_str = new_str.Concat(str.Take((alg_size_key - i) / 8).ToArray()).ToArray();
                    break;
                }
                else new_str = new_str.Concat(str).ToArray();
            }
        }
        return new_str;
    }
}
}