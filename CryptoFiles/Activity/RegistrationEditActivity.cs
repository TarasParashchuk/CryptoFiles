using Android.App;
using Android.OS;
using Android.Widget;
using CryptoFiles.Model;
using Newtonsoft.Json;
using System;

namespace CryptoFiles
{
    [Activity(Label = "RegistrationEditActivity", MainLauncher = false)]
    public class RegistrationEditActivity : Activity
    {
        private EditText category_text;
        private EditText password_text;
        private Button button;
        private ModelCategory information_category;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_login);

            var about_category = Intent.GetStringExtra("about_category");

            category_text = FindViewById<EditText>(Resource.Id.input_login);
            password_text = FindViewById<EditText>(Resource.Id.input_password);
            button = FindViewById<Button>(Resource.Id.btn_login);
            button.Click += Button_Click;

            if (about_category != string.Empty)
            {
                information_category = JsonConvert.DeserializeObject<ModelCategory>(about_category);
                category_text.Text = information_category.Category;
                button.Text = "Сохранить категорию";
            }
            else information_category = null;
        }

        private void Button_Click(object sender, EventArgs e)
        {
            var conn = new DataBase().SQLiteConnection();
            var db = new DataBaseHelp(conn);
            var toast_mess = string.Empty;

            if (validate())
            {
                var category = category_text.Text;
                var password = password_text.Text;

                if(information_category == null)
                {
                    var list = db.GetItems<ModelCategory>();
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].Category == category)
                        {
                            Toast.MakeText(this, "Категория с таким именем уже зарегистрирована", ToastLength.Short).Show();
                            return;
                        }
                    }

                    information_category = new ModelCategory();
                    information_category.Category = category;
                    information_category.Password = EncryptionDecryptionService.GetSHA(password, false);
                    information_category.Count = 0;
                    toast_mess = "Категория успешно создана";
                    db.SaveItem(information_category, false);
                }
                else
                {
                    information_category.Category = category;
                    information_category.Password = EncryptionDecryptionService.GetSHA(password, false);
                    information_category.Count = information_category.Count;
                    toast_mess = "Категория обновлена";
                    db.SaveItem(information_category, true);
                }

                Toast.MakeText(this, toast_mess, ToastLength.Short).Show();
                StartActivity(typeof(CategoryActivity));
            }  
            else Toast.MakeText(this, "Поля ввода не пройшли валидацию", ToastLength.Short).Show();
        }

        public bool validate()
        {
            var valid = true;
            var login = category_text.Text;
            var password = password_text.Text;

            if (string.IsNullOrEmpty(login))
            {
                category_text.SetError("Поле имени категории не должно быть пустым", null);
                valid = false;
            }

            if (string.IsNullOrEmpty(password) || password.Length < 4 || password.Length > 20)
            {
                password_text.SetError("Количество буквенно-цифровых символов от 4 до 20", null);
                valid = false;
            }

            return valid;
        }
    }
}