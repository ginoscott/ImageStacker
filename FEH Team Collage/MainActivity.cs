using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using System;
using System.Collections.Generic;
using Com.Cloudrail.SI;
using Com.Cloudrail.SI.Services;
using Com.Cloudrail.SI.Interfaces;
using System.IO;
using System.Threading;
using Android.Graphics;
using Android.Content.PM;
using System.Threading.Tasks;
using Plugin.CurrentActivity;

namespace FEH_Team_Collage
{
	[Activity(Label = "FEH Sen Proj", MainLauncher = true)]
	public class MainActivity : Activity
	{
		private static Context context;

		private GridView gridView;
		private ImageAdapter adapter;

		private int _upperBound = 0;
		private int _lowerBound = 0;


		static int ADJUST_CROP_MARGINS_REQUEST = 29;

		private static String BROWSABLE = "android.intent.category.BROWSABLE";

		public static Context GetAppContext() {
			return MainActivity.context;
		}

		protected override void OnNewIntent(Intent intent) {
			if (intent.Categories.Contains(BROWSABLE)) {
				// Here we pass the response to the SDK which will automatically
				// complete the authentication process
				CloudRail.AuthenticationResponse = intent;
			}
			base.OnNewIntent(intent);
		}
		async void PerformBackgroundOp(GoogleDrive gd) {
			await Task.Run(() => gd.Login());
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			MainActivity.context = Android.App.Application.Context;
			CrossCurrentActivity.Current.Init(this, savedInstanceState);

			// Initialize the SDK
			CloudRail.AppKey = "5aeb7a5d53d06b4cef821f14";
			// Inizialize the Google Drive service.
			GoogleDrive gdrive = new GoogleDrive(
				this, 
				"870232221465-mbejvcpr04tl0e51885vo37g2hlqgu6k.apps.googleusercontent.com", 
				"",
				//"com.googleusercontent.apps.870232221465-mbejvcpr04tl0e51885vo37g2hlqgu6k:/oauth2redirect",
				"com.chkansaku.fehsenproj:/oauth2redirect",
				"state"
			);
			// Now we enable the use of the advanced authentication, meaning the one that
			// does not use WebViews.
			//gdrive.UseAdvancedAuthentication();
			// We're logging in the user. This will take them to an on-device browser. After finishing
			// the login process, the website will send them back to our app.
			//gdrive.Login();
			ICloudStorage service;
			service = gdrive;
			Stream result;
			gdrive.UseAdvancedAuthentication();
			void backfunc() {
				//result = service.Download("/Pictures/FEH/test.png");
				//String temp = gdrive.UserLogin;
				gdrive.Login();
				/*var sdCardPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath;
				var filePath = System.IO.Path.Combine(sdCardPath, "test");
				var stream = new FileStream(filePath, FileMode.Create);
				Bitmap bp = BitmapFactory.DecodeStream(result);
				bp.Compress(Bitmap.CompressFormat.Png, 100, stream);
				stream.Close();
				bp.Recycle();
				Android.Media.MediaScannerConnection.ScanFile(this, new String[] { sdCardPath + "/" + "test" }, null, null);
				*/
				//Toast.MakeText(this, "calling backfunc", ToastLength.Short).Show();
			};
			//PerformBackgroundOp(gdrive);

			//ThreadPool.QueueUserWorkItem(o => backfunc());
			//Stream result = service.Download("/Pictures/FEH/test.png");
			// handle exceptions created when service is cancelled

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			// Get UI stuff from layout
			Button btnCreate = FindViewById<Button>(Resource.Id.btnCreate);
			Button btnAdd = FindViewById<Button>(Resource.Id.btnAdd);
			Button btnDelete = FindViewById<Button>(Resource.Id.btnDelete);
			Button btnAdjust = FindViewById<Button>(Resource.Id.btnAdjustCropRect);
			EditText editText = FindViewById<EditText>(Resource.Id.fileName);
			gridView = FindViewById<GridView>(Resource.Id.gridView);
			adapter = new ImageAdapter(this);
			gridView.Adapter = adapter;

			// listen for gridview item clicks
			gridView.ItemClick += (sender, e) =>
			{
				var imageIntent = new Intent();
				imageIntent.SetType("image/*");
				imageIntent.SetAction(Intent.ActionGetContent);
				StartActivityForResult(
					Intent.CreateChooser(imageIntent, "Select photo"),e.Position);
			};

			// create button listeners
			btnAdd.Click += (sender, e) =>		// +
			{
				adapter.AddNewItem();
			};
			btnDelete.Click += (sender, e) =>	// -
			{
				adapter.RemoveItem();
			};
			btnCreate.Click += (sender, e) =>	// Create collage
			{
				if(adapter.GridViewNotEmpty())
				{
					if (_upperBound == 0 && _lowerBound == 0)
					{
						// if it's not been adjusted yet, then set
						// it to default values using the ratio
						int[] dims = adapter.GetImgDims().ToArray();
						_upperBound = 0;
						_lowerBound = dims[1] * 348 / 2208;
					}
					// call the function to create the stacked image
					adapter.StackMultipleImages(_upperBound, _lowerBound);

					// launch a new Final Render View Activity
					var intent = new Intent();
					intent.SetClass(MainActivity.GetAppContext(), typeof(FinalRenderViewActivity));
					intent.PutExtra("outFileName", editText.Text.ToString());
					StartActivity(intent);

					// reset the values back to initial
					editText.Text = null;
					_upperBound = 0;
					_lowerBound = 0;
				} else
				{
					Toast.MakeText(this, "You must choose at least 1 image!", ToastLength.Short).Show();
				}
				
			};
			btnAdjust.Click += (sender, e) => {
				// launch a new Adjust Crop Rect Activity
				if (adapter.GridViewNotEmpty())
				{
					var intent = new Intent();
					intent.SetClass(this, typeof(AdjustCropRectActivity));
					intent.PutExtra("imgPath", adapter.GetFirstNonEmptyImgPath());
					intent.PutExtra("dims", adapter.GetImgDims().ToArray());
					intent.PutExtra("upperValue", _upperBound);
					intent.PutExtra("lowerValue", _lowerBound);
					StartActivityForResult(intent, ADJUST_CROP_MARGINS_REQUEST);
				}
				else
				{
					Toast.MakeText(this, "Please load an image", ToastLength.Short).Show();
				}
			};
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			if (resultCode == Result.Ok)
			{
				if(requestCode == ADJUST_CROP_MARGINS_REQUEST)
				{
					// result from the AdjustCropRectActivity activity
					_upperBound = data.GetIntExtra("upperValue", 0);
					_lowerBound = data.GetIntExtra("lowerValue", 0);
				} else
				{
					// result from picking an image from the gallery
					// I've arbitrarily set the max limit of img slots to 12,
					// so the requestCode should never go over 11
					String imgPath = GetPathToImage(data.Data);
					adapter.SetImage(imgPath, requestCode);
				}
				
			}
		}

