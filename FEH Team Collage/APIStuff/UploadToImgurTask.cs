using System;
using System.Text;

using Android.App;
using Android.OS;
using Android.Widget;
using System.IO;
using Android.Util;
using Java.Net;
using Java.Util;
using Org.Json;
using Plugin.Clipboard;

namespace FEH_Team_Collage.APIStuff
{
	class UploadToImgurTask : AsyncTask
	{
		private Activity mActivity;
		private Android.Net.Uri mImageUri;  // local Uri to upload

		private static String TAG = "UploadToImgurTask";
		private static String UPLOAD_URL = "https://api.imgur.com/3/image";

		private String uploadLink;

		public UploadToImgurTask(Android.Net.Uri imageUri, Activity activity) {
			mImageUri = imageUri;
			mActivity = activity;
		}

		protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params) {
			Stream imageIn;
			try {
				imageIn = mActivity.ContentResolver.OpenInputStream(mImageUri);
			}
			catch (Java.IO.FileNotFoundException e) {
				Log.Error(TAG, "could not open Stream", e);
				return null;
			}
			
			HttpURLConnection conn = null;
			Stream responseIn = null;

			try {
				conn = (HttpURLConnection)new URL(UPLOAD_URL).OpenConnection();
				conn.DoOutput = true;
				ImgurAuthorization.GetInstance().AddToHttpURLConnection(conn);

				Stream outstream = conn.OutputStream;
				Copy(imageIn, outstream); // set image
				outstream.Flush();
				outstream.Close();

				if (conn.ResponseCode == Java.Net.HttpStatus.Ok) {	// execute api call
					responseIn = conn.InputStream;
					return OnInput(responseIn);
				}
				else {
					Log.Info(TAG, "responseCode=" + conn.ResponseCode);
					responseIn = conn.ErrorStream;
					StringBuilder sb = new StringBuilder();
					Scanner scanner = new Scanner(responseIn);
					while (scanner.HasNext) {
						sb.Append(scanner.Next());
					}
					Log.Info(TAG, "error response: " + sb.ToString());
					return null;
				}
			}
			catch (Exception ex) {
				Log.Error(TAG, "Error during POST", ex);
				return null;
			}
			finally {
				try {
					responseIn.Close();
				}
				catch (Exception ignore) { }
				try {
					conn.Disconnect();
				}
				catch (Exception ignore) { }
				try {
					imageIn.Close();
				}
				catch (Exception ignore) { }


				// delete temp file
				ImageAdapter.DeleteTempFile();
			}
		}

		protected override void OnPostExecute(Java.Lang.Object result) {
			base.OnPostExecute(result);
			if(result != null) {
				// success
				Toast.MakeText(MainActivity.GetAppContext(), "File has been uploaded to Imgur!", ToastLength.Short).Show();
				Toast.MakeText(MainActivity.GetAppContext(), "Link copied to clipboard.", ToastLength.Short).Show();
				CrossClipboard.Current.SetText(uploadLink);
			} else {
				// failure
				Toast.MakeText(MainActivity.GetAppContext(), "File upload failed", ToastLength.Short).Show();
			}
			
		}

		private static int Copy(Stream input, Stream output) {

			byte[] buffer = new byte[8192];
			int count = 0;	// the offset, how many bytes we've read so far
			int n = 0;  // how many bytes were read into the buffer (presumably 8192 at a time). Exits when n == -1
			while ((n = input.Read(buffer, 0, buffer.Length)) > 0) {
				output.Write(buffer, 0, n);
				count += n;
			}

			return count;
		}

		protected String OnInput(Stream _in) {
			StringBuilder sb = new StringBuilder();
			Scanner scanner = new Scanner(_in);
			while (scanner.HasNext) {
				sb.Append(scanner.Next());
			}

			JSONObject root = new JSONObject(sb.ToString());
			String id = root.GetJSONObject("data").GetString("id");
			String deletehash = root.GetJSONObject("data").GetString("deletehash");

			uploadLink = "http://imgur.com/" + id;

			Log.Info(TAG, "new imgur url: http://imgur.com/" + id + " (delete hash: " + deletehash + ")");
			return id;
		}
	}
}