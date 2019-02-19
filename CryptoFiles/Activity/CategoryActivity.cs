using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using System.Collections.Generic;
using Android.Support.Design.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using System;
using Android.Content;
using CryptoFiles.Model;
using Android.Views;
using System.Linq;
using Java.Lang.Reflect;
using Field = Java.Lang.Reflect.Field;
using Newtonsoft.Json;

namespace CryptoFiles
{
    [Activity(Label = "CategoryActivity", MainLauncher = false, Theme = "@style/Theme.AppCompat.Light.NoActionBar")]
    public class CategoryActivity : AppCompatActivity
    {
        private List<ModelCategory> list_name_category;
        private ListView List_Category;
        private ListViewCategoryAdapter ListAdapter;
        private ModelCategory item;
        private FloatingActionButton Add_Button;
        private SupportCryptoFunction support_func;
        private TextView info_text;
        private int control_flag;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Категории";

            List_Category = FindViewById<ListView>(Resource.Id.ListFiles);
            Add_Button = FindViewById<FloatingActionButton>(Resource.Id.AddButton);
            info_text = FindViewById<TextView>(Resource.Id.text_informal);

            Add_Button.Click += AddButtonOnClick;
            List_Category.ItemClick += List_Files_ItemClick;
            List_Category.ItemLongClick += List_Category_ItemLongClick;

            support_func = new SupportCryptoFunction(this);
            LoadData();
        }

        private void List_Category_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            item = list_name_category[e.Position];
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

            popupMenu.Inflate(Resource.Layout.menu_itemlistcategoty);
            popupMenu.Show();
            popupMenu.MenuItemClick += PopupMenu_MenuItemClick;
        }

        private void PopupMenu_MenuItemClick(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.menuDeleteCategory:
                    {
                        control_flag = 0;
                        Show_Dialog_Password();
                        break;
                    }
                case Resource.Id.menuEditCategory:
                    {
                        control_flag = 1;
                        Show_Dialog_Password();
                        break;
                    }
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_categoty, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var id = item.ItemId;

            if (id == Resource.Id.menu_name_category)
                list_name_category = list_name_category.OrderByDescending(u => u.Category).ToList();
            else if (id == Resource.Id.menu_count)
                list_name_category = list_name_category.OrderByDescending(u => u.Count).ToList();

            ListAdapter = new ListViewCategoryAdapter(this, list_name_category);
            List_Category.Adapter = ListAdapter;
            Toast.MakeText(this, "Сортировка по: " + item.TitleFormatted, ToastLength.Short).Show();
            return base.OnOptionsItemSelected(item);
        }

        private void LoadData()
        {
            list_name_category = support_func.GetTabelCategory();
            if (list_name_category.Count != 0)
            {
                ListAdapter = new ListViewCategoryAdapter(this, list_name_category);
                List_Category.Adapter = ListAdapter;
                info_text.Text = string.Empty;
            }
            else
            {
                List_Category.SetAdapter(null);
                info_text.Text = "Создайте новую категорию";
            }
        }

        private void List_Files_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            item = list_name_category[e.Position];
            control_flag = 2;
            Show_Dialog_Password();
        }

        private void AddButtonOnClick(object sender, EventArgs eventArgs)
        {
            var intent = new Intent(this, typeof(RegistrationEditActivity));
            intent.PutExtra("about_category", string.Empty);
            StartActivity(intent);
        }

        private void DialogFragment_DialogClosed(object sender, DialogEventArgs e)
        {
            var current_password_SHA = EncryptionDecryptionService.GetSHA(e.Password, false);

            if (item.Password != current_password_SHA)
            {
                Toast.MakeText(this, "Неверный пароль", ToastLength.Long).Show();
                return;
            }
            else
            {
                switch(control_flag)
                {
                    case 0:
                        {
                            support_func.DataBaseConnection().DeleteItem<ModelCategory>(item.id);
                            LoadData();
                            break;
                        }
                    case 1:
                        {
                            var intent = new Intent(this, typeof(RegistrationEditActivity));
                            intent.PutExtra("about_category", JsonConvert.SerializeObject(item));
                            StartActivity(intent);
                            break;
                        }
                    case 2:
                        {
                            var intent = new Intent(this, typeof(MainActivity));
                            intent.PutExtra("id", item.id);
                            intent.PutExtra("key", e.Password);
                            StartActivity(intent);
                            break;
                        }
                }
            }
        }

        private void Show_Dialog_Password()
        {
            Android.Support.V4.App.FragmentTransaction ft = SupportFragmentManager.BeginTransaction();
            var dialogFragment = DialogPassword.NewInstace(null);
            dialogFragment.Dialog_Closed += DialogFragment_DialogClosed;
            ft.Add(dialogFragment, "dialog_Password");
            ft.CommitAllowingStateLoss();
        }

        public override void OnBackPressed()
        {

        }
    }
}