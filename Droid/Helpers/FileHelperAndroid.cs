using MyHealthVitals.Droid;
using System;
using System.IO;
using OxyPlot;

using System.Net;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using MimeKit;
using MailKit;
using MailKit.Net.Smtp;
using Android.Widget;

using Xamarin.Forms;
//using ToastIOS;
//using Foundation;
//using UIKit;
//using MessageUI;
//using SystemConfiguration;
//using CoreFoundation;

using System.Collections.Generic;

using System.Diagnostics.Contracts;

using System.Runtime.Serialization.Formatters.Binary;

using System.Text;

using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;

//using Android.OS;
//using Android.App;
using Android.Content;
//using Xamarin.Forms;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
//using Android.OS;
using Android.Support.V7.AppCompat;
using Android.Text;
using nexus.core.logging;
using nexus.protocols.ble;
using Android.Util;

[assembly: Xamarin.Forms.Dependency(typeof(BaseUrl_Android))]

[assembly: Xamarin.Forms.Dependency(typeof(FileHelperAndroid))]

namespace MyHealthVitals.Droid
{
	public class FileHelperAndroid : IFileHelper
	{
        String Patient;
		String DOB;
		String Finding;
		String Recorded;
		String FindingDetails;
		String TestDuration;
		String HeartRate;


		String filePath = null;
		String filePathNEW = null;
		string name;
		static MimeMessage message = null;// = new MimeMessage();
		MailKit.Net.Smtp.SmtpClient client = null;// = new MailKit.Net.Smtp.SmtpClient();
												  //        client.Connect("smtp.gmail.com", 587, false);

		//  string emaiAddress,   

	
        public async Task<bool> dispAlert(String title, String message, bool tablet, String btn1, String btn2)
        {
            //Debug.WriteLine("made it to dispAlert()");
            bool val = false;
            Android.App.AlertDialog.Builder dialog = new AlertDialog.Builder(Xamarin.Forms.Forms.Context as Activity);
            //Debug.WriteLine("Initialized the dialog");
            AlertDialog alert = dialog.Create();

			//alert.SetTitle(title);
			if (tablet)
			{
				//message.Replace("\n", "<br/>");
				//Debug.WriteLine("message = "+message);
				if (Convert.ToInt32(Android.OS.Build.VERSION.Sdk) >= 24)
				{
					alert.SetTitle(Html.FromHtml("<big><big><big>" + title + "</big></big></big>", 0));
				}
				else
				{
					alert.SetTitle(Html.FromHtml("<big><big><big>" + title + "</big></big></big>"));
				}
			}
			else
			{
				alert.SetTitle(title);
			}
            //Debug.WriteLine("set the title");

            //Debug.WriteLine("Android Build # = " + Android.OS.Build.VERSION.Sdk);

            //need to check for Android version # and if 24 or higher add ,0
            if (tablet)
            {
                message.Replace("\n","<br/>");
                //Debug.WriteLine("message = "+message);
				if (Convert.ToInt32(Android.OS.Build.VERSION.Sdk) >= 24)
				{
					alert.SetMessage(Html.FromHtml("<big><big>" + message + "</big></big>", 0));
				}
				else
				{
					alert.SetMessage(Html.FromHtml("<big><big>" + message + "</big></big>"));
				}
            }else{
                alert.SetMessage(message);
            }
            //Debug.WriteLine("set the message");

            //Debug.WriteLine("made it to Task.Run()");
            await Task.Run(() =>
			{
				var waitHandle = new AutoResetEvent(false);

                if (tablet)
                {
                    //Debug.WriteLine("set tablet buttons");
                    if (Convert.ToInt32(Android.OS.Build.VERSION.Sdk) >= 24)
                    {
                        alert.SetButton((int)(DialogButtonType.Positive), Html.FromHtml("<big><big>" + btn1 + "</big></big>",0), (sender, e) =>
						{
							val = true;
							waitHandle.Set();
						});

						if (btn2 != null)
						{
							alert.SetButton((int)DialogButtonType.Negative, Html.FromHtml("<big><big>" + btn2 + "</big></big>",0), (sender, e) =>
							{
								val = false;
								waitHandle.Set();
							});							
						}
                    }else{

						alert.SetButton((int)(DialogButtonType.Positive), Html.FromHtml("<big><big>" + btn1 + "</big></big>"), (sender, e) =>
						{
							val = true;
							waitHandle.Set();
						});
						if (btn2 != null)
						{
							//Debug.WriteLine("message = <big><big>" + btn2 + "</big></big>");
							alert.SetButton((int)DialogButtonType.Negative, Html.FromHtml("<big><big>" + btn2 + "</big></big>"), (sender, e) =>
							{
							  val = false;
							  waitHandle.Set();
							});							
						}

						
                    }
                }else{
                    //Debug.WriteLine("set phone buttons");
					alert.SetButton((int)(DialogButtonType.Positive), btn1, (sender, e) =>
					{
						val = true;
						waitHandle.Set();
					});

					if (btn2 != null)
					{
						alert.SetButton((int)DialogButtonType.Negative, btn2, (sender, e) =>
						{
							val = false;
							waitHandle.Set();
						});
					}
                }
                //Debug.WriteLine("made it to alert.show()");
				Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
				{
                    //alert.Window.SetLayout(1200, 1200);
					alert.Show();
					
                }));
				waitHandle.WaitOne();
			});
            //Debug.WriteLine("made it to alert.dispose()");
			alert.Dispose();

