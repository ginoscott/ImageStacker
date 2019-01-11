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
using Android.Graphics;
using System.IO;
using Android.Media;

namespace FEH_Team_Collage
{
	[Activity(Label = "Final Render View")]
	class FinalRenderViewActivity : Activity
	{
		private ImageView imgViewFinalRender;
		private Bitmap bitmap;
		private Java.IO.File tempFile;
		private String outFileName;
		private Boolean loginSuccess = false;


		static int IMGUR_LOGIN_REQUEST = 30;

		Button btnUploadToImgur;

		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.finalRenderView_Layout);
			
			// grab values from bundle
			outFileName = Intent.GetStringExtra("outFileName");

			// get saved image from temp file
			tempFile = ImageAdapter.GetTempFile();
			bitmap = BitmapFactory.DecodeFile(tempFile.Path);

			// create references to the ui elements
			imgViewFinalRender = FindViewById<ImageView>(Resource.Id.imgViewFinalRender);
			Button btnSave = FindViewById<Button>(Resource.Id.btnSave);
			btnUploadToImgur = FindViewById<Button>(Resource.Id.btnUploadToImgur);
			if(!APIStuff.ImgurAuthorization.GetInstance().IsLoggedIn()) {
				btnUploadToImgur.Text = "Log in";
			} else {
				btnUploadToImgur.Text = "Upload to Imgur";
			}

			// set the image
			imgViewFinalRender.SetImageBitmap(bitmap);


			// create button listeners
			btnSave.Click += (sender, e) =>					// save
			{
				btnSave.Enabled = false;
				SaveImageLocally();
			};
			btnUploadToImgur.Click += (IntentSender, e) =>  // upload to Imgur
			{
				btnUploadToImgur.Enabled = false;
				UploadToImgur();
			};
		}

		private void SaveImageLocally() {
			// figure out the filepath and filename to save to
			String saveFileName = "FEHTeam" + DateTime.Now.ToFileTime() + ".png";
			// check if user has a filename specified
			if (outFileName.Trim().Length > 0) {
				saveFileName = outFileName + ".png";
			}

			// save the result image
			var sdCardPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath;
			var filePath = System.IO.Path.Combine(sdCardPath, saveFileName);
			var stream = new FileStream(filePath, FileMode.Create);
			bitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
			stream.Close();
			bitmap.Recycle();

			// call the Media Scanner to let the gallery (and everything that uses it)
			// know that there is a new picture we saved and makes it visible to them
			MediaScannerConnection.ScanFile(MainActivity.GetAppContext(), new String[] { sdCardPath + "/" + saveFileName }, null, null);

			// notify the user that the file is saved
			Toast.MakeText(MainActivity.GetAppContext(), "File has been saved!", ToastLength.Short).Show();
		}

		private void UploadToImgur() {
			// check if logged in to Imgur, log in before executing upload task
			Boolean loggedIn = APIStuff.ImgurAuthorization.GetInstance().IsLoggedIn();
			if (!loggedIn) {
				//StartActivity(new Intent(this, typeof(APIStuff.ImgurLoginActivity)));
				StartActivityForResult(new Intent(this, typeof(APIStuff.ImgurLoginActivity)), IMGUR_LOGIN_REQUEST);
			}
			// proceed with upload if login was a success or we are already logged in
			if(loginSuccess || loggedIn) {
				Android.Net.Uri imageUri = Android.Net.Uri.Parse(tempFile.ToURI().ToString());
				new APIStuff.UploadToImgurTask(imageUri, this).Execute();
			} else {
				// enable button again to try again
				btnUploadToImgur.Enabled = true;
			}
		}

		protected override void OnDestroy() {
			base.OnDestroy();
			bitmap.Recycle();
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
			base.OnActivityResult(requestCode, resultCode, data);
			if (resultCode == Result.Ok) {
				if (requestCode == IMGUR_LOGIN_REQUEST) {
					// result from the ImgurLoginActivity
					loginSuccess = true;
					btnUploadToImgur.Text = "Upload to Imgur";
				}
			}
		}

	}
}