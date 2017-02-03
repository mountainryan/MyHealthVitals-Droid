using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MyHealthVitals
{
    public class VitalsData
    {
        public VitalsData(){}

        public delegate void BpmUpdateEventHandler(int val);
        public event BpmUpdateEventHandler OnBmpChange;

        public DateTime Date
        {
            get
            {
                return DateTime.Parse(DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"));
            }
        }

        private int _bpm = 0;
        public int Bpm
        {
            get
            {
                return _bpm;
            }
            set
            {
                if (value > 0)
                {
                    _bpm = value;
                    if (OnBmpChange != null)
                        OnBmpChange(value);
                }
            }
        }


        public delegate void SpO2UpdateEventHandler(int val);
        public event SpO2UpdateEventHandler OnSpO2Change;
        private int _spo2 = 0;
        public int SpO2
        {
            get
            {
                return _spo2;
            }
            set
            {
                if (value > 0)
                {
                    _spo2 = value;
                    if (OnSpO2Change != null)
                        OnSpO2Change(value);
                }
            }
        }


        public delegate void BPSysUpdateEventHandler(int val);
        public event BPSysUpdateEventHandler OnBPSysChange;

        private int _bpsys = 0;
        public int BPSys
        {
            get
            {
                return _bpsys;
            }
            set
            {
                _bpsys = value;
                if (OnBPSysChange != null)
                    OnBPSysChange(value);
            }
        }

        public delegate void BPDiaUpdateEventHandler(int val);
        public event BPDiaUpdateEventHandler OnBPDiaChange;

        private int _bpdia = 0;
        public int BPDia
        {
            get
            {
                return _bpdia;
            }
            set
            {
                _bpdia = value;
                if (OnBPDiaChange != null)
                    OnBPDiaChange(value);
            }

        }

        public delegate void TempUpdateEventHandler(int val);
        public event TempUpdateEventHandler OnTempChange;

        private int _temp = 0;
        public int Temp
        {
            get
            {
                return _temp;
            }
            set
            {
                _temp = value;
                if (OnTempChange != null)
                    OnTempChange(value);
            }
        }
    }
}
