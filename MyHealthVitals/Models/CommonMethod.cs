using System;
using System.Threading;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public class CommonMethod
	{
		public static CommonMethod sharedInstance = new CommonMethod();

		private CommonMethod()
		{
		}


		public class MySystemDeviceTimer
		{
			private readonly TimeSpan timespan;
			private readonly Action callback;

			private CancellationTokenSource cancellation;

			public bool running { get; private set; }

			public MySystemDeviceTimer(TimeSpan timespan, Action callback)
			{
				this.timespan = timespan;
				this.callback = callback;
				this.cancellation = new CancellationTokenSource();
			}

			public void Start()
			{
				running = true;
				start(true);
			}

			private void start(bool continuous)
			{
				CancellationTokenSource cts = this.cancellation;    // safe copy
				Device.StartTimer(this.timespan,
					() =>
					{
						if (cts.IsCancellationRequested)
						{
							running = false;
							return false;
						}
						this.callback.Invoke();
						return continuous;
					});
			}

			public void FireOnce()
			{
				running = true;
				start(false);
				running = false;
			}

			public void Stop()
			{
				Interlocked.Exchange(ref this.cancellation, new CancellationTokenSource()).Cancel();
		 }
		}



		public string getExplanation(int state)
		{
			string result = "";
			switch (state)
			{
				case 0:
					result = "No irregularity found";
					break;
				case 1:
					result = "Suspected a little fast beat";
					break;
				case 2:
					result = "Supected fast beat";
					break;
				case 3:
					result = "Supected short run of fast beat";
					break;
				case 4:
					result = "Supected a little slow beat";
					break;
				case 5:
					result = "Supected slow beat";
					break;
				case 6:
					result = "Supected occasional short beat inerval";
					break;
				case 7:
					result = "Supected irregular beat interval";
					break;
				case 8:
					result = "Supected fast beat with short beat interval";
					break;
				case 9:
					result = "Supected slow beat with short beat interval";
					break;
				case 10:
					result = "Supected slow beat with irregular beat interval";
					break;
				case 11:
					result = "Waveform baselline wander";
					break;
				case 12:
					result = "Supected fast beat with baseline wander";
					break;
				case 13:
					result = "Supected slow beat with baseline wander";
					break;
				case 14:
					result = "Supected occasional short beat interal with baseline wander";
					break;
				case 15:
					result = "Supected irregular beat interval with baseline wander";
					break;
				case 16:
					result = "Poor Signal, measure again";
					break;
				default:
					break;
			}
				return result;
		}
	}
}