            //});
            //Debug.WriteLine("made it to return");
            return val;
        }

        public void setEcgInof(String Patient, String DOB, String Finding, String Recorded,
							   String FindingDetails, String HeartRate, String TestDuration = "30s")
		{
			this.Patient = Patient;
			this.DOB = DOB;
			this.Finding = Finding;
			this.Recorded = Recorded;
			this.FindingDetails = FindingDetails;
			this.TestDuration = TestDuration;
			this.HeartRate = HeartRate;
		}

        public void delBLEinfo()
        {
			//delete the file
            var filename = "BLElog.txt";
			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var txtpath = Path.Combine(documentsPath, filename);
			if (checkFileExist(filename))
			{
				File.Delete(txtpath);
			}
        }

        public List<string> getBLEinfo(string devicename)
        {
			List<string> result = new List<string>();
            var filename = "BLElog.txt";
			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var txtpath = Path.Combine(documentsPath, filename);
            //check if file exists
            if (checkFileExist(filename))
            {
				//Debug.WriteLine("getBLEinfo file exists!");
                //read the file until you find devicename
				string line;
				StreamReader sr = new System.IO.StreamReader(txtpath);
				while ((line = sr.ReadLine()) != null)
				{
                    //Debug.WriteLine("line info = " + line);
                    //do stuff with line
                    if (line==devicename)
                    {
                        result.Add(line);
                        //get next 2 lines as well
                        line = sr.ReadLine();
                        //Debug.WriteLine("line info 2 = " + line);
                        result.Add(line);
                        line = sr.ReadLine();
                        //Debug.WriteLine("line info 3 = " + line);
                        result.Add(line);
                    }
				}
				sr.Close();
            }else{
                //Debug.WriteLine("getBLEinfo file does NOT exist!");
                //no info to get

            }

            return result;
        }

        public void saveBLEinfo(string devicename, int blenum, Guid deviceid)
        {
            var filename = "BLElog.txt";
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var txtpath = Path.Combine(documentsPath, filename);
            bool BLEinfoExists = false;
            //write info to file if it doesn't already exist
            if (checkFileExist(filename))
            {
                //Debug.WriteLine("saveBLEinfo file exists!");
				//read file for existing BLE info on devicename
				string line; 
				StreamReader sr = new System.IO.StreamReader(txtpath);
				while ((line = sr.ReadLine()) != null)
				{
					//do stuff with line
					if (line == devicename)
					{
                        line = sr.ReadLine();
                        if (line == blenum.ToString())
                        {
                            line = sr.ReadLine();
                            if (line == deviceid.ToString())
                            {
                                //already exists
                                BLEinfoExists = true;
                            }
                        }
					}
				}
				sr.Close();

                if (!BLEinfoExists)
                {
                    //Debug.WriteLine("appending BLE file!");
					//append to file
					using (StreamWriter sw = File.AppendText(txtpath))
					{
						sw.WriteLine(devicename);
						sw.WriteLine(blenum.ToString());
						sw.WriteLine(deviceid.ToString());
					}
                }else{
                    //Debug.WriteLine("BLE info already exists!");
                }


            }else{
                //Debug.WriteLine("saveBLEinfo file does NOT exist!");
                //Debug.WriteLine("creating and writing to BLE file!");
				//write initial lines to file
				System.IO.StreamWriter sw = new System.IO.StreamWriter(txtpath);
				sw.WriteLine(devicename);
				sw.WriteLine(blenum.ToString());
				sw.WriteLine(deviceid.ToString());
				sw.Close();
            }
        }

		public void saveTotxt(List<int> ecgList, string title, string subtitle, String fileName)
		{
			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			filePath = Path.Combine(documentsPath, fileName + ".txt");

			using (var streamWriter = new StreamWriter(filePath, true))
			{
				streamWriter.Write(Patient + ";");
				streamWriter.Write(DOB + ";");
				streamWriter.Write(Finding + ";");
				streamWriter.Write(Recorded + ";");
				streamWriter.Write(FindingDetails + ";");
				streamWriter.Write(TestDuration + ";");
				streamWriter.Write(HeartRate + ";");
				foreach (int val in ecgList)
				{
					streamWriter.Write(val);
					streamWriter.Write(";");
				}

			}
			//      await setEmailClient();
		}
		public List<string> readFromTxt(String fileName)
		{
			List<string> result = new List<string>();
			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			filePath = Path.Combine(documentsPath, fileName + ".txt");
			string readresult = null;
			using (var streamReader = new StreamReader(filePath))
			{
				readresult = streamReader.ReadToEnd();
			}
			String[] ecgdata = readresult.Split(new char[] { ';' });

			this.Patient = ecgdata[0];
			this.DOB = ecgdata[1];
			this.Finding = ecgdata[2];
			this.Recorded = ecgdata[3];
			this.FindingDetails = ecgdata[4];
			this.TestDuration = ecgdata[5];
			this.HeartRate = ecgdata[6];

			for (int i = 7; i < ecgdata.Length - 1; i++)
			{
				result.Add(ecgdata[i]);
				//write to screen
				//Debug.WriteLine("ecgdata["+i+"] = "+ecgdata[i]);
			}
			return result;
		}
		String fileName = "";
		String fileNameECG = "";
		//  #undef k;


		public byte[] saveToPdf(PlotModel ecgModel, String fileName, string name)
		{

			//Show_Dialog msg = new Show_Dialog(Xamarin.Forms.Forms.Context as Activity);
			//msg.ShowDialog("Error", "Message");

			ecgModel.Annotations.Clear();

			this.name = name;
			this.fileName = fileName + ".pdf";
			this.fileNameECG = fileName + "ECG.pdf";
			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			filePath = Path.Combine(documentsPath, fileName + ".pdf");
			Debug.WriteLine("pdf filepath: " + filePath.ToString());
			filePathNEW = Path.Combine(documentsPath, fileName + "ECG.pdf");

			using (var stream = File.Create(filePath))
			{
				var pdfExporter = new PdfExporter();
				pdfExporter.Height = 1120;
				pdfExporter.Width = 780;
				pdfExporter.Export(ecgModel, stream);
			}
			if (checkFileExist(fileName + ".txt"))
			{
				File.Delete(Path.Combine(documentsPath, fileName + ".txt"));
			}
			byte[] result = editPDF(fileName, Android.App.Application.Context);
			ShowDialog();

			return result;
		}

		public string retGif()
		{
			var assetManager = Xamarin.Forms.Forms.Context.Assets;
			string html;
			using (var streamReader = new StreamReader(assetManager.Open("gifContainer.html")))
			{
				html = streamReader.ReadToEnd();

			}
			return html;
		}

		public void copyAsset()
		{
			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			filePath = Path.Combine(documentsPath, "loading.gif");
			//Debug.WriteLine("pdf filepath: " + filePath.ToString());
			//filePathNEW = Path.Combine(documentsPath, fileName + "ECG.pdf");

			if (checkFileExist(filePath))
			{
				File.Delete(filePath);
			}
			using (var asset = Android.App.Application.Context.Assets.Open("loading.gif"))
			using (var dest = File.Create(filePath))
				asset.CopyTo(dest);
			if (checkFileExist(filePath))
			{
				//Debug.WriteLine("GIF file exists");
			}
			else 
			{
				//Debug.WriteLine("GIF file does NOT exist");
			}
			Task_vars.gifpath = filePath;
		}


		private byte[] editPDF(string fileName, Context c)
		{

			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			filePath = Path.Combine(documentsPath, fileName + ".pdf");
			filePathNEW = Path.Combine(documentsPath, fileName + "ECG.pdf");
			string oldFile = filePath;// "oldFile.pdf";
			string newFile = filePathNEW;// "newFile.pdf";

			/// open the reader
			PdfReader reader = new PdfReader(oldFile);
			iTextSharp.text.Rectangle size = reader.GetPageSizeWithRotation(1);
			Document document = new Document(size);

			// open the writer
			FileStream fs = new FileStream(newFile, FileMode.Create, FileAccess.Write);
			PdfWriter writer = PdfWriter.GetInstance(document, fs);
			document.Open();

			// the pdf content
			PdfContentByte cb = writer.DirectContent;

			// select the font properties
			BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
			//HELVETICA_BOLD
			cb.SetFontAndSize(bf, 11);

			String[] Patient_Name = Patient.Split();
			string Pat_Name = "";
			for (int i = 0; i <= Patient_Name.Length - 1; i++)
			{
				if (Patient_Name[i] != "")
				{
					Pat_Name += Patient_Name[i] + "_";
				}
			}
			//remove last _
			Pat_Name = Pat_Name.Substring(0, Pat_Name.Length - 1);
			Task_vars.patient_name = Pat_Name;
			//Task_vars.patient_name = Patient.Replace(' ','_');

			// write the text in the pdf content
			cb.BeginText();
			string text = "Patient:  " + Patient;
			cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, text, 35, 1070, 0);
			cb.EndText();

			cb.BeginText();
			text = "Finding:  " + Finding;
			cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, text, 35, 1050, 0);
			cb.EndText();

			cb.BeginText();
			text = "Heart Rate:  " + HeartRate;
			cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, text, 35, 1030, 0);
			cb.EndText();


			cb.BeginText();
			text = "DOB:   " + DOB + "                  Duration: " + TestDuration;
			cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, text, 260, 1070, 0);
			cb.EndText();


			cb.BeginText();
			text = "Finding Result Details: " + FindingDetails;
			cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, text, 260, 1050, 0);
			cb.EndText();

			cb.BeginText();
			text = "Recorded:  " + Recorded;
			cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, text, 260, 1030, 0);
			cb.EndText();

			//Stream inputImageStream = new FileStream("ICULogo.png", FileMode.Open, FileAccess.Read, FileShare.Read);


			Stream inputImageStream = c.Assets.Open("ICULogo.png");

			iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(inputImageStream);
			image.ScaleAbsolute(100, 50);
			//
			image.SetAbsolutePosition(650, 1030);
			cb.AddImage(image);


			// create the new page and add it to the pdf
			PdfImportedPage page = writer.GetImportedPage(reader, 1);
			cb.AddTemplate(page, 0, 0);

			// close the streams and voilá the file should be changed :)
			document.Close();
			fs.Close();
			writer.Close();
			reader.Close();
			File.Delete(filePath);
			Task_vars.ecgfilename = fileName + ".pdf";
			Task_vars.ecgfilepath = filePathNEW;
			FileInfo ecg_file = new FileInfo(filePathNEW);
			Task_vars.ecgfilelength = ecg_file.Length;

			//Task_vars.ecgfilename = "test.txt";
			//return SendTest();

			Debug.WriteLine("ECG pdf file path: " + filePathNEW);

			return FileRead(filePathNEW);


			//      FileUpload(filePathNEW, fileName);
		}

		public byte[] SendTest()
		{
			//string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			//string testfilePath = Path.Combine(documentsPath, "test.txt");
			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var testfilePath = Path.Combine(documentsPath, "test.txt");

			string[] lines = { "First line", "Second line" };
			System.IO.File.WriteAllLines(@testfilePath, lines);

			Task_vars.ecgfilelength = testfilePath.Length;

			Debug.WriteLine("Test file path: " + testfilePath);

			return File.ReadAllBytes(testfilePath);
		}


		public byte[] FileRead(string filepath)
		{
			//var serviceUri = Credential.BASE_URL + $"Patient/{Credential.sharedInstance.Mrn}/HomeHealth/Reading";
			//Category = "ECG";
			return File.ReadAllBytes(filepath);

		}

		public bool checkFileExist(string fileName)
		{
			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			filePath = Path.Combine(documentsPath, fileName);
			//System.Diagnostics.Debug.WriteLine("IF EXIST filePath == " + filePath);

			return File.Exists(filePath);
		}

		public static void Email(Context context, string emailTo, string emailCC, string subject, string emailText, string filelocation)
		{

			var email = new Intent(Intent.ActionSendMultiple);
			email.SetType("text/plain");
			email.PutExtra(Intent.ExtraEmail, new string[] { emailTo });
			email.PutExtra(Intent.ExtraCc, new string[] { emailCC });
            email.PutExtra(Intent.ExtraStream, Android.Net.Uri.Parse("file://" + filelocation));
			email.PutExtra(Intent.ExtraSubject, subject);
            email.PutExtra(Intent.ExtraText, emailText);
			context.StartActivity(Intent.CreateChooser(email, "Send mail..."));
		}

		//  String emailContent = "Hi  " + "\n\n\tThe above named party has sent you a copy of his most recent ECG.\n\n-- ISeeYouCare";
        //private static Page page;
		public async Task<bool> ShowDialog()
		{
            
            var popup = new EntryPopup("Email this report? If yes, please enter email address", string.Empty, "Yes", "No");
			popup.PopupClosed += (o, closedArgs) => {
                if (closedArgs.Button == "Yes")
                {
                    //use it as closedArgs.Text
                    Debug.WriteLine("my text: " + closedArgs.Text);
                    var input = closedArgs.Text.Trim();
					
                    if (input.Contains("@"))
                    {
                        sentEmail(fileNameECG, input);
                        //System.Diagnostics.Debug.WriteLine("Toast MakeText Sending email...");
#if false
                        Toast.MakeText("Sending email...")
                                 .SetType(ToastType.Info)
                                 .SetDuration(2000)
                                 .SetBgBlue(100)
                                 .SetGravity(ToastGravity.Top)
                                 .Show();
#endif
                    }
                }
				else
				{
					//cancel was pressed
				}



			};
				popup.Show();

			return true;
		}

		public async Task<bool> setEmailClient()
		{
			return await Task.Run(() =>
			{

				message = new MimeMessage();
				client = new MailKit.Net.Smtp.SmtpClient();

				if (client != null)
				{
					if (!client.IsConnected)
					{
						client.Connect("smtp.gmail.com", 465, true);
					}
				}
				return true;
			});

		}

		public Task<bool> sentToEmail(string fileName)
		{
			//this.fileName = fileName;
			this.fileNameECG = fileName;

			return ShowDialog();
		}


        //Toast toast = Toast.MakeText(Android.App.Application.Context, "Your toast message", ToastLength.Long);
		String pdfPath = null;

		async public Task<bool> sentEmail(string fileName, string addressEmail)
		{
			//System.Diagnostics.Debug.WriteLine("SendMail fileName=    " + fileName);

			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

			filePathNEW = Path.Combine(documentsPath, fileName);
			//System.Diagnostics.Debug.WriteLine(" filePathNEW  = " + filePathNEW);

			message.From.Add(new MailboxAddress("ICUCare", "icucaredonotreplay@gmail.com"));
			message.To.Add(new MailboxAddress("", addressEmail));
			message.Subject = "ECG Report";

			var builder = new BodyBuilder();
			string sayHi = "Hi  " + "\n\n\tThe above named party has sent you a copy of his most recent ECG.\n\n-- ISeeYouCare";
			// Set the plain-text version of the message text
			builder.TextBody = @sayHi;
			// We may also want to attach a calendar event for Monica's party...
			//  builder.Attachments.Add(@filePath);
			builder.Attachments.Add(@filePathNEW);

			// Now we just need to set the message body and we're done
			message.Body = builder.ToMessageBody();
			String toastText = "Your ECG Report has been sent to " + addressEmail;
			//System.Diagnostics.Debug.WriteLine(" SendMail  SmtpClient ");

			using (client)//var client = new MailKit.Net.Smtp.SmtpClient())
			{
				
                //  client.Connect("smtp.gmail.com", 587, false);
                //System.Diagnostics.Debug.WriteLine(" SendMail  Connect " + client.IsConnected);
                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.

                if (!client.IsConnected)
                {
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
                    {
                        if (Device.Idiom == TargetIdiom.Tablet)
                        {
                            var val = await DependencyService.Get<IFileHelper>().dispAlert("Email alert", "The email client connection is unsuccessful, please check wifi or data connection.", true, "OK", null);    
                        }else{
                            var val = await DependencyService.Get<IFileHelper>().dispAlert("Email alert", "The email client connection is unsuccessful, please check wifi or data connection.", false, "OK", null);
                        }

                    }));
                    /*
                    UIAlertView alert = new UIAlertView();

                    alert.AddButton("OK");
                    alert.Message = "The email client connection is unsuccessful, please check wifi or data connection.";
                    alert.AlertViewStyle = UIAlertViewStyle.Default;// = UIAlertViewStyle.PlainTextInput;
                    alert.Show();
                    return false;
                    */
                    //Display an error message
                }
                //Toast Toast = makeText();


                Toast toast = Toast.MakeText(Android.App.Application.Context, "Sending email...", ToastLength.Short);
                toast.SetGravity(Android.Views.GravityFlags.Center,0,0);
                toast.Show();
                /*
                toast.makeText("Sending email...")
                                 .SetType(ToastType.Info)
                                 .SetDuration(2000)
                                 .SetBgBlue(100)
                                 .SetGravity(ToastGravity.Center)
                                 .Show();
                */


                client.AuthenticationMechanisms.Remove("XOAUTH2");

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate("icucaredonotreplay@gmail.com", "Start12345");
                try
                {
                    //System.Diagnostics.Debug.WriteLine(" SendMail  SendAsync before ");
                    //      System.Diagnostics.Debug.WriteLine("showDialog client.Verify(input);=    " + client.Verify(addressEmail));

                    //await client.SendAsync(message);
                    await client.SendAsync(message);

                    //System.Diagnostics.Debug.WriteLine(" SendMail  SendAsync end ");
                    File.Delete(filePathNEW);
                }
                catch (SmtpCommandException ex)
                {
                    Console.WriteLine("Error sending message: {0}", ex.Message);
                    Console.WriteLine("\tStatusCode: {0}", ex.StatusCode);
                    switch (ex.ErrorCode)
                    {
                        case SmtpErrorCode.RecipientNotAccepted:
                            toastText = "Recipient not accepted: " + ex.Mailbox;
                            break;
                        case SmtpErrorCode.SenderNotAccepted:
                            toastText = "Sender not accepted: " + ex.Mailbox;
                            break;
                        case SmtpErrorCode.MessageNotAccepted:
                            toastText = "Message not accepted: " + ex.Mailbox;
                            break;
                        default:
                            toastText = "Message can not be accepted by: " + ex.Mailbox;
                            break;
                    }

                }
                catch (SmtpProtocolException ex)
                {
                    toastText = "Protocol error while sending message " + ex.Message;
                    client.Disconnect(true);
                }
                finally
                {
                    Toast toast2 = Toast.MakeText(Android.App.Application.Context, toastText, ToastLength.Short);
					toast2.SetGravity(Android.Views.GravityFlags.Center, 0, 0);

					toast2.Show();
                    /*
                    System.Diagnostics.Debug.WriteLine("Toast MakeText Sended ");
                    Toast.MakeText(toastText)
                         .SetType(ToastType.Notice)
                         .SetDuration(5000)
                         .SetBgBlue(100)
                         .SetGravity(ToastGravity.Center)
                         .Show();
                    */
                }

			}


			return true;
		}
	}


	public enum NetworkStatus
	{
		NotReachable,
		ReachableViaCarrierDataNetwork,
		ReachableViaWiFiNetwork
	}

	public static class Reachability
	{
		/*
        private static NetworkReachability _defaultRouteReachability;

        public static event EventHandler ReachabilityChanged;

        public static bool IsNetworkAvailable()
        {
            if (_defaultRouteReachability == null)
            {
                _defaultRouteReachability = new NetworkReachability(new IPAddress(0));
                _defaultRouteReachability.SetNotification(OnChange);
                _defaultRouteReachability.Schedule(CFRunLoop.Current, CFRunLoop.ModeDefault);
            }

            NetworkReachabilityFlags flags;

            return _defaultRouteReachability.TryGetFlags(out flags) &&
                IsReachableWithoutRequiringConnection(flags);
        }

        private static bool IsReachableWithoutRequiringConnection(NetworkReachabilityFlags flags)
        {
            // Is it reachable with the current network configuration?
            bool isReachable = (flags & NetworkReachabilityFlags.Reachable) != 0;

            // Do we need a connection to reach it?
            bool noConnectionRequired = (flags & NetworkReachabilityFlags.ConnectionRequired) == 0;

            // Since the network stack will automatically try to get the WAN up,
            // probe that
            if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
                noConnectionRequired = true;

            return isReachable && noConnectionRequired;
        }

        private static void OnChange(NetworkReachabilityFlags flags)
        {
            var h = ReachabilityChanged;
            if (h != null)
                h(null, EventArgs.Empty);
        }

        /*
        public bool checkFileExist(string fileName)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string filePath = Path.Combine(documentsPath, fileName);
            System.Diagnostics.Debug.WriteLine("IF EXIST filePath == " +filePath);
            System.Diagnostics.Debug.WriteLine("File.Exists(filePath); == " + File.Exists(filePath));

            return File.Exists(filePath);
        }

        public List<string> readFromTxt(string fileName)
        {
            throw new NotImplementedException();
        }

        public byte[] saveToPdf(PlotModel ecgModel, string fileName, string name)
        {
            throw new NotImplementedException();
        }

        public void saveTotxt(List<int> ecgModel, string title, string subtitle, string fileName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> sentToEmail(string fileName)
        {
            throw new NotImplementedException();
        }

        public void setEcgInof(string Patient, string DOB, string Finding, string Recorded, string FindingDetails, string HeartRate, string TestDuration = "30s")
        {
            throw new NotImplementedException();
        }

        public Task<bool> setEmailClient()
        {
            throw new NotImplementedException();
        }
        */
	}

	public class BaseUrl_Android : IBaseUrl
	{
		public string Get()
		{
			return "file:///android_asset/";
		}
	}

}
