using System;
using System.Collections.Generic;

using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Square.Picasso;
using System.IO;
using Android.Media;
using Plugin.CurrentActivity;
using Android.Util;

namespace FEH_Team_Collage
{
	public class ImageAdapter : BaseAdapter
	{
		private readonly Context context;
		private List<String> _gridViewString;
		private List<String> _gridViewImage;
		private List<int> _dims;

		private static Java.IO.File tempFile;
		private static String TAG = "ImageAdapter";

		public ImageAdapter(Context c)
		{
			context = c;
			_gridViewString = new List<string>();
			_gridViewImage = new List<String>();
			ResetGridToDefault();
		}

		public ImageAdapter(Context c, List<string> gVString, List<String> gVImg)
		{
			context = c;
			_gridViewString = gVString;
			_gridViewImage = gVImg;
		}

		public override int Count
		{
			get { return _gridViewString.Count; }
		}

		public override Java.Lang.Object GetItem(int position)
		{
			return null;
		}

		public override long GetItemId(int position)
		{
			return 0;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			View view = convertView;
			GridItemViewHolder holder = null;

			if (view != null)
				holder = view.Tag as GridItemViewHolder;

			if(holder == null)
			{
				holder = new GridItemViewHolder();
				LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
				view = inflater.Inflate(Resource.Layout.gridView_Layout, null);
				holder.Image = view.FindViewById<ImageView>(Resource.Id.imgView);
				holder.Desc = view.FindViewById<TextView>(Resource.Id.textView);
				view.Tag = holder;
			}
			
			holder.Desc.Text = _gridViewString[position];
			if (_gridViewImage[position] == null)
			{
				holder.Image.SetImageResource(Resource.Drawable.addCircle);
			} else
			{
				// potential Out of Memory error here, because every time the view is refreshed,
				// the image is reloaded from memory, which can cause problems if the size if big,
				// since the old one won't be collected in time by the gc, potentially (I think)
				//
				// note that I'm not actually using this line and its two functions
				// but I'm keeping it here because it was a pain in the to code and
				// it may come in handy at some future point in time maybe idk
				//holder.Image.SetImageBitmap(decodeSampledBitmapFromFilePath(_gridViewImage[position], 110, 110));


				Picasso.With(context)
					.Load("file://" + _gridViewImage[position])
					.Transform(new CropPortraitTransformation())
					.Into(holder.Image);
			}
			return view;
		}

		// Add a new item to the grid
		public void AddNewItem()
		{
			if(_gridViewString.Count < 12)
			{
				int newItemNum = _gridViewString.Count + 1;
				_gridViewString.Add(newItemNum.ToString());
				_gridViewImage.Add(null);
				NotifyDataSetChanged();
			} else
			{
				Toast.MakeText(context, "Max limit 12 images reached", ToastLength.Short).Show();
			}
		}

		// Remove an item off the end of list
		public void RemoveItem()
		{
			if(_gridViewString.Count > 0)
			{
				_gridViewString.RemoveAt(_gridViewString.Count-1);
				_gridViewImage.RemoveAt(_gridViewImage.Count-1);
				NotifyDataSetChanged();
			}
		}

		// Set the image of an item
		public void SetImage(String imgPath, int pos)
		{
			// if an image is selected that is not stored locally (eg: such as one on Google Drive),
			//	then the string will return a null, so return in that case.
			if (String.IsNullOrEmpty(imgPath))
				return;
			_gridViewImage[pos] = imgPath;
			NotifyDataSetChanged();
		}

		// check if at least one image is loaded in a slot
		public bool GridViewNotEmpty()
		{
			foreach(String imgPath in _gridViewImage)
			{
				// return true if at least one imgPath exists
				if (imgPath != null)
				{
					return true;
				}
			}
			return false;
		}

		public String GetFirstNonEmptyImgPath()
		{
			foreach(String path in _gridViewImage)
			{
				if(path != null)
				{
					return path;
				}
			}
			return null;
		}

