using Android.Content;

namespace CryptoFiles
{
    [BroadcastReceiver]
    public class CompleteReceiver : BroadcastReceiver
    {
        private CompleteCreateCryptoFile _CompleteCreateCryptoFile;
        public const string const_action_progress_encrypt = "receiver.complete.encrypt.file";
        public const string const_action_progress_decrypt = "receiver.complete.decrypt.file";

        public interface CompleteCreateCryptoFile
        {
            void ProgressCryptoFile(bool complete_flag, string SHA);
        }

        public override void OnReceive(Context context, Intent intent)
        {
            var flag_progress = false;
            switch (intent.Action)
            {
                case const_action_progress_encrypt: flag_progress = intent.GetBooleanExtra("complete_flag_encrypt", false); break;
                case const_action_progress_decrypt: flag_progress = intent.GetBooleanExtra("complete_flag_decrypt", false); break;
            }
            var SHA = intent.GetStringExtra("file_sha");
            _CompleteCreateCryptoFile = (CompleteCreateCryptoFile)context;
            _CompleteCreateCryptoFile.ProgressCryptoFile(flag_progress, SHA);
        }
    }
}