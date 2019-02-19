using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Text.Method;
using Android.Views;
using Android.Widget;

namespace CryptoFiles
{
    class DialogPassword : Android.Support.V4.App.DialogFragment
    {
        public event EventHandler<DialogEventArgs> Dialog_Closed;
        private EditText edit_text_password;

        public static DialogPassword NewInstace(Bundle bundle)
        {
            var fragment = new DialogPassword();
            fragment.Arguments = bundle;
            return fragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.dialogforpassword, container, false);
            edit_text_password = view.FindViewById<EditText>(Resource.Id.edit_textpassword);
            var buttonSubmit = view.FindViewById<Button>(Resource.Id.buttonSubmit);
            var buttonCancel = view.FindViewById<Button>(Resource.Id.buttonCancel);
            var check_box = view.FindViewById<CheckBox>(Resource.Id.checkbox_password);
            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);
            Dialog.SetCanceledOnTouchOutside(false);

            check_box.CheckedChange += delegate
            {
                if (!check_box.Checked)
                    edit_text_password.TransformationMethod = new PasswordTransformationMethod();
                else edit_text_password.TransformationMethod = null;
            };

            buttonSubmit.Click += delegate {
                Dismiss();
            };

            buttonCancel.Click += delegate {
                Dismiss();
            };

            return view;
        }

        public override void OnDismiss(IDialogInterface dialog)
        {
            base.OnDismiss(dialog);
            Dialog_Closed?.Invoke(this, new DialogEventArgs { Password = edit_text_password.Text});
        }
    }
}