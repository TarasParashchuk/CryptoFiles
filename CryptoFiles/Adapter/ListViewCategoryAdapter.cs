using Android.Content;
using Android.Views;
using Android.Widget;
using CryptoFiles.Model;
using System.Collections.Generic;

namespace CryptoFiles
{
    class ListViewCategoryAdapter : BaseAdapter<ModelCategory>
    {
        private Context context;
        private List<ModelCategory> data;
        private ListViewCategoryAdapterViewHolder viewHolder;

        public ListViewCategoryAdapter(Context context, List<ModelCategory> data)
        {
            this.context = context;
            this.data = data;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override ModelCategory this[int position]
        {
            get
            {
                return data[position];
            }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;
            var item = data[position];

            if (view != null)
                viewHolder = view.Tag as ListViewCategoryAdapterViewHolder;
            else
            {
                view = LayoutInflater.From(context).Inflate(Resource.Layout.ItemListCategory, null, false);

                viewHolder = new ListViewCategoryAdapterViewHolder();

                viewHolder.Name_Category = view.FindViewById<TextView>(Resource.Id.Name_Category);
                viewHolder.Count_File_Category = view.FindViewById<TextView>(Resource.Id.Count_Files);

                view.Tag = viewHolder;
            }

            viewHolder.Name_Category.Text = item.Category;
            viewHolder.Count_File_Category.Text = item.Count.ToString();

            return view;
        }

        public override int Count
        {
            get
            {
                return data.Count;
            }
        }

    }

    class ListViewCategoryAdapterViewHolder : Java.Lang.Object
    {
        internal TextView Name_Category;
        internal TextView Count_File_Category;
    }
}