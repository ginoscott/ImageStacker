using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Graphics;
using Android.Views;

namespace FEH_Team_Collage
{
	[Activity(Label = "Adjust Crop Margins")]
	public class AdjustCropRectActivity : Activity
	{
		private ImageView imgViewAdjust;
		NumberPicker numpickUpper;
		NumberPicker numpickLower;

		public FloatingWindowService fwService { get; set; }
		public bool isBound { get; set; } = false;
		private FloatingWindowServiceConnection _fwServiceConnection;
		private int[] _imgPos = new int[2];
		private int _upperValue, _lowerValue, _x, _y, _width, _height;
		private int _statusBarHeight = 0;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.adjustCropRect_Layout);

			// grab the values from the bundle
			System.String imgPath = Intent.GetStringExtra("imgPath");
			int[] dims = Intent.GetIntArrayExtra("dims");
			_upperValue = Intent.GetIntExtra("upperValue", 0);
			_lowerValue = Intent.GetIntExtra("lowerValue", 0);

			// create references to the ui elements
			numpickUpper = FindViewById<NumberPicker>(Resource.Id.numpickUpper);
			numpickLower = FindViewById<NumberPicker>(Resource.Id.numpickLower);
			imgViewAdjust = FindViewById<ImageView>(Resource.Id.imgViewAdjust);
			Button btnSetBounds = FindViewById<Button>(Resource.Id.btnSetBounds);
			Button btnFinishAdjust = FindViewById<Button>(Resource.Id.btnFinishAdjust);

			// set the image
			imgViewAdjust.SetImageBitmap(CropAdjustPreview(imgPath));
			
			// set the numberpicker min/max values according to the dimension of the image we have
			numpickUpper.WrapSelectorWheel = false;
			numpickLower.WrapSelectorWheel = false;
			numpickUpper.MinValue = 0;
			numpickUpper.MaxValue = _height;
			numpickLower.MinValue = 0;
			numpickLower.MaxValue = _height;

			// set the numberpicker default value, which is our best approximation by the ratio we computed
			if(_upperValue == 0 && _lowerValue == 0)
			{
				// if it's not been adjusted in this activity yet, then set
				// it to default values using the ratio
				_upperValue = 0;
				_lowerValue = dims[1] * 348 / 2208;
			}
			numpickUpper.Value = _upperValue;
			numpickLower.Value = _lowerValue;

			// Create the Crop Rect Preview
			Intent fwServiceIntent = new Intent(this, typeof(FloatingWindowService));

			_fwServiceConnection = new FloatingWindowServiceConnection(this);
			BindService(fwServiceIntent, _fwServiceConnection, Bind.AutoCreate);

			// grab the imgViewAdjust info (x, y, width) after it's been drawn
			imgViewAdjust.GetImgViewInfo((valDict) =>
			{
				// update our member variables
				_x = valDict["x"];
				_y = valDict["y"] - GetStatusBarHeight();
				_width = valDict["width"];

				if (isBound)
				{
					// update the position of the floating window
					fwService.UpdatePosition(
						_x,
						_y + _upperValue,
						_lowerValue - _upperValue,
						_width);
				}
			});

			// add a listener for the buttons
			btnSetBounds.Click += (sender, e) => {
				if(isBound)
				{
					// make sure upper < lower
					if(numpickUpper.Value > numpickLower.Value)
					{
						int temp = numpickUpper.Value;
						numpickUpper.Value = numpickLower.Value;
						numpickLower.Value = temp;
					}

					// update the position of the floating window
					fwService.UpdatePosition(
						_x,
						_y + numpickUpper.Value,
						numpickLower.Value - numpickUpper.Value,
						_width);
				}
			};
			btnFinishAdjust.Click += (sender, e) => {
				// update our member variables and make sure upper < lower
				_upperValue = numpickUpper.Value;
				_lowerValue = numpickLower.Value;
				if (_upperValue > _lowerValue)
				{
					numpickUpper.Value = _lowerValue;
					numpickLower.Value = _upperValue;
					_upperValue = numpickUpper.Value;
					_lowerValue = numpickLower.Value;
				}

				// create an intent to return our results to the main activity
				Intent returnIntent = new Intent();
				returnIntent.PutExtra("upperValue", _upperValue);
				returnIntent.PutExtra("lowerValue", _lowerValue);
				SetResult(Result.Ok, returnIntent);
				Finish();
			};
		}

		private int GetStatusBarHeight()
		{
			if(_statusBarHeight == 0)
			{
				int resourceId = Resources.GetIdentifier("status_bar_height", "dimen", "android");
				if (resourceId > 0)
				{
					_statusBarHeight = Resources.GetDimensionPixelSize(resourceId);
					return _statusBarHeight;
				}
			}
			return _statusBarHeight;
		}
		

		protected override void OnStop()
		{
			// recycle the preview image
			base.OnStop();
			if (imgViewAdjust != null)
				imgViewAdjust.SetImageBitmap(null);
			// unbind the service
			if (isBound)
			{
				UnbindService(_fwServiceConnection);
				isBound = false;
			}

			// since saving the state is annoying, just close this activity and
			// return the user back to the Main activity
			Finish();
		}

		private Bitmap CropAdjustPreview(System.String filePath)
		{
			BitmapRegionDecoder bitmapRegionDecoder = BitmapRegionDecoder.NewInstance(filePath, false);
			BitmapFactory.Options options = new BitmapFactory.Options();
			options.InJustDecodeBounds = true;
			BitmapFactory.DecodeFile(filePath, options);
			// setting up a cropping rect to grab only the top character info banner of the screenshot
			_width = options.OutWidth / 4;
			_height = options.OutHeight / 2;
			Rect cropRect = new Rect(0, 0, _width, _height);

			// Decode bitmap with cropping rect set
			options.InJustDecodeBounds = false;
			return bitmapRegionDecoder.DecodeRegion(cropRect, options);
		}
	}
}