using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Android.Graphics;

namespace FEH_Team_Collage
{
	public class CropPortraitTransformation : Java.Lang.Object, ITransformation
	{
		public Bitmap Transform(Bitmap source)
		{
			int size = Math.Min(source.Width, source.Height);
			int width = source.Width * 270 / 1242;
			int height = source.Height * 242 / 2208;
			Bitmap result = Bitmap.CreateBitmap(source, 0, 0, width, height);
			if (result != source)
			{
				source.Recycle();
			}
			return result;
		}

		public string Key
		{
			get { return "square()"; }
		}
	}
}