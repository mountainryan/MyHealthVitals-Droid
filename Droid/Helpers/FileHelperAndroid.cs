using MyHealthVitals.Droid;
using System;
using System.IO;
using OxyPlot;

using System.Net;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Diagnostics;
using MimeKit;
using MailKit;
using MailKit.Net.Smtp;

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

using Android.App;
using Android.Content;

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
			//var serviceUri = Credential.BASE_URL_TEST + $"Patient/{Credential.sharedInstance.Mrn}/HomeHealth/Reading";
			//Category = "ECG";
			return File.ReadAllBytes(filepath);

		}

		public bool checkFileExist(string fileName)
		{
			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			filePath = Path.Combine(documentsPath, fileName);
			System.Diagnostics.Debug.WriteLine("IF EXIST filePath == " + filePath);

			return File.Exists(filePath);
		}
		//  String emailContent = "Hi  " + "\n\n\tThe above named party has sent you a copy of his most recent ECG.\n\n-- ISeeYouCare";

		public async Task<bool> ShowDialog()
		{
			/*
            //await setEmailClient();
            System.Diagnostics.Debug.WriteLine("showDialog");

            string message = "If yes, please enter an email address.";
            UIAlertView alert = new UIAlertView("Email this report", message, null, "NO", "YES");
            //      alert.Message =
            alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;

            alert.ShouldEnableFirstOtherButton = (UIAlertView alertView) =>
            {
                var txt = alertView.GetTextField(0).Text;
                return txt.Length > 6 && txt.Contains("@");
            };
            UITextField alertTextField = alert.GetTextField(0);
            alertTextField.KeyboardType = UIKeyboardType.EmailAddress;
            alert.Show();
            alert.Clicked += async (object s, UIButtonEventArgs ev) =>
            {
                System.Diagnostics.Debug.WriteLine("EV.BUTTON INDEX " + ev.ButtonIndex);

                if (ev.ButtonIndex == 1)
                {
                    var internetStatus = Reachability.IsNetworkAvailable();

                    System.Diagnostics.Debug.WriteLine("internetStatus" + internetStatus);
                    if (!internetStatus)
                    {
                        var alert1 = new UIAlertView()
                        {
                            Title = "No internet",
                            Message = "Please check wifi or data connection."
                        };
                        alert1.AddButton("OK");
                        alert1.AlertViewStyle = UIAlertViewStyle.Default;

                        alert1.Show();
                        return;
                    }

                    string input = alert.GetTextField(0).Text;
                    //      emailContent = alert.GetTextField(1).Text;
                    if (input.Contains("@"))
                    {
                        await sentEmail(fileNameECG, input);
                        System.Diagnostics.Debug.WriteLine("Toast MakeText Sending email...");
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
            };
            return true;
            */
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



		String pdfPath = null;

		async public Task<bool> sentEmail(string fileName, string addressEmail)
		{
			System.Diagnostics.Debug.WriteLine("SendMail fileName=    " + fileName);

			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

			filePathNEW = Path.Combine(documentsPath, fileName);
			System.Diagnostics.Debug.WriteLine(" filePathNEW  = " + filePathNEW);

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
			System.Diagnostics.Debug.WriteLine(" SendMail  SmtpClient ");

			using (client)//var client = new MailKit.Net.Smtp.SmtpClient())
			{
				/*
                //  client.Connect("smtp.gmail.com", 587, false);
                System.Diagnostics.Debug.WriteLine(" SendMail  Connect " + client.IsConnected);
                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.

                if (!client.IsConnected)
                {
                    UIAlertView alert = new UIAlertView();

                    alert.AddButton("OK");
                    alert.Message = "The email client connection is unsuccessful, please check wifi or data connection.";
                    alert.AlertViewStyle = UIAlertViewStyle.Default;// = UIAlertViewStyle.PlainTextInput;
                    alert.Show();
                    return false;
                }

                Toast.MakeText("Sending email...")
                                 .SetType(ToastType.Info)
                                 .SetDuration(2000)
                                 .SetBgBlue(100)
                                 .SetGravity(ToastGravity.Center)
                                 .Show();



                client.AuthenticationMechanisms.Remove("XOAUTH2");

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate("icucaredonotreplay@gmail.com", "Start12345");
                try
                {
                    System.Diagnostics.Debug.WriteLine(" SendMail  SendAsync before ");
                    //      System.Diagnostics.Debug.WriteLine("showDialog client.Verify(input);=    " + client.Verify(addressEmail));

                    //await client.SendAsync(message);
                    await client.SendAsync(message);

                    System.Diagnostics.Debug.WriteLine(" SendMail  SendAsync end ");
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
                    System.Diagnostics.Debug.WriteLine("Toast MakeText Sended ");
                    Toast.MakeText(toastText)
                         .SetType(ToastType.Notice)
                         .SetDuration(5000)
                         .SetBgBlue(100)
                         .SetGravity(ToastGravity.Center)
                         .Show();

                }
                */
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
