﻿namespace HackF5.UnitySpy.AvaloniaGui.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using Avalonia.Controls;
    using HackF5.UnitySpy.Detail;
    using HackF5.UnitySpy.Util;
    using HackF5.UnitySpy.AvaloniaGui.Mvvm;
    using HackF5.UnitySpy.AvaloniaGui.View;
    using ReactiveUI;

    public class RawMemoryViewModel : ReactiveObject
    {        
        private ProcessFacade process;

        private int bufferSize;

        private string startAddress;

        private byte[] buffer;

        public RawMemoryViewModel(ProcessFacade process)
        {
            this.process = process;
            this.Refresh = ReactiveCommand.Create(this.StartRefresh);
            this.StartRefresh();
        }     

        public ProcessFacade Process
        {
            set {           
                if (this.process != value)     
                {
                    this.process = value;
                    this.StartRefresh();
                }
            }
        }  
                                
        public int BufferSize
        {
            get => this.bufferSize;
            set {
                if(bufferSize != value)
                {
                    this.RaiseAndSetIfChanged(ref this.bufferSize, value);    
                    this.InitBuffer();             
                    this.StartRefresh();
                }
            }
        }      
                                
        public string StartAddress
        {
            get => this.startAddress;
            set {
                if(startAddress != value)
                {
                    this.RaiseAndSetIfChanged(ref this.startAddress, value);                 
                    this.StartRefresh();
                }
            }
        }

        public ObservableCollection<string> BufferLines { get; } =
            new ObservableCollection<string>();

        public ReactiveCommand<Unit, Unit> Refresh { get; }

        private void InitBuffer()
        {
            buffer = new byte[bufferSize];
        }

        private void StartRefresh() 
        {
            Task.Run(() => this.ExecuteRefresh());
        }

        private void ExecuteRefresh()
        {
            if (this.buffer == null)
            {
                this.InitBuffer();
            }
            IntPtr startAddressPtr = new IntPtr(Convert.ToInt64(startAddress, 16));
            this.process.ReadProcessMemory(buffer, startAddressPtr, true, bufferSize);

            this.BufferLines.Clear();

            int bytesPerLine = 32;            
            for (int i = 0; i < bufferSize; i+=bytesPerLine)
            {
                IntPtr address = startAddressPtr + i;
                StringBuilder line = new StringBuilder(address.ToString("X"));
                line.Append(": ");
                StringBuilder charLine = new StringBuilder();
                for (int j = 0; j < bytesPerLine; j++)
                {
                    line.Append(buffer[i + j].ToString("X"));
                    line.Append(" ");
                    charLine.Append(ToAsciiSymbol(buffer[i + j]));
                }
                line.Append(charLine);
                this.BufferLines.Add(line.ToString());
            }
        }  

        private static char ToAsciiSymbol(byte value)
        {
            if (value < 32) return '.';  // Non-printable ASCII
            if (value < 127) return (char)value;   // Normal ASCII
            // Handle the hole in Latin-1
            if (value == 127) return '.';
            if (value < 0x90) return "€.‚ƒ„…†‡ˆ‰Š‹Œ.Ž."[ value & 0xF ];
            if (value < 0xA0) return ".‘’“”•–—˜™š›œ.žŸ"[ value & 0xF ];
            if (value == 0xAD) return '.';   // Soft hyphen: this symbol is zero-width even in monospace fonts
            return (char) value;   // Normal Latin-1
        }
    }
}
