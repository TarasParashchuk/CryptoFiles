using Android.Content;
using Android.Views;
using Android.Widget;
using CryptoFiles.Model;
using System.Collections.Generic;

namespace CryptoFiles
{
    class ListViewAdapter : BaseAdapter<ModelDataFile>
    {
        private Context context;
        private List<ModelDataFile> data;
        private ListViewAdapterViewHolder viewHolder;

        public ListViewAdapter(Context context, List<ModelDataFile> data)
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

        public override ModelDataFile this[int position]
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
                viewHolder = view.Tag as ListViewAdapterViewHolder;
            else
            {
                view = LayoutInflater.From(context).Inflate(Resource.Layout.ItemList, null, false);

                viewHolder = new ListViewAdapterViewHolder();

                viewHolder.Name_File = view.FindViewById<TextView>(Resource.Id.Name_file);
                viewHolder.Size_File = view.FindViewById<TextView>(Resource.Id.Size_file);
                viewHolder.Type_File = view.FindViewById<TextView>(Resource.Id.Type_file);
                viewHolder.Data_File = view.FindViewById<TextView>(Resource.Id.Data_file);

                view.Tag = viewHolder;
            }

            viewHolder.Name_File.Text = item.FileName;
            viewHolder.Type_File.Text = item.FileType;
            viewHolder.Size_File.Text = EncryptionDecryptionService.ConvertToSizeWithName(item.FileSize);
            viewHolder.Data_File.Text = item.FileDate;

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

    class ListViewAdapterViewHolder : Java.Lang.Object
    {
        internal TextView Name_File;
        internal TextView Size_File;
        internal TextView Type_File;
        internal TextView Data_File;
    }
}