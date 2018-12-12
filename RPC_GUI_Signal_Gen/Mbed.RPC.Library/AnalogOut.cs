﻿// * This library is revised by Kehinde Aina, November 2016

using System;
using System.Diagnostics;

namespace org.mbed.RPC
{
    // *  This class is used to map a .NET class on to the mbed API C++ class for AnalogOut.
    // *  
    // * @author Stanislaus Eichstädt
    // * @license
    // * Copyright (c) 2010 ARM Ltd
    // *
    // *Permission is hereby granted, free of charge, to any person obtaining a copy
    // *of this software and associated documentation files (the "Software"), to deal
    // *in the Software without restriction, including without limitation the rights
    // *to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    // *copies of the Software, and to permit persons to whom the Software is
    // *furnished to do so, subject to the following conditions:
    // * <br>
    // *The above copyright notice and this permission notice shall be included in
    // *all copies or substantial portions of the Software.
    // * <br>
    // *THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    // *IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    // *FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    // *AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    // *LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    // *OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    // *THE SOFTWARE.
    public class AnalogOut
    {
	    private SerialRPC mbedRPC;
	    private String name;

		public AnalogOut(SerialRPC connectedMbed, MbedPin pin) 
        {
			//Create a new AnalogOut on mbed
			mbedRPC = connectedMbed;

            name = "mbed_" + pin.PinName.ToLower();
            String[] Args = { pin.PinName, name };
            string parameter = mbedRPC.RPC("AnalogOut", "new", Args);
			Debug.Print("New AnalogOut initialized: " + parameter);
		}

		public AnalogOut(SerialRPC connectedMbed, String ExistName)
        {
			//Tie to existing instance
			mbedRPC = connectedMbed;
			name = ExistName;
		}

		public string write(float value)
        {
			String s = value.ToString();
            s = s.Replace(',', '.');            //Hack -> replace , to .!
			String[] Args = {s};
			return mbedRPC.RPC(name, "write", Args);
		}

		public float read()
        {
			String response = mbedRPC.RPC(name, "read", null);
            response = response.Replace('.', ',');                      //Hack -> replace '.' to ','!
			//Need to convert response to and int and return
			float i = Convert.ToSingle(response);
			return(i);
		}

		public void delete()
        {
			mbedRPC.RPC(name, "delete", null);
		}
	}
}