		private string GetPathToImage(Android.Net.Uri uri)
		{
			string doc_id = "";
			using (var c1 = ContentResolver.Query(uri, null, null, null, null))
			{
				c1.MoveToFirst();
				String document_id = c1.GetString(0);
				doc_id = document_id.Substring(document_id.LastIndexOf(":") + 1);
			}

			string path = null;

			// The projection contains the columns we want to return in our query.
			string selection = Android.Provider.MediaStore.Images.Media.InterfaceConsts.Id + " =? ";
			using (var cursor = ContentResolver.Query(Android.Provider.MediaStore.Images.Media.ExternalContentUri, null, selection, new string[] { doc_id }, null))
			{
				if (cursor == null) return path;
				var columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
				cursor.MoveToFirst();
				try {
					path = cursor.GetString(columnIndex);
				} catch (Android.Database.CursorIndexOutOfBoundsException) {
					path = null;
					Toast.MakeText(this, "Please select an image stored locally.", ToastLength.Short).Show();
				}
				
			}
			return path;
		}
		
		protected override void OnPause() {
			base.OnPause();
			if (IsFinishing) {
				// Here  you can be sure the Activity will be destroyed eventually
				ImageAdapter.DeleteTempFile();
				// this essentially "logs out" the user. remove for launch
				APIStuff.ImgurAuthorization.GetInstance().ResetTokens();
			}
		}
	}
}