		private void ResetGridToDefault()
		{
			_gridViewImage.Clear();
			_gridViewString.Clear();
			AddNewItem(); AddNewItem();
			AddNewItem(); AddNewItem();
		}

		public String GetGridViewImage(int pos)
		{
			if (_gridViewImage.Count > pos)
				return _gridViewImage[pos];
			else
				return null;
		}

		// pos 0: width, pos 1: height
		public List<int> GetImgDims()
		{
			// if it's already been defined, then return the existing values
			if(_dims == null) {
				_dims = new List<int>();

				// acquire image dimensions
				BitmapFactory.Options options = new BitmapFactory.Options();
				String filePath = "";
				foreach (String imgPath in _gridViewImage)
				{
					if(imgPath != null)
					{
						filePath = imgPath;
						break;
					}
				}
				if(filePath == "")
					return null;
				BitmapFactory.DecodeFile(filePath, options);
				_dims.Add(options.OutWidth); // width
				_dims.Add(options.OutHeight); // height
			}
			return _dims;
		}

		public void StackMultipleImages(int upperBound, int lowerBound)
		{
			// first acquire the indices of the imagelist that are
			// actually populated with something, ie: the user chose
			// an image for that slot
			List<int> gridIndices = new List<int>();
			for (int i = 0; i < _gridViewImage.Count; i++)
			{
				// check if user loaded an image into this slot
				if(_gridViewImage[i] != null)
				{
					gridIndices.Add(i);
				}
			}
			
			// build the result canvas from the first image
			Bitmap firstImg = CropToCharacterStatus(_gridViewImage[gridIndices[0]], upperBound, lowerBound);
			int imgHeight = firstImg.Height;
			Bitmap result = Bitmap.CreateBitmap(
				firstImg.Width,
				// multiply height by the total number of images
				// that we will be stacking
				imgHeight * gridIndices.Count,
				firstImg.GetConfig());
			Canvas canvas = new Canvas(result);
			// add the first image to the result canvas
			canvas.DrawBitmap(firstImg, 0, 0, null);
			firstImg.Recycle();

			// add the rest of the images to the result canvas
			if(gridIndices.Count > 1)
			{
				for(int i = 1; i < gridIndices.Count; i++)
				{
					Bitmap img = CropToCharacterStatus(_gridViewImage[gridIndices[i]], upperBound, lowerBound);
					canvas.DrawBitmap(img, 0, imgHeight * i, null);
					img.Recycle();
				}
			}


			// save the results of the operations to a temp file
			tempFile = Java.IO.File.CreateTempFile("temp_result_pic", ".tmp");
			var stream = new FileStream(tempFile.Path, FileMode.Open);
			result.Compress(Bitmap.CompressFormat.Png, 100, stream);
			stream.Close();
			result.Recycle();

			// reset the grid
			ResetGridToDefault();
		}

		private Bitmap CropToCharacterStatus(String filePath, int upperBound, int lowerBound)
		{
			BitmapRegionDecoder bitmapRegionDecoder = BitmapRegionDecoder.NewInstance(filePath, false);
			BitmapFactory.Options options = new BitmapFactory.Options();
			options.InJustDecodeBounds = true;
			BitmapFactory.DecodeFile(filePath, options);
			// setting up a cropping rect to grab only the top character info banner of the screenshot
			int width = options.OutWidth;
			int height = lowerBound+1;
			Rect cropRect = new Rect(0, upperBound, width, height);

			// Decode bitmap with cropping rect set
			options.InJustDecodeBounds = false;
			return bitmapRegionDecoder.DecodeRegion(cropRect, options);
		}

		public static Java.IO.File GetTempFile() {
			return tempFile;
		}
		
		public static void DeleteTempFile() {
			try {
				tempFile.Delete();
				tempFile = null;
			}
			catch(NullReferenceException e) {
				Log.Error(TAG, "No temp file to delete.", e);
			}
		}
	}
}