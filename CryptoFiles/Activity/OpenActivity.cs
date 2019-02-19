using Android.App;
using Android.Content.PM;
using Android.OS;

namespace CryptoFiles
{
    [Activity(Theme = "@style/Theme.Splash",
        MainLauncher = true,
        ScreenOrientation = ScreenOrientation.Portrait,
        NoHistory = false)]
    public class OpenActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            StartActivity(typeof(CategoryActivity));
        }
    }
}