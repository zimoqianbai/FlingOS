﻿#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
#endregion
    
using System;
using Kernel.FOS_System.Collections;

namespace Kernel.Hardware.Interrupts
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct InterruptDescriptor
    {
        ushort OffsetLo;
        ushort Selector;
        byte UNUSED;
        byte Type_S_DPL_P;
        ushort OffsetHi;
    }
    public class InterruptHandlers : FOS_System.Object
    {
        public List HandlerDescrips = new List(1);
    }
    public class HandlerDescriptor : FOS_System.Object
    {
        public InterruptHandler handler;
        public FOS_System.Object data;
    }
    public delegate void InterruptHandler(FOS_System.Object data);
    [Compiler.PluggedClass]
    public unsafe static class Interrupts
    {
        private static InterruptHandlers[] Handlers = new InterruptHandlers[256];

        public static void SetIRQHandler(int num, InterruptHandler handler,
                                         FOS_System.Object data)
        {
            SetISRHandler(num + 32, handler, data);
            //TODO: The following method doesn't work yet - fix the asm. (Caused double protection fault??)
            EnableIRQ((byte)num);
        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static void EnableIRQ(byte num)
        {
        }
        public static void SetISRHandler(int num, InterruptHandler handler,
                                         FOS_System.Object data)
        {
            if (Handlers[num] == null)
            {
#if DEBUG
                BasicConsole.WriteLine("Creating new InterruptHandlers...");
#endif
                Handlers[num] = new InterruptHandlers();
            }
#if DEBUG
            BasicConsole.WriteLine(((FOS_System.String)"Adding new HandlerDescriptor... ISR: ") + num);
#endif
            Handlers[num].HandlerDescrips.Add(new HandlerDescriptor()
            {
                handler = handler,
                data = data
            });
#if DEBUG
            BasicConsole.WriteLine("Added.");
#endif
        }

        private static void CommonISR(uint ISRNum)
        {
            try
            {
#if DEBUG
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine(((FOS_System.String)"ISR: ") + ISRNum);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
#endif

                InterruptHandlers handlers = Handlers[ISRNum];
                if (handlers != null)
                {
                    for (int i = 0; i < handlers.HandlerDescrips.Count; i++)
                    {
                        HandlerDescriptor descrip = (HandlerDescriptor)handlers.HandlerDescrips[i];
                        InterruptHandler func = descrip.handler;
                        func(descrip.data);
                    }
                }

                if (ISRNum >= 32 && ISRNum <= 47)
                {
                    EndIRQ(ISRNum > 39);
                }
            }
            catch
            {
#if DEBUG
                BasicConsole.SetTextColour(BasicConsole.error_colour);
                BasicConsole.WriteLine(((FOS_System.String)"Error processing ISR: ") + ISRNum);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
#endif
            }
        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static void EndIRQ(bool slave)
        {
        }

        [Compiler.PluggedMethod(ASMFilePath=@"ASM\Interrupts\Interrupts")]
        private static InterruptDescriptor* GetIDTPtr()
        {
            return null;
        }

        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static void InvokeInterrupt(uint IntNum)
        {
        }
    }
}
