using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace CryptoFiles
{
    class DialogSHA : Android.Support.V4.App.DialogFragment
    {
        private static string sha_crypto = string.Empty;
        private static string sha_decrypto = string.Empty;

        public static DialogSHA NewInstace(Bundle bundle)
        {
            var fragment = new DialogSHA();
            sha_crypto = bundle.GetString("sha_crypto");
            sha_decrypto = bundle.GetString("sha_decrypto");
            fragment.Arguments = bundle;
            return fragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.dialogforSHA, container, false);
            var text_sha_crypto = view.FindViewById<TextView>(Resource.Id.textViewCrypto);
            text_sha_crypto.Text = sha_crypto;
            var text_sha_decrypto = view.FindViewById<TextView>(Resource.Id.textViewDecrypto);
            text_sha_decrypto.Text = sha_decrypto;
            var button = view.FindViewById<Button>(Resource.Id.buttonOK);
            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);
            Dialog.SetCanceledOnTouchOutside(false);
            button.Click += delegate {
                Dismiss();
            };
            
            return view;
        }
    }
}